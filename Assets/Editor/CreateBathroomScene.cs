// Assets/Editor/CreateBathroomScene.cs
// Break Room -> Build Bathroom Scene
// Kúpeľňa z primitív (vaňa, WC, umývadlo+skrinka, sprcha, práčka, police,
// doplnky) – všetko rozbitné. Klon Obývačky, vlastná dlažba a svetlá.

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateBathroomScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Bathroom.unity";

    [MenuItem("BreakRoom/Art/Build Bathroom")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);
        var oldRug = GameObject.Find("rugRectangle"); if (oldRug != null) Object.DestroyImmediate(oldRug);

        // svetlá dlaždice
        Recolor("Podlaha",     new Color(0.78f, 0.80f, 0.84f));
        Recolor("Stena_Zadna", new Color(0.66f, 0.78f, 0.85f));
        Recolor("Stena_Lava",  new Color(0.70f, 0.80f, 0.86f));
        Recolor("Stena_Prava", new Color(0.70f, 0.80f, 0.86f));

        Color white   = new Color(0.92f, 0.93f, 0.95f);
        Color porc    = new Color(0.88f, 0.90f, 0.93f);
        Color glass   = new Color(0.6f, 0.72f, 0.82f);
        Color chrome   = new Color(0.72f, 0.74f, 0.8f);

        // ── VAŇA pri ľavej stene ──
        Box("Tub_body",  new Vector3(-4.3f, 0.35f, 3.3f), new Vector3(1.7f, 0.7f, 3.0f), white, 12);
        Box("Tub_inner", new Vector3(-4.3f, 0.55f, 3.3f), new Vector3(1.3f, 0.5f, 2.6f), new Color(0.8f,0.84f,0.88f), 4);
        Cyl("Tub_tap",   new Vector3(-3.7f, 0.8f, 4.6f), new Vector3(0.07f,0.18f,0.07f), Vector3.zero, chrome, 2);

        // ── WC pri zadnej stene vpravo ──
        Cyl("WC_bowl",  new Vector3(4.2f, 0.28f, 4.4f), new Vector3(0.55f,0.28f,0.6f), Vector3.zero, porc, 6);
        Box("WC_seat",  new Vector3(4.2f, 0.45f, 4.4f), new Vector3(0.62f,0.08f,0.66f), white, 3);
        Box("WC_tank",  new Vector3(4.2f, 0.75f, 5.05f), new Vector3(0.66f,0.6f,0.24f), porc, 5);
        Box("WC_lid",   new Vector3(4.2f, 1.07f, 5.05f), new Vector3(0.7f,0.06f,0.3f), white, 2);
        // držiak na papier + rolka
        Cyl("WC_roll",  new Vector3(3.3f, 0.9f, 5.2f), new Vector3(0.18f,0.1f,0.18f), new Vector3(90,0,0), white, 1);

        // ── UMÝVADLO + SKRINKA pri zadnej stene (stred) ──
        Box("Sink_cab",   new Vector3(-1.4f, 0.42f, 5.45f), new Vector3(1.4f, 0.84f, 0.7f), new Color(0.85f,0.86f,0.9f), 7);
        Box("Sink_top",   new Vector3(-1.4f, 0.86f, 5.45f), new Vector3(1.5f, 0.08f, 0.78f), white, 4);
        Cyl("Sink_basin", new Vector3(-1.4f, 0.9f, 5.4f), new Vector3(0.5f,0.06f,0.4f), Vector3.zero, porc, 3);
        Cyl("Sink_tap",   new Vector3(-1.4f, 1.0f, 5.6f), new Vector3(0.06f,0.14f,0.06f), Vector3.zero, chrome, 2);
        Box("Mirror",     new Vector3(-1.4f, 1.7f, 5.82f), new Vector3(1.1f, 0.9f, 0.05f), new Color(0.7f,0.85f,0.92f), 3);
        // kúpeľňové doplnky na pulte
        Cyl("Bottle1", new Vector3(-2.0f, 1.02f, 5.4f), new Vector3(0.1f,0.16f,0.1f), Vector3.zero, new Color(0.2f,0.6f,0.85f), 1);
        Cyl("Bottle2", new Vector3(-0.8f, 1.0f, 5.45f), new Vector3(0.09f,0.13f,0.09f), Vector3.zero, new Color(0.85f,0.35f,0.5f), 1);
        Box("SoapBox", new Vector3(-1.4f, 0.95f, 5.2f), new Vector3(0.18f,0.1f,0.12f), new Color(0.95f,0.9f,0.6f), 1);

        // ── SPRCHA pri pravej stene ──
        Box("Shower_tray", new Vector3(5.0f, 0.08f, 1.2f), new Vector3(1.6f, 0.16f, 1.8f), white, 5);
        Box("Shower_glassF", new Vector3(4.2f, 1.3f, 1.2f), new Vector3(0.06f, 2.4f, 1.8f), glass, 4);
        Box("Shower_glassS", new Vector3(5.0f, 1.3f, 0.32f), new Vector3(1.6f, 2.4f, 0.06f), glass, 4);
        Cyl("Shower_pipe", new Vector3(5.5f, 1.9f, 1.2f), new Vector3(0.05f,0.4f,0.05f), Vector3.zero, chrome, 2);
        Cyl("Shower_head", new Vector3(5.3f, 2.25f, 1.2f), new Vector3(0.22f,0.05f,0.22f), Vector3.zero, chrome, 2);

        // ── PRÁČKA pri prednej stene vľavo ──
        Box("Wash_body",  new Vector3(-4.6f, 0.5f, -3.4f), new Vector3(0.95f, 1.0f, 0.9f), new Color(0.9f,0.91f,0.93f), 8);
        Cyl("Wash_door",  new Vector3(-4.6f, 0.6f, -3.86f), new Vector3(0.5f,0.05f,0.5f), new Vector3(90,0,0), new Color(0.35f,0.5f,0.6f), 3);
        Box("Wash_panel", new Vector3(-4.6f, 1.06f, -3.5f), new Vector3(0.9f, 0.12f, 0.6f), new Color(0.3f,0.32f,0.36f), 2);
        // koš na prádlo + kôš na odpad
        Cyl("Hamper", new Vector3(-2.9f, 0.4f, -3.6f), new Vector3(0.55f,0.4f,0.55f), Vector3.zero, new Color(0.7f,0.6f,0.45f), 3);
        Cyl("Bin",    new Vector3(3.2f, 0.28f, -3.4f), new Vector3(0.4f,0.28f,0.4f), Vector3.zero, new Color(0.4f,0.6f,0.7f), 2);

        // ── POLICA + skrinka so zrkadlom pri pravej stene vzadu ──
        Box("Shelf",     new Vector3(2.4f, 1.6f, 5.78f), new Vector3(1.4f, 0.1f, 0.35f), white, 3);
        Box("ShelfItem1",new Vector3(2.0f, 1.78f, 5.78f), new Vector3(0.16f,0.24f,0.16f), new Color(0.5f,0.8f,0.6f), 1);
        Box("ShelfItem2",new Vector3(2.7f, 1.76f, 5.78f), new Vector3(0.2f,0.2f,0.16f), new Color(0.85f,0.7f,0.3f), 1);
        // uteráky na vešiaku
        Box("Towel1", new Vector3(0.6f, 1.5f, 5.82f), new Vector3(0.5f, 0.7f, 0.08f), new Color(0.9f,0.5f,0.4f), 2);
        Box("Towel2", new Vector3(1.3f, 1.5f, 5.82f), new Vector3(0.5f, 0.7f, 0.08f), new Color(0.4f,0.7f,0.85f), 2);

        // ── kúpeľňová predložka (nízke hp) + rastlina ──
        Box("Mat",   new Vector3(-1.4f, 0.03f, 4.2f), new Vector3(1.2f, 0.04f, 0.8f), new Color(0.55f,0.7f,0.65f), 1);
        Cyl("PlantPot", new Vector3(5.2f, 0.3f, -4.6f), new Vector3(0.4f,0.3f,0.4f), Vector3.zero, new Color(0.6f,0.45f,0.3f), 2);
        Box("PlantTop", new Vector3(5.2f, 0.85f, -4.6f), new Vector3(0.6f,0.7f,0.6f), new Color(0.25f,0.55f,0.3f), 2);

        // ── SVETLÁ ──
        PointLight("Glow_Ceil", new Vector3(0f, 3.4f, 1.5f), new Color(1f, 0.97f, 0.92f), 1.9f, 14f);
        PointLight("Glow_Mirror", new Vector3(-1.4f, 2.2f, 4.8f), new Color(0.9f, 0.95f, 1f), 1.2f, 7f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        { list.Add(new EditorBuildSettingsScene(DST, true)); EditorBuildSettings.scenes = list.ToArray(); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Bathroom hotová.");
    }

    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }

    static void Recolor(string name, Color c)
    {
        var go = GameObject.Find(name); if (go == null) return;
        var r = go.GetComponent<Renderer>(); if (r != null) r.sharedMaterial = Mat(c);
    }

    static GameObject Box(string n, Vector3 pos, Vector3 scale, Color col, int hp, bool breakable = true)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = n; g.transform.position = pos; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mat(col);
        if (breakable)
        {
            var rb = g.AddComponent<Rigidbody>(); rb.isKinematic = true;
            var b = g.AddComponent<Breakable>(); b.hp = Mathf.Max(1, hp); b.damage = 1; b.xpValue = 12; b.fragmentCount = 8;
        }
        return g;
    }

    static GameObject Cyl(string n, Vector3 pos, Vector3 scale, Vector3 euler, Color col, int hp, bool breakable = true)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        g.name = n; g.transform.position = pos; g.transform.localScale = scale; g.transform.eulerAngles = euler;
        g.GetComponent<Renderer>().sharedMaterial = Mat(col);
        if (breakable)
        {
            var rb = g.AddComponent<Rigidbody>(); rb.isKinematic = true;
            var b = g.AddComponent<Breakable>(); b.hp = Mathf.Max(1, hp); b.damage = 1; b.xpValue = 12; b.fragmentCount = 8;
        }
        return g;
    }

    static void PointLight(string name, Vector3 pos, Color col, float intensity, float range)
    {
        var go = new GameObject(name); go.transform.position = pos;
        var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = col; l.intensity = intensity; l.range = range;
    }
}
#endif
