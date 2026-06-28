// Assets/Editor/CreateWarehouseScene.cs
// Break Room -> Build Warehouse Scene
// Sklad z primitív: regály so stohmi debien, palety, sudy, vysokozdvižný
// vozík, voľné krabice – všetko rozbitné. Klon Obývačky, betón + kov.

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateWarehouseScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Warehouse.unity";

    static readonly Color CRATE  = new Color(0.62f, 0.44f, 0.22f);
    static readonly Color CRATE2 = new Color(0.70f, 0.52f, 0.28f);
    static readonly Color STEEL  = new Color(0.45f, 0.47f, 0.52f);
    static readonly Color YELLOW = new Color(0.92f, 0.74f, 0.12f);

    [MenuItem("BreakRoom/Art/Build Warehouse")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);
        var oldRug = GameObject.Find("rugRectangle"); if (oldRug != null) Object.DestroyImmediate(oldRug);

        Recolor("Podlaha",     new Color(0.32f, 0.33f, 0.35f));
        Recolor("Stena_Zadna", new Color(0.42f, 0.44f, 0.48f));
        Recolor("Stena_Lava",  new Color(0.40f, 0.42f, 0.46f));
        Recolor("Stena_Prava", new Color(0.40f, 0.42f, 0.46f));

        // ── REGÁLY pri stenách ──
        BuildRack("RackBack", new Vector3(0f, 0f, 5.5f), 0f, 5);     // zadná stena
        BuildRack("RackLeft", new Vector3(-5.5f, 0f, 0f), 90f, 4);   // ľavá stena

        // ── PALETY so stohmi debien (stred) ──
        Pallet("PalA", new Vector3(-1.5f, 0f, 1.5f), 3);
        Pallet("PalB", new Vector3( 1.8f, 0f, 2.6f), 2);
        Pallet("PalC", new Vector3( 0.4f, 0f, -1.8f), 2);

        // ── SUDY v rohu vpravo vzadu ──
        Drum("DrumA", new Vector3(4.8f, 0f, 3.2f), new Color(0.2f,0.45f,0.7f));
        Drum("DrumB", new Vector3(4.8f, 0f, 1.8f), new Color(0.7f,0.2f,0.15f));
        Drum("DrumC", new Vector3(4.0f, 0f, 2.5f), new Color(0.25f,0.55f,0.3f));

        // ── VYSOKOZDVIŽNÝ VOZÍK vpredu vľavo ──
        BuildForklift(new Vector3(-2.6f, 0f, -3.4f));

        // ── voľné krabice rozhádzané ──
        Box("Crate1", new Vector3(2.8f, 0.4f, -3.2f), new Vector3(0.8f,0.8f,0.8f), CRATE, 4);
        Box("Crate2", new Vector3(3.4f, 0.4f, -2.4f), new Vector3(0.7f,0.7f,0.7f), CRATE2, 4);
        Box("Crate3", new Vector3(3.1f, 1.1f, -3.0f), new Vector3(0.6f,0.6f,0.6f), CRATE, 3);
        Box("Crate4", new Vector3(0.2f, 0.35f, 4.0f), new Vector3(0.7f,0.7f,0.7f), CRATE2, 4);
        Box("Crate5", new Vector3(-3.6f, 0.35f, 3.6f), new Vector3(0.7f,0.7f,0.7f), CRATE, 4);

        // ── kovová skriňa + paletový vozík ──
        Box("Locker", new Vector3(5.4f, 1.0f, -3.6f), new Vector3(0.9f, 2.0f, 0.7f), STEEL, 7);
        Box("Jack",   new Vector3(1.0f, 0.18f, -4.4f), new Vector3(0.5f, 0.3f, 1.4f), new Color(0.8f,0.5f,0.1f), 4);

        // ── SVETLÁ haly ──
        PointLight("Glow_A", new Vector3(-2f, 4.0f, 2f), new Color(1f, 0.96f, 0.85f), 1.8f, 15f);
        PointLight("Glow_B", new Vector3( 2.5f, 4.0f, -1f), new Color(0.92f, 0.95f, 1f), 1.6f, 15f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        { list.Add(new EditorBuildSettingsScene(DST, true)); EditorBuildSettings.scenes = list.ToArray(); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Warehouse hotový.");
    }

    // Regál: stĺpy + police + debny na nich. rot 0 = pozdĺž X (pri zadnej stene),
    // rot 90 = pozdĺž Z (pri bočnej stene).
    static void BuildRack(string id, Vector3 c, float rotY, int bays)
    {
        bool sideways = Mathf.Abs(rotY) > 45f;
        Vector3 along = sideways ? Vector3.forward : Vector3.right; // smer regála
        float span = 10f;
        float step = span / bays;
        Vector3 start = c - along * (span * 0.5f);

        // stĺpy
        for (int i = 0; i <= bays; i++)
        {
            Vector3 p = start + along * (i * step);
            Box(id + "_post" + i, p + Vector3.up * 1.8f, sideways ? new Vector3(0.5f,3.6f,0.16f) : new Vector3(0.16f,3.6f,0.5f), STEEL, 6);
        }
        // police + debny
        float[] levels = { 1.0f, 2.1f, 3.2f };
        int li = 0;
        foreach (float y in levels)
        {
            Box(id + "_beam" + li, c + Vector3.up * y, sideways ? new Vector3(0.5f,0.1f,span) : new Vector3(span,0.1f,0.5f), new Color(0.8f,0.55f,0.15f), 5);
            for (int b = 0; b < bays; b++)
            {
                Vector3 p = start + along * (b * step + step * 0.5f);
                Color col = ((b + li) % 2 == 0) ? CRATE : CRATE2;
                Box(id + "_box" + li + "_" + b, p + Vector3.up * (y + 0.45f), new Vector3(0.8f, 0.8f, 0.8f), col, 4);
            }
            li++;
        }
    }

    // Paleta + stoh debien
    static void Pallet(string id, Vector3 pos, int boxes)
    {
        Box(id + "_pallet", pos + new Vector3(0, 0.08f, 0), new Vector3(1.3f, 0.16f, 1.3f), new Color(0.5f,0.36f,0.18f), 4);
        for (int i = 0; i < boxes; i++)
            Box(id + "_b" + i, pos + new Vector3((i%2==0?-0.3f:0.3f), 0.16f + 0.85f + i * 0.82f, 0f),
                new Vector3(0.8f, 0.8f, 0.8f), (i % 2 == 0) ? CRATE : CRATE2, 4);
    }

    static void Drum(string n, Vector3 pos, Color col)
    {
        Cyl(n + "_body", pos + new Vector3(0, 0.62f, 0), new Vector3(0.8f, 0.62f, 0.8f), Vector3.zero, col, 5);
        Cyl(n + "_lid",  pos + new Vector3(0, 1.26f, 0), new Vector3(0.82f, 0.05f, 0.82f), Vector3.zero, STEEL, 2);
        Box(n + "_ring", pos + new Vector3(0, 0.62f, 0), new Vector3(0.84f, 0.08f, 0.84f), YELLOW, 2);
    }

    // Vysokozdvižný vozík z primitív (telo, kabína, stožiar, vidlice, kolesá)
    static void BuildForklift(Vector3 p)
    {
        Color dark = new Color(0.12f, 0.12f, 0.13f);
        Box("Fork_body", p + new Vector3(0, 0.6f, 0), new Vector3(1.2f, 0.7f, 2.0f), YELLOW, 12);
        Box("Fork_seat", p + new Vector3(0, 1.05f, -0.5f), new Vector3(0.7f, 0.5f, 0.7f), new Color(0.2f,0.2f,0.22f), 4);
        Box("Fork_cage", p + new Vector3(0, 1.8f, -0.4f), new Vector3(0.1f, 1.4f, 0.1f), STEEL, 3);
        Box("Fork_cage2",p + new Vector3(0.5f, 1.8f, -0.4f), new Vector3(0.1f, 1.4f, 0.1f), STEEL, 3);
        // stožiar vpredu
        Box("Fork_mastL", p + new Vector3(-0.35f, 1.4f, 1.05f), new Vector3(0.12f, 2.6f, 0.12f), STEEL, 4);
        Box("Fork_mastR", p + new Vector3( 0.35f, 1.4f, 1.05f), new Vector3(0.12f, 2.6f, 0.12f), STEEL, 4);
        // vidlice
        Box("Fork_tineL", p + new Vector3(-0.3f, 0.18f, 1.7f), new Vector3(0.12f, 0.08f, 1.0f), new Color(0.3f,0.3f,0.32f), 3);
        Box("Fork_tineR", p + new Vector3( 0.3f, 0.18f, 1.7f), new Vector3(0.12f, 0.08f, 1.0f), new Color(0.3f,0.3f,0.32f), 3);
        // kolesá
        Cyl("Fork_wFL", p + new Vector3(-0.62f, 0.32f, 0.7f), new Vector3(0.6f,0.16f,0.6f), new Vector3(0,0,90), dark, 4);
        Cyl("Fork_wFR", p + new Vector3( 0.62f, 0.32f, 0.7f), new Vector3(0.6f,0.16f,0.6f), new Vector3(0,0,90), dark, 4);
        Cyl("Fork_wBL", p + new Vector3(-0.62f, 0.30f, -0.7f), new Vector3(0.5f,0.16f,0.5f), new Vector3(0,0,90), dark, 4);
        Cyl("Fork_wBR", p + new Vector3( 0.62f, 0.30f, -0.7f), new Vector3(0.5f,0.16f,0.5f), new Vector3(0,0,90), dark, 4);
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
