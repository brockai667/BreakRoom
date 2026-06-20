// FlattenAuto.cs
// Umiestni do: Assets/Editor/FlattenAuto.cs
// 1-KLIK automatika nad logikou FlattenMaterials – netreba nastavovat okno.
//
// Menu: BreakRoom > Art > 1x Klik: ...
//  - "Sploast BPS (Shading)"      -> najbezpecnejsie: zmatni + zrus mapy/odlesky, FARBY necha
//  - "Sploast BPS (Plne farby)"   -> z textur spravi jednu plnu farbu (najviac One Armed Robber)
//  - "Sploast VSETKO (okrem Kenney)" -> chyti aj ine realisticke materialy mimo BPS
//
// Pred zasahom sa pyta na potvrdenie a registruje Undo. Da sa vratit (Ctrl+Z) aj cez git.
// Headless (CI / cowork bez kliku):
//   Unity.exe -batchmode -quit -projectPath "C:/Users/damia/BreakRoom" -executeMethod FlattenAuto.BatchFlattenBpsShading

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class FlattenAuto
{
    const string BpsFolder = "Assets/Brick Project Studio";

    // priecinky ktore su uz flat (Kenney) – tie pri "VSETKO" preskakujeme
    static readonly string[] SkipFolders =
    {
        "Assets/KenneyFurniture",
        "Assets/kenney_factory-kit_3.0",
    };

    // ---- MENU (klikacie) -------------------------------------------------

    [MenuItem("BreakRoom/Art/1x Klik: Sploast BPS (Shading)", priority = 20)]
    static void MenuBpsShading()
    {
        var mats = MaterialsInFolders(new[] { BpsFolder });
        RunInteractive(mats, solid: false,
            $"Sploastit {mats.Count} materialov v '{BpsFolder}' (Shading – farby ostanu)?");
    }

    [MenuItem("BreakRoom/Art/1x Klik: Sploast BPS (Plne farby)", priority = 21)]
    static void MenuBpsSolid()
    {
        var mats = MaterialsInFolders(new[] { BpsFolder });
        RunInteractive(mats, solid: true,
            $"Sploastit {mats.Count} materialov v '{BpsFolder}' na PLNE FARBY (z textur spravi 1 farbu)?");
    }

    [MenuItem("BreakRoom/Art/1x Klik: Sploast VSETKO (okrem Kenney)", priority = 40)]
    static void MenuAllNonKenney()
    {
        var mats = AllProjectMaterials(excludeFolders: SkipFolders);
        RunInteractive(mats, solid: false,
            $"Sploastit {mats.Count} materialov v celom projekte (okrem Kenney, Shading)?");
    }

    // ---- HEADLESS (cez -executeMethod) -----------------------------------

    public static void BatchFlattenBpsShading()
    {
        var mats = MaterialsInFolders(new[] { BpsFolder });
        int n = RunSilent(mats, solid: false);
        Debug.Log($"[FlattenAuto] BATCH Shading hotovo: {n} materialov v {BpsFolder}.");
    }

    public static void BatchFlattenBpsSolid()
    {
        var mats = MaterialsInFolders(new[] { BpsFolder });
        int n = RunSilent(mats, solid: true);
        Debug.Log($"[FlattenAuto] BATCH Plne farby hotovo: {n} materialov v {BpsFolder}.");
    }

    // ---- jadro -----------------------------------------------------------

    static void RunInteractive(List<Material> mats, bool solid, string question)
    {
        if (mats.Count == 0)
        {
            EditorUtility.DisplayDialog("Flatten", "Nenasiel som ziadne materialy na spracovanie.", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Flatten – potvrdenie",
                question + "\n\nTip: maj commit/zalohu. Da sa vratit cez Undo aj git.",
                "Sploastit", "Zrusit"))
            return;

        int done = RunSilent(mats, solid);
        EditorUtility.DisplayDialog("Flatten – hotovo",
            $"Sploastenych {done} materialov ({(solid ? "Plne farby" : "Shading")}).\n" +
            "Pozri scenu. Ak treba este plochejsie, pusti verziu 'Plne farby'.", "Super");
    }

    static int RunSilent(List<Material> mats, bool solid)
    {
        if (mats.Count == 0) return 0;
        Undo.RegisterCompleteObjectUndo(mats.ToArray(), "Flatten Materials (Auto)");

        int done = 0;
        try
        {
            for (int i = 0; i < mats.Count; i++)
            {
                var m = mats[i];
                if (EditorUtility.DisplayCancelableProgressBar("Flatten (Auto)",
                        m.name, (float)i / Mathf.Max(1, mats.Count)))
                    break;
                Flatten(m, solid);
                EditorUtility.SetDirty(m);
                done++;
            }
        }
        finally { EditorUtility.ClearProgressBar(); }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FlattenAuto] Hotovo: {done}/{mats.Count} materialov ({(solid ? "SolidFromAlbedo" : "ShadingOnly")}).");
        return done;
    }

    static void Flatten(Material m, bool solid)
    {
        // 1) zmatni povrch
        SetFloatIfHas(m, "_Smoothness", 0f);
        SetFloatIfHas(m, "_Glossiness", 0f); // Standard
        SetFloatIfHas(m, "_Metallic", 0f);

        // 2) prec s PBR mapami
        ClearTex(m, "_BumpMap");           m.DisableKeyword("_NORMALMAP");
        ClearTex(m, "_DetailNormalMap");
        ClearTex(m, "_MetallicGlossMap");  m.DisableKeyword("_METALLICSPECGLOSSMAP");
        ClearTex(m, "_SpecGlossMap");      m.DisableKeyword("_SPECGLOSSMAP");
        ClearTex(m, "_OcclusionMap");      m.DisableKeyword("_OCCLUSIONMAP");
        ClearTex(m, "_ParallaxMap");       m.DisableKeyword("_PARALLAXMAP");

        // 3) ploche tienenie – prec s odleskmi a reflexiami
        m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); // URP
        m.EnableKeyword("_GLOSSYREFLECTIONS_OFF");      // Standard

        // 4) farba
        if (solid)
        {
            var tex = GetAlbedo(m);
            Color tint = GetBaseColor(m);
            Color avg = tex != null ? AverageColor(tex) : tint;
            SetBaseColor(m, new Color(avg.r * tint.r, avg.g * tint.g, avg.b * tint.b, tint.a));
            RemoveAlbedo(m);
        }
        // ShadingOnly: albedo texturu necha tak
    }

    // ---- zber materialov -------------------------------------------------

    static List<Material> MaterialsInFolders(string[] folders)
    {
        var valid = folders.Where(AssetDatabase.IsValidFolder).ToArray();
        var set = new HashSet<Material>();
        if (valid.Length > 0)
            foreach (var g in AssetDatabase.FindAssets("t:Material", valid))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                if (m != null) set.Add(m);
            }
        return set.Where(IsProcessable).ToList();
    }

    static List<Material> AllProjectMaterials(string[] excludeFolders)
    {
        var set = new HashSet<Material>();
        foreach (var g in AssetDatabase.FindAssets("t:Material"))
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (excludeFolders.Any(f => path.StartsWith(f + "/") || path == f)) continue;
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) set.Add(m);
        }
        return set.Where(IsProcessable).ToList();
    }

    // preskoc UI / text / particles / skybox / hidden shadery
    static bool IsProcessable(Material m)
    {
        if (m == null || m.shader == null) return false;
        string s = m.shader.name;
        return !(s.Contains("TextMeshPro") || s.StartsWith("UI/") || s.Contains("Sprites/")
              || s.Contains("Particles") || s.Contains("Skybox") || s.StartsWith("Hidden/"));
    }

    // ---- helpery ---------------------------------------------------------

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

    // priemerna farba textury (funguje aj na non-readable cez RenderTexture)
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
