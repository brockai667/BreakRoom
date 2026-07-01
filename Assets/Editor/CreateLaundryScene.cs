// Assets/Editor/CreateLaundryScene.cs
// Break Room -> Build Laundry Scene
// Práčovňa z REÁLNYCH Kenney Furniture modelov (washer/dryer/police/krabice),
// mierka kalibrovaná ako Obývačka (S=0.4). Vzor: CreateBedroomScene.

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateLaundryScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Laundry.unity";
    const float  S   = 0.4f;   // rovnaká mierka ako Obývačka

    [MenuItem("Break Room/Build Laundry Scene")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);
        var oldRug = GameObject.Find("rugRectangle"); if (oldRug != null) Object.DestroyImmediate(oldRug);

        // ── PRÁČOVŇA (layout odvodený od funkčnej Obývačky) ──
        Place("rugDoormat",          new Vector3( 0f, 0.02f, -4.9f), Vector3.zero,        1, S * 1.4f);
        // práčka + sušička pri zadnej stene, čelom do miestnosti
        Place("washer",              new Vector3(-1.0f, 0f, 5.3f),  new Vector3(0,180,0), 8, S);
        Place("dryer",               new Vector3( 1.0f, 0f, 5.3f),  new Vector3(0,180,0), 8, S);
        Place("trashcan",            new Vector3( 2.4f, 0f, 4.8f),  new Vector3(0,180,0), 2, S);
        // skladací stôl na triedenie prádla (pravá stena)
        Place("sideTable",           new Vector3( 5.4f, 0f, 2.0f),  new Vector3(0,-90,0), 4, S);
        Place("cardboardBoxOpen",    new Vector3( 5.1f, 0f, 0.4f),  new Vector3(0,-70,0), 2, S);
        // regál so saponátmi + krabice (ľavá stena)
        Place("bookcaseOpen",        new Vector3(-5.5f, 0f, -1.5f), new Vector3(0,90,0),  6, S);
        Place("cardboardBoxClosed",  new Vector3(-5.0f, 0f, -4.0f), new Vector3(0,20,0),  2, S);
        Place("cardboardBoxOpen",    new Vector3(-4.1f, 0f, -3.5f), new Vector3(0,-25,0), 2, S);
        // vešiak na vysušené oblečenie (predná stena vpravo)
        Place("coatRackStanding",    new Vector3( 4.6f, 0f, -5.0f), Vector3.zero,         3, S);
        // doplnky
        Place("plantSmall2",         new Vector3( 5.3f, 0f, -1.0f), Vector3.zero,         2, S);
        Place("lampRoundFloor",      new Vector3(-5.3f, 0f, 5.2f),  Vector3.zero,         3, S);

        // ── ATMOSFÉRA (chladné, jasné žiarivkové osvetlenie utility miestnosti) ──
        PointLight("Glow_Ceil",   new Vector3(0f, 3.4f, 1f),     new Color(0.92f, 0.96f, 1f), 2.0f, 14f);
        PointLight("Glow_Wash",   new Vector3(0f, 1.6f, 4.6f),   new Color(0.9f, 0.94f, 1f),  1.1f, 8f);
        PointLight("Glow_Fold",   new Vector3(4.8f, 1.6f, 1.5f), new Color(1f, 0.98f, 0.9f),  0.9f, 7f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        { list.Add(new EditorBuildSettingsScene(DST, true)); EditorBuildSettings.scenes = list.ToArray(); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Laundry Room (reálny nábytok, správna mierka) hotová.");
    }

    // ── HELPERY (rovnaké ako Obývačka/Bedroom) ──
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
        if (prefab == null) { Debug.LogWarning("[Laundry] model nenájdený: " + name); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = go.transform.localScale * scale;   // NÁSOB natívnu mierku
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
