// Assets/Editor/CreateGarageScene.cs
// Break Room -> Build Garage Scene
// Dielňa z REÁLNYCH Kenney Factory Kit modelov, mierka násobí natívnu (ako Obývačka).

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateGarageScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Garage.unity";
    const float  S   = 1.25f;

    [MenuItem("Break Room/Build Garage Scene")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);

        // zmaž koberec z Obývačky (nerozbíjateľný, farbil podlahu)
        var oldRug = GameObject.Find("rugRectangle"); if (oldRug != null) Object.DestroyImmediate(oldRug);
        // betónová podlaha + svetlejšie steny do garáže
        Recolor("Podlaha",      new Color(0.30f, 0.30f, 0.33f));
        Recolor("Stena_Zadna",  new Color(0.50f, 0.50f, 0.54f));
        Recolor("Stena_Lava",   new Color(0.46f, 0.46f, 0.50f));
        Recolor("Stena_Prava",  new Color(0.46f, 0.46f, 0.50f));

        // ── AUTO v strede garáže (čelom k vchodu -z) ──
        BuildCar(new Vector3(0f, 0f, 0.8f), new Color(0.74f, 0.13f, 0.13f));

        // ── PRACOVNÝ PULT pri zadnej stene + náradie ──
        Box("Bench_top",  new Vector3(-3.4f, 0.92f, 5.4f), new Vector3(3.2f, 0.12f, 0.9f), new Color(0.45f,0.30f,0.16f), 8);
        Box("Bench_legL", new Vector3(-4.85f,0.46f, 5.4f), new Vector3(0.12f,0.92f,0.8f),  new Color(0.3f,0.3f,0.32f), 4);
        Box("Bench_legR", new Vector3(-1.95f,0.46f, 5.4f), new Vector3(0.12f,0.92f,0.8f),  new Color(0.3f,0.3f,0.32f), 4);
        Box("Pegboard",   new Vector3(-3.4f, 2.25f, 5.86f),new Vector3(3.0f, 1.5f, 0.05f), new Color(0.55f,0.4f,0.25f), 5);
        Box("Toolbox",    new Vector3(-2.5f, 1.12f, 5.4f), new Vector3(0.6f,0.28f,0.4f),   new Color(0.85f,0.15f,0.12f), 3);
        Box("Vice",       new Vector3(-4.3f, 1.08f, 5.35f),new Vector3(0.3f,0.25f,0.3f),   new Color(0.35f,0.4f,0.45f), 3);

        // ── PNEUMATIKY (stoh + na stene) ──
        for (int i = 0; i < 3; i++) Tire("TireStack" + i, new Vector3(5.0f, 0.18f + i * 0.4f, 4.3f));
        Tire("TireWall", new Vector3(5.78f, 1.7f, 1.0f), 90f);

        // ── SUDY S OLEJOM ──
        Drum("Drum1", new Vector3(-5.2f, 0f, -3.2f), new Color(0.72f,0.15f,0.12f));
        Drum("Drum2", new Vector3(-5.2f, 0f, -1.6f), new Color(0.15f,0.35f,0.7f));

        // ── REGÁL + skriňa na náradie (Kenney modely) ──
        Place("bookcaseOpen",     new Vector3(5.4f, 0f, 1.0f),  new Vector3(0,-90,0), 6, S);
        Place("cabinetBedDrawer", new Vector3(3.6f, 0f, 5.3f),  new Vector3(0,180,0), 5, S);
        Place("trashcan",         new Vector3(4.7f, 0f, -4.6f), Vector3.zero, 2, S);

        // ── kanister + kufrík na zemi ──
        Box("Jerrycan", new Vector3(2.3f, 0.3f, -3.2f), new Vector3(0.4f,0.6f,0.5f), new Color(0.85f,0.45f,0.1f), 2);
        Box("Toolbox2", new Vector3(-1.9f,0.18f,-2.7f), new Vector3(0.7f,0.34f,0.4f), new Color(0.15f,0.2f,0.55f), 3);

        // ── GARÁŽOVÉ DVERE (segmenty) na prednej stene ──
        for (int i = 0; i < 4; i++)
            Box("Door" + i, new Vector3(0f, 0.65f + i * 1.05f, -5.82f), new Vector3(5.2f, 1.0f, 0.08f),
                new Color(0.66f - i * 0.02f, 0.68f, 0.72f), 0, false);

        // ── TEPLÉ SVETLO dielne ──
        PointLight("Glow_Work", new Vector3(-3.4f, 3.2f, 4.5f), new Color(1f, 0.92f, 0.7f), 2.2f, 11f);
        PointLight("Glow_Car",  new Vector3(0f, 3.6f, 0.8f),    new Color(1f, 0.95f, 0.85f), 2.2f, 13f);
        PointLight("Glow_Front",new Vector3(0f, 3.0f, -4f),     new Color(0.9f, 0.85f, 0.7f), 1.4f, 12f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        { list.Add(new EditorBuildSettingsScene(DST, true)); EditorBuildSettings.scenes = list.ToArray(); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Garage (factory-kit, správna mierka) hotový.");
    }

    static void PropEmissive(string name, Vector3 pos, Vector3 scale, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.position = pos; g.transform.localScale = scale;
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 2.2f); }
        g.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(g.GetComponent<Collider>());
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

    // pneumatika = tmavý plochý valec
    static void Tire(string n, Vector3 pos, float tiltX = 0f)
    {
        Cyl(n, pos, new Vector3(0.95f, 0.18f, 0.95f), new Vector3(tiltX, 0, 0), new Color(0.10f, 0.10f, 0.11f), 4);
        Cyl(n + "_hub", pos, new Vector3(0.4f, 0.19f, 0.4f), new Vector3(tiltX, 0, 0), new Color(0.55f, 0.56f, 0.6f), 2);
    }

    // sud s olejom
    static void Drum(string n, Vector3 pos, Color col)
    {
        Cyl(n + "_body", pos + new Vector3(0, 0.62f, 0), new Vector3(0.8f, 0.62f, 0.8f), Vector3.zero, col, 5);
        Cyl(n + "_lid",  pos + new Vector3(0, 1.26f, 0), new Vector3(0.82f, 0.05f, 0.82f), Vector3.zero, new Color(0.3f,0.3f,0.32f), 2);
        Box(n + "_ring", pos + new Vector3(0, 0.62f, 0), new Vector3(0.84f, 0.08f, 0.84f), new Color(0.85f,0.8f,0.2f), 2);
    }

    // low-poly auto z primitív (karoséria, kabína, okná, kolesá, svetlá)
    static void BuildCar(Vector3 p, Color body)
    {
        Color glass  = new Color(0.55f, 0.70f, 0.85f);
        Color chrome = new Color(0.72f, 0.74f, 0.80f);
        Color dark   = new Color(0.08f, 0.08f, 0.09f);

        Box("Car_body",  p + new Vector3(0, 0.62f, 0),    new Vector3(1.9f, 0.62f, 4.0f),  body, 16);
        Box("Car_hood",  p + new Vector3(0, 0.85f, 1.35f),new Vector3(1.78f,0.16f, 1.2f),  body, 6);
        Box("Car_trunk", p + new Vector3(0, 0.86f,-1.55f),new Vector3(1.78f,0.16f, 0.9f),  body, 6);
        Box("Car_cabin", p + new Vector3(0, 1.18f,-0.15f),new Vector3(1.68f,0.58f, 1.9f),  body, 10);
        Box("Car_roof",  p + new Vector3(0, 1.49f,-0.15f),new Vector3(1.52f,0.08f, 1.7f),  body, 6);
        // okná
        Box("Car_wsF",   p + new Vector3(0, 1.20f, 0.86f),new Vector3(1.48f,0.5f, 0.07f),  glass, 3);
        Box("Car_wsB",   p + new Vector3(0, 1.20f,-1.16f),new Vector3(1.48f,0.46f,0.07f),  glass, 3);
        Box("Car_winL",  p + new Vector3(-0.83f,1.20f,-0.15f),new Vector3(0.06f,0.42f,1.5f),glass, 2);
        Box("Car_winR",  p + new Vector3( 0.83f,1.20f,-0.15f),new Vector3(0.06f,0.42f,1.5f),glass, 2);
        // nárazníky
        Box("Car_bumpF", p + new Vector3(0, 0.42f, 2.06f),new Vector3(1.9f,0.3f,0.18f), chrome, 4);
        Box("Car_bumpB", p + new Vector3(0, 0.42f,-2.06f),new Vector3(1.9f,0.3f,0.18f), chrome, 4);
        // svetlá (predné žlté, zadné červené)
        Box("Car_lhL", p + new Vector3(-0.62f,0.62f,2.06f),new Vector3(0.32f,0.2f,0.06f), new Color(1f,0.95f,0.7f), 1);
        Box("Car_lhR", p + new Vector3( 0.62f,0.62f,2.06f),new Vector3(0.32f,0.2f,0.06f), new Color(1f,0.95f,0.7f), 1);
        Box("Car_tlL", p + new Vector3(-0.62f,0.62f,-2.06f),new Vector3(0.32f,0.2f,0.06f), new Color(0.8f,0.1f,0.1f), 1);
        Box("Car_tlR", p + new Vector3( 0.62f,0.62f,-2.06f),new Vector3(0.32f,0.2f,0.06f), new Color(0.8f,0.1f,0.1f), 1);
        // kolesá (valec otočený o 90° okolo Z → os po X)
        Vector3[] wp = { new Vector3(-0.97f,0.42f,1.35f), new Vector3(0.97f,0.42f,1.35f),
                         new Vector3(-0.97f,0.42f,-1.35f), new Vector3(0.97f,0.42f,-1.35f) };
        int wi = 0;
        foreach (var w in wp)
        {
            Cyl("Car_wheel" + wi, p + w, new Vector3(0.84f, 0.16f, 0.84f), new Vector3(0,0,90), dark, 5);
            Cyl("Car_hub" + wi,   p + w + new Vector3((w.x<0? -0.02f:0.02f),0,0), new Vector3(0.42f,0.17f,0.42f), new Vector3(0,0,90), chrome, 2);
            wi++;
        }
    }

    static void PointLight(string name, Vector3 pos, Color col, float intensity, float range)
    {
        var go = new GameObject(name); go.transform.position = pos;
        var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = col; l.intensity = intensity; l.range = range;
    }

    static GameObject LoadPrefab(string name)
    {
        foreach (var g in AssetDatabase.FindAssets(name))
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            string ext = System.IO.Path.GetExtension(path).ToLower();
            if (ext != ".prefab" && ext != ".fbx") continue;
            if (System.IO.Path.GetFileNameWithoutExtension(path) != name) continue;
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go != null) return go;
        }
        return null;
    }

    static GameObject Place(string name, Vector3 pos, Vector3 euler, int hp, float scale)
    {
        var prefab = LoadPrefab(name);
        if (prefab == null) { Debug.LogWarning("[Garage] model nenájdený: " + name); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = go.transform.localScale * scale;
        EnsureCollider(go);
        var rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true;
        var bk = go.AddComponent<Breakable>(); bk.hp = Mathf.Max(1, hp); bk.damage = 1; bk.xpValue = 12; bk.fragmentCount = 8;
        return go;
    }

    static void EnsureCollider(GameObject go)
    {
        if (go.GetComponentInChildren<Collider>() != null) return;
        var rends = go.GetComponentsInChildren<Renderer>();
        var bc = go.AddComponent<BoxCollider>();
        if (rends.Length == 0) return;
        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        Vector3 ls = go.transform.lossyScale;
        bc.center = go.transform.InverseTransformPoint(b.center);
        bc.size = new Vector3(b.size.x / Mathf.Max(0.001f, ls.x),
                              b.size.y / Mathf.Max(0.001f, ls.y),
                              b.size.z / Mathf.Max(0.001f, ls.z));
    }
}
#endif
