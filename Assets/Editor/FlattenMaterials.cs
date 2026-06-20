// FlattenMaterials.cs
// Umiestni do: Assets/Editor/FlattenMaterials.cs
// Menu: BreakRoom > Art > Flatten Materials...
//
// Zhodi materialy na flat low-poly look (styl One Armed Robber):
//  - vynuluje smoothness a metallic
//  - odstrani normal / metallic / occlusion / height (parallax) mapy
//  - vypne specular highlights a reflexie  -> ciste matne plochy
//  - volitelne: spravi z albedo textury jednu plnu farbu (najviac "flat")
//
// Funguje na URP Lit / Simple Lit aj na legacy Standard shader.
// RoomTheme.cs ostane funkcny, lebo farba ostava v _BaseColor / _Color.
//
// TIP: pred spustenim si commitni projekt (mas git). Operacia meni .mat assety.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlattenMaterialsWindow : EditorWindow
{
    enum Mode
    {
        ShadingOnly,        // necha albedo texturu, len zmatni povrch
        SolidFromAlbedo,    // z albedo textury vypocita priemernu farbu -> plna farba
        SolidFromBaseColor  // zmaze albedo texturu, necha aktualny _BaseColor tint
    }

    enum Scope
    {
        Selection,     // vybrane materialy / priecinky v Projecte
        Folder,        // vsetky materialy pod zadanou cestou
        OpenScenes,    // materialy pouzite v otvorenych scenach
        EntireProject  // uplne vsetky (s potvrdenim)
    }

    Mode  mode  = Mode.ShadingOnly;
    Scope scope = Scope.Selection;
    string folder = "Assets/Brick Project Studio";
    float smoothness = 0f;
    bool disableSpecular = true;
    bool disableReflections = true;
    bool keepAlbedoInShadingMode = true;
    bool dryRun = false;

    [MenuItem("BreakRoom/Art/Flatten Materials...")]
    static void Open()
    {
        var w = GetWindow<FlattenMaterialsWindow>("Flatten Materials");
        w.minSize = new Vector2(380, 360);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Flatten Materials → low-poly flat look", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Zhodi materialy na matny flat styl (One Armed Robber). " +
            "Pred spustenim odporucam commit (git). Da sa Undo, ale pri velkych davkach " +
            "je istejsie mat zalohu.", MessageType.Info);

        EditorGUILayout.Space();
        mode = (Mode)EditorGUILayout.EnumPopup(new GUIContent("Rezim",
            "ShadingOnly = len zmatni povrch (najbezpecnejsie)\n" +
            "SolidFromAlbedo = z textury spravi jednu farbu\n" +
            "SolidFromBaseColor = zmaze texturu, necha tint"), mode);

        scope = (Scope)EditorGUILayout.EnumPopup(new GUIContent("Rozsah"), scope);
        if (scope == Scope.Folder)
            folder = EditorGUILayout.TextField(new GUIContent("Priecinok", "napr. Assets/Brick Project Studio"), folder);

        EditorGUILayout.Space();
        smoothness = EditorGUILayout.Slider(new GUIContent("Smoothness",
            "0 = uplne matne. Pre sklo/kov mozes neskor rucne zvysit ~0.3."), smoothness, 0f, 1f);
        disableSpecular    = EditorGUILayout.Toggle("Vypnut specular", disableSpecular);
        disableReflections = EditorGUILayout.Toggle("Vypnut reflexie", disableReflections);

        using (new EditorGUI.DisabledScope(mode != Mode.ShadingOnly))
            keepAlbedoInShadingMode = EditorGUILayout.Toggle(
                new GUIContent("Nechat albedo texturu", "Plati len pre ShadingOnly"), keepAlbedoInShadingMode);

        EditorGUILayout.Space();
        dryRun = EditorGUILayout.Toggle(new GUIContent("Dry run (len vypis, nic nemeni)"), dryRun);

        EditorGUILayout.Space();
        var mats = CollectMaterials();
        EditorGUILayout.LabelField($"Najdenych materialov: {mats.Count}");

        using (new EditorGUI.DisabledScope(mats.Count == 0))
        {
            GUI.backgroundColor = dryRun ? Color.white : new Color(1f, 0.55f, 0.2f);
            if (GUILayout.Button(dryRun ? "Vypis (dry run)" : "FLATTEN", GUILayout.Height(34)))
                Run(mats);
            GUI.backgroundColor = Color.white;
        }
    }

    // ---- zber materialov podla rozsahu ----------------------------------
    List<Material> CollectMaterials()
    {
        var result = new HashSet<Material>();

        switch (scope)
        {
            case Scope.Selection:
            {
                foreach (var o in Selection.objects)
                {
                    if (o is Material m) { result.Add(m); continue; }
                    var path = AssetDatabase.GetAssetPath(o);
                    if (AssetDatabase.IsValidFolder(path))
                        AddFromGuids(AssetDatabase.FindAssets("t:Material", new[] { path }), result);
                }
                // ak su vybrane objekty v scene, vezmi aj ich materialy
                foreach (var go in Selection.gameObjects)
                    foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                        foreach (var sm in r.sharedMaterials)
                            if (sm != null) result.Add(sm);
                break;
            }
            case Scope.Folder:
            {
                if (AssetDatabase.IsValidFolder(folder))
                    AddFromGuids(AssetDatabase.FindAssets("t:Material", new[] { folder }), result);
                break;
            }
            case Scope.OpenScenes:
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (!s.isLoaded) continue;
                    foreach (var root in s.GetRootGameObjects())
                        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                            foreach (var sm in r.sharedMaterials)
                                if (sm != null) result.Add(sm);
                }
                break;
            }
            case Scope.EntireProject:
                AddFromGuids(AssetDatabase.FindAssets("t:Material"), result);
                break;
        }

        return result.Where(IsProcessable).ToList();
    }

    static void AddFromGuids(string[] guids, HashSet<Material> set)
    {
        foreach (var g in guids)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
            if (m != null) set.Add(m);
        }
    }

    // preskoc UI / text / particles / skybox / hidden shadery
    static bool IsProcessable(Material m)
    {
        if (m == null || m.shader == null) return false;
        string s = m.shader.name;
        return !(s.Contains("TextMeshPro") || s.StartsWith("UI/") || s.Contains("Sprites/")
              || s.Contains("Particles") || s.Contains("Skybox") || s.StartsWith("Hidden/"));
    }

    // ---- vykonanie -------------------------------------------------------
    void Run(List<Material> mats)
    {
        if (scope == Scope.EntireProject && !dryRun &&
            !EditorUtility.DisplayDialog("Flatten – cely projekt?",
                $"Toto zmeni {mats.Count} materialov v celom projekte. Mas commit/zalohu?",
                "Pokracovat", "Zrusit"))
            return;

        int done = 0;
        if (!dryRun) Undo.RegisterCompleteObjectUndo(mats.ToArray(), "Flatten Materials");

        try
        {
            for (int i = 0; i < mats.Count; i++)
            {
                var m = mats[i];
                if (EditorUtility.DisplayCancelableProgressBar("Flatten Materials",
                        m.name, (float)i / Mathf.Max(1, mats.Count)))
                    break;

                if (dryRun) { Debug.Log($"[Flatten] by upravil: {m.name}  ({m.shader.name})", m); done++; continue; }

                Flatten(m);
                EditorUtility.SetDirty(m);
                done++;
            }
        }
        finally { EditorUtility.ClearProgressBar(); }

        if (!dryRun) AssetDatabase.SaveAssets();
        Debug.Log($"[Flatten] Hotovo: {done}/{mats.Count} materialov ({mode}, smoothness={smoothness}).");
    }

    void Flatten(Material m)
    {
        // 1) zmatni povrch
        SetFloatIfHas(m, "_Smoothness", smoothness);
        SetFloatIfHas(m, "_Glossiness", smoothness); // Standard shader
        SetFloatIfHas(m, "_Metallic", 0f);

        // 2) preč s PBR mapami
        ClearTex(m, "_BumpMap");           m.DisableKeyword("_NORMALMAP");
        ClearTex(m, "_DetailNormalMap");
        ClearTex(m, "_MetallicGlossMap");  m.DisableKeyword("_METALLICSPECGLOSSMAP");
        ClearTex(m, "_SpecGlossMap");       m.DisableKeyword("_SPECGLOSSMAP");
        ClearTex(m, "_OcclusionMap");      m.DisableKeyword("_OCCLUSIONMAP");
        ClearTex(m, "_ParallaxMap");       m.DisableKeyword("_PARALLAXMAP");

        // 3) ploche tienenie – preč s odleskmi a odrazmi prostredia
        if (disableSpecular)    m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        if (disableReflections)
        {
            m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); // URP
            m.EnableKeyword("_GLOSSYREFLECTIONS_OFF");      // Standard
        }

        // 4) farba podla rezimu
        if (mode == Mode.ShadingOnly)
        {
            if (!keepAlbedoInShadingMode) RemoveAlbedo(m);
        }
        else if (mode == Mode.SolidFromBaseColor)
        {
            RemoveAlbedo(m); // necha aktualny tint
        }
        else // SolidFromAlbedo
        {
            var tex = GetAlbedo(m) as Texture2D;
            Color tint = GetBaseColor(m);
            Color avg = tex != null ? AverageColor(GetAlbedo(m)) : tint;
            SetBaseColor(m, new Color(avg.r * tint.r, avg.g * tint.g, avg.b * tint.b, tint.a));
            RemoveAlbedo(m);
        }
    }

    // ---- helpery na vlastnosti ------------------------------------------
    static void SetFloatIfHas(Material m, string p, float v) { if (m.HasProperty(p)) m.SetFloat(p, v); }
    static void ClearTex(Material m, string p) { if (m.HasProperty(p)) m.SetTexture(p, null); }

    static Texture GetAlbedo(Material m)
    {
        if (m.HasProperty("_BaseMap")) return m.GetTexture("_BaseMap");
        if (m.HasProperty("_MainTex")) return m.GetTexture("_MainTex");
        return null;
    }
    static void RemoveAlbedo(Material m)
    {
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", null);
        if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", null);
    }
    static Color GetBaseColor(Material m)
    {
        if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
        if (m.HasProperty("_Color"))     return m.GetColor("_Color");
        return Color.white;
    }
    static void SetBaseColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color"))     m.SetColor("_Color", c);
    }

    // priemerna farba textury cez maly RenderTexture (funguje aj na non-readable)
    static Color AverageColor(Texture src)
    {
        if (src == null) return Color.gray;
        const int S = 16;
        var rt = RenderTexture.GetTemporary(S, S, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, S, S), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        var px = tex.GetPixels();
        Object.DestroyImmediate(tex);
        double r = 0, g = 0, b = 0; int n = 0;
        foreach (var c in px) { if (c.a < 0.1f) continue; r += c.r; g += c.g; b += c.b; n++; }
        if (n == 0) return Color.gray;
        return new Color((float)(r / n), (float)(g / n), (float)(b / n), 1f);
    }
}
#endif
