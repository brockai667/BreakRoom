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

        // ── DIELŇA (factory-kit modely, rozmiestnené po 12x12 miestnosti) ──
        Place("machine",           new Vector3(-3.0f, 0f, 4.6f), new Vector3(0,90,0), 10, S);
        Place("machine-bed",       new Vector3( 3.0f, 0f, 4.6f), new Vector3(0,-90,0),10, S);
        Place("machine-fortified", new Vector3( 0.0f, 0f, 5.3f), Vector3.zero,        12, S);
        Place("hopper-round",      new Vector3(-5.2f, 0f, 2.4f), Vector3.zero,        6,  S);
        Place("hopper-square",     new Vector3( 5.2f, 0f, 2.4f), Vector3.zero,        6,  S);
        Place("hopper-high-round", new Vector3( 5.2f, 0f, 4.6f), Vector3.zero,        6,  S);
        Place("conveyor-long",     new Vector3( 0.0f, 0f, 2.2f), Vector3.zero,        8,  S);
        Place("box-large",         new Vector3(-4.6f, 0f, -1.0f),new Vector3(0,20,0), 4,  S);
        Place("box-wide",          new Vector3( 2.0f, 0f, 0.4f), new Vector3(0,-15,0),3,  S);
        Place("box-long",          new Vector3(-2.0f, 0f, -1.6f),new Vector3(0,35,0), 3,  S);
        Place("box-small",         new Vector3( 1.0f, 0f, -2.6f),new Vector3(0,10,0), 2,  S);
        Place("box-small",         new Vector3( 3.6f, 0f, -1.4f),new Vector3(0,-25,0),2,  S);
        Place("cog-a",             new Vector3(-2.6f, 0f, 1.0f), Vector3.zero,        4,  S);
        Place("cog-c",             new Vector3( 3.6f, 0f, -2.4f),Vector3.zero,        4,  S);
        Place("lever-single",      new Vector3(-5.2f, 0f, 0.0f), Vector3.zero,        2,  S);
        Place("cone",              new Vector3( 1.6f, 0f, -3.4f),Vector3.zero,        2,  S);

        // ── ATMOSFÉRA (priemyselné akcenty – emisívny pruh + farebné svetlá) ──
        PropEmissive("Stripe_Back", new Vector3(0f, 4.4f, 5.85f), new Vector3(11f, 0.18f, 0.06f), new Color(1f, 0.46f, 0.12f));
        PointLight("Glow_BackL", new Vector3(-3.5f, 3.4f, 5.2f), new Color(1f, 0.5f, 0.18f), 2.0f, 10f);
        PointLight("Glow_BackR", new Vector3( 3.5f, 3.4f, 5.2f), new Color(0.28f, 0.55f, 1f), 1.7f, 10f);
        PointLight("Glow_Mid",   new Vector3( 0f, 3.6f, 1.5f),   new Color(1f, 0.86f, 0.65f), 1.4f, 12f);

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
