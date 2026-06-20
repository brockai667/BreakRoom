// Assets/Editor/MainMenuArt.cs
// BreakRoom -> Art -> Build Main Menu 3D (Hangar)
// Prerobí Main Menu na 3D scénu: industriálny hangár z factory-kit modelov,
// pódium s pomaly rotujúcou zbraňou, kinematická kamera, vypne 2D tehlu.

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MainMenuArt
{
    const string SCENE = "Assets/Scenes/MainMenu.unity";
    const string FAC   = "Assets/kenney_factory-kit_3.0/Models/FBX format/";
    const float  S     = 1.25f;
    const string ROOT  = "MenuArt3D";

    [MenuItem("BreakRoom/Art/Build Main Menu 3D (Hangar)", priority = 5)]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SCENE, OpenSceneMode.Single);

        // odstráň starý art (ak bežal znova) + vypni 2D tehlové pozadie
        var old = GameObject.Find(ROOT); if (old != null) Object.DestroyImmediate(old);
        var bg = GameObject.Find("BackgroundCanvas"); if (bg != null) bg.SetActive(false);

        var root = new GameObject(ROOT);

        // ── PODLAHA + STENY HANGÁRA ──
        Material concrete = Mat(new Color(0.20f, 0.21f, 0.24f));
        Material wallMat  = Mat(new Color(0.28f, 0.30f, 0.34f));
        Material ceilMat  = Mat(new Color(0.16f, 0.17f, 0.19f));
        Box(root, "Floor",   new Vector3(0, -0.05f, 5f), new Vector3(34, 0.1f, 34), concrete);
        Box(root, "WallBack", new Vector3(0, 4.5f, 13f),  new Vector3(28, 9f, 0.5f), wallMat);
        Box(root, "WallL",   new Vector3(-11f, 4.5f, 4f), new Vector3(0.5f, 9f, 26f), wallMat);
        Box(root, "WallR",   new Vector3( 11f, 4.5f, 4f), new Vector3(0.5f, 9f, 26f), wallMat);
        Box(root, "Ceiling", new Vector3(0, 9f, 4f),      new Vector3(28, 0.5f, 30), ceilMat);
        // akcentové pruhy na zadnej stene (oranžová žiara)
        Box(root, "StripeTop", new Vector3(0, 7.6f, 12.7f), new Vector3(26, 0.25f, 0.1f), Emissive(new Color(1f, 0.46f, 0.14f)));
        Box(root, "StripeBot", new Vector3(0, 1.4f, 12.7f), new Vector3(26, 0.18f, 0.1f), Emissive(new Color(0.9f, 0.4f, 0.1f)));

        // ── FACTORY PROPS (pozadie za pódiom) ──
        Spawn("door-wide-open",  new Vector3( 0f, 0f, 12.4f), 0f);
        Spawn("machine",         new Vector3(-5.5f, 0f, 8.5f), 60f);
        Spawn("machine-bed",     new Vector3( 5.5f, 0f, 8.5f), -60f);
        Spawn("machine-fortified",new Vector3(-8f, 0f, 6.5f), 90f);
        Spawn("hopper-round",    new Vector3( 8f, 0f, 6.5f), 0f);
        Spawn("hopper-high-round",new Vector3( 9f, 0f, 9.5f), 0f);
        Spawn("hopper-square",   new Vector3(-9f, 0f, 9.5f), 0f);
        Spawn("conveyor-long",   new Vector3( 0f, 0f, 7.0f), 90f);
        Spawn("crane",           new Vector3( 6.5f, 0f, 11.5f), -30f);
        Spawn("box-large",       new Vector3(-7.5f, 0f, 3.5f), 18f);
        Spawn("box-wide",        new Vector3( 7.5f, 0f, 3.5f), -22f);
        Spawn("box-long",        new Vector3(-6f, 0f, 4.8f), 40f);
        Spawn("cog-a",           new Vector3( 4f, 0f, 5.0f), 0f);
        Spawn("cog-c",           new Vector3(-3.5f, 0f, 5.2f), 0f);
        Spawn("cone",            new Vector3( 2.4f, 0f, 2.6f), 0f);
        Spawn("cone",            new Vector3(-2.4f, 0f, 2.6f), 0f);

        // ── PÓDIUM + ZBRAŇ (hero vľavo, tlačidlá idú doprava) ──
        float px = -1.9f;   // hero zbraň v ľavej časti scény
        Cyl(root, "PodiumBase", new Vector3(px, 0.12f, 0), new Vector3(2.8f, 0.12f, 2.8f), Mat(new Color(0.10f, 0.11f, 0.14f)));
        Cyl(root, "PodiumStep", new Vector3(px, 0.26f, 0), new Vector3(2.1f, 0.10f, 2.1f), Mat(new Color(0.16f, 0.17f, 0.21f)));
        Cyl(root, "PodiumRing", new Vector3(px, 0.33f, 0), new Vector3(2.2f, 0.02f, 2.2f), Emissive(new Color(0.95f, 0.42f, 0.1f)));

        var weapon = WeaponPreview.BuildModel(WeaponData.Get("sledge"));
        weapon.name = "MenuWeapon";
        weapon.transform.SetParent(root.transform, false);
        weapon.transform.localPosition = new Vector3(px, 1.75f, 0f);
        weapon.transform.localScale = Vector3.one * 1.95f;
        weapon.transform.localEulerAngles = new Vector3(0f, 0f, 18f);
        weapon.AddComponent<MenuSpin>();

        // ── SVETLO ──
        var dir = Object.FindFirstObjectByType<Light>();
        if (dir != null && dir.type == LightType.Directional)
        {
            dir.color = new Color(0.65f, 0.72f, 0.85f);
            dir.intensity = 0.85f;
            dir.transform.rotation = Quaternion.Euler(55f, -150f, 0f);
        }
        // bodový reflektor na zbraň
        var spotGO = new GameObject("PodiumSpot"); spotGO.transform.SetParent(root.transform, false);
        spotGO.transform.position = new Vector3(px, 4.2f, -0.6f);
        spotGO.transform.rotation = Quaternion.LookRotation((new Vector3(px, 1.6f, 0) - spotGO.transform.position).normalized);
        var spot = spotGO.AddComponent<Light>();
        spot.type = LightType.Spot; spot.spotAngle = 55f; spot.range = 10f;
        spot.color = new Color(1f, 0.93f, 0.78f); spot.intensity = 8f;
        // teplé + farebné akcenty v pozadí
        Point(root, "GlowL", new Vector3(-7f, 3f, 9f),  new Color(1f, 0.55f, 0.2f), 2.2f, 12f);
        Point(root, "GlowR", new Vector3( 7f, 3f, 9f),  new Color(0.3f, 0.6f, 1f), 1.8f, 12f);
        Point(root, "GlowBack", new Vector3(0f, 4f, 12f), new Color(1f, 0.46f, 0.14f), 2.0f, 12f);

        RenderSettings.ambientLight = new Color(0.30f, 0.31f, 0.35f);

        // ── KAMERA ──
        var cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f);
            cam.fieldOfView = 54f;
            foreach (var old2 in cam.GetComponents<MenuCameraPan>()) Object.DestroyImmediate(old2);
            var pan = cam.gameObject.AddComponent<MenuCameraPan>();
            pan.focus = new Vector3(-0.5f, 1.1f, 0f);  // mierne doľava — hero v ľavej tretine
            pan.radius = 6.0f;
            pan.height = 2.5f;
            pan.sweepDeg = 9f;                          // menší výkyv, zbraň ostáva vľavo
            pan.sweepSpeed = 0.1f;
        }

        // ── PRESUN UI TLAČIDIEL DOPRAVA (AAA layout: hero vľavo, menu vpravo) ──
        RepositionUI();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Main Menu 3D (hangár + zbraň na pódiu) hotové.");
    }

    // Presunie hlavné menu do pravého stĺpca, titul ako banner hore.
    static void RepositionUI()
    {
        // titul — banner hore v strede
        MoveUI("TitleText", new Vector2(0f, 360f), Vector2.zero, false);

        // pravý vertikálny stĺpec (anchor stred): PLAY / [COLLECTION runtime] / QUIT
        float rx = 540f; Vector2 bs = new Vector2(460f, 90f);
        MoveUI("PlayButton", new Vector2(rx, 120f), bs, true);
        MoveUI("QuitButton", new Vector2(rx, -100f), bs, true);
        // staré nepoužívané buttony odprac z cesty (ak sú aktívne)
        MoveUI("ShopButton", new Vector2(3000f, 0f), Vector2.zero, false);
        MoveUI("CollectionButton", new Vector2(3000f, 0f), Vector2.zero, false);

        // prémiový jednotný štýl (zaoblené + oranžový akcent)
        StyleMenuBtn("PlayButton");
        StyleMenuBtn("QuitButton");
    }

    // Zjednotí vzhľad menu tlačidla: zaoblené tmavé pozadie + oranžový spodný akcent + tieň.
    static void StyleMenuBtn(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) return;
        var img = go.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = UITheme.Rounded(18); img.type = Image.Type.Sliced;
            img.color = new Color(0.13f, 0.14f, 0.18f, 0.97f);
        }
        var btn = go.GetComponent<Button>();
        if (btn != null && img != null) UITheme.Hover(btn, img.color, img.color);

        if (go.transform.Find("Accent") == null)
        {
            var acc = new GameObject("Accent"); acc.transform.SetParent(go.transform, false);
            var ai = acc.AddComponent<Image>(); ai.sprite = UITheme.Rounded(4); ai.type = Image.Type.Sliced;
            ai.color = UITheme.Accent; ai.raycastTarget = false;
            var ar = acc.GetComponent<RectTransform>();
            ar.anchorMin = new Vector2(0.08f, 0f); ar.anchorMax = new Vector2(0.92f, 0f); ar.pivot = new Vector2(0.5f, 0f);
            ar.sizeDelta = new Vector2(0, 6); ar.anchoredPosition = new Vector2(0, 9);
        }
        UITheme.Shadow(go, new Vector2(0, -4), 0.5f);
    }

    static void MoveUI(string name, Vector2 pos, Vector2 size, bool setSize)
    {
        var go = GameObject.Find(name);
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        if (setSize) rt.sizeDelta = size;
    }

    // ── HELPERY ──
    static GameObject Spawn(string fbx, Vector3 pos, float yRot)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(FAC + fbx + ".fbx");
        if (model == null) { Debug.LogWarning("[MenuArt] chýba: " + fbx); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        go.name = "BG_" + fbx;
        go.transform.SetParent(GameObject.Find(ROOT).transform, true);
        go.transform.position = pos;
        go.transform.eulerAngles = new Vector3(0f, yRot, 0f);
        go.transform.localScale = go.transform.localScale * S;
        return go;
    }

    static GameObject Box(GameObject parent, string name, Vector3 pos, Vector3 scale, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent.transform, false);
        g.transform.position = pos; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }
    static GameObject Cyl(GameObject parent, string name, Vector3 pos, Vector3 scale, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        g.name = name; g.transform.SetParent(parent.transform, false);
        g.transform.position = pos; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }
    static void Point(GameObject parent, string name, Vector3 pos, Color col, float intensity, float range)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        go.transform.position = pos;
        var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = col; l.intensity = intensity; l.range = range;
    }
    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
    static Material Emissive(Color c)
    {
        var m = Mat(c);
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 2.2f); }
        return m;
    }
}
#endif
