// Assets/Editor/CreateBedroomScene.cs
// Break Room -> Build Bedroom Scene
// Spálňa z REÁLNYCH Kenney Furniture modelov, mierka kalibrovaná ako Obývačka (S=0.4).

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateBedroomScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Bedroom.unity";
    const float  S   = 0.4f;   // rovnaká mierka ako Obývačka

    [MenuItem("Break Room/Build Bedroom Scene")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);

        // ── SPÁLŇA (layout odvodený od funkčnej Obývačky) ──
        Place("rugRectangle",        new Vector3( 0f, 0.02f, 3.0f), Vector3.zero,        2, S * 1.7f);
        // posteľ pri zadnej stene + nočné stolíky + lampa
        Place("bedDouble",           new Vector3( 0f, 0f, 4.3f),    new Vector3(0,180,0), 9, S);
        Place("cabinetBedDrawerTable",new Vector3(-1.7f, 0f, 5.1f), new Vector3(0,180,0), 3, S);
        Place("cabinetBedDrawerTable",new Vector3( 1.7f, 0f, 5.1f), new Vector3(0,180,0), 3, S);
        Place("lampSquareTable",     new Vector3(-1.7f, 0.45f, 5.1f),Vector3.zero,        2, S);
        // komoda + TV (ľavá stena)
        Place("cabinetBedDrawer",    new Vector3(-5.4f, 0f, 2.2f),  new Vector3(0,90,0),  5, S);
        Place("computerScreen",      new Vector3(-5.2f, 0.5f, 2.2f),new Vector3(0,90,0),  3, S);
        // šatník (ľavá stena vpredu)
        Place("bookcaseClosedDoors", new Vector3(-5.4f, 0f, -1.5f), new Vector3(0,90,0),  7, S);
        // polica + knihy (pravá-zadná)
        Place("bookcaseOpen",        new Vector3( 5.4f, 0f, 3.5f),  new Vector3(0,-90,0), 6, S);
        Place("books",               new Vector3( 5.1f, 0.5f, 3.5f),new Vector3(0,-90,0), 1, S);
        // pracovný kút (ľavá-predná)
        Place("desk",                new Vector3(-5.0f, 0f, -4.4f), new Vector3(0,90,0),  5, S);
        Place("chairDesk",           new Vector3(-4.0f, 0f, -4.4f), new Vector3(0,-90,0), 4, S);
        Place("laptop",              new Vector3(-5.0f, 0.6f, -4.4f),new Vector3(0,90,0),  2, S);
        // doplnky
        Place("pottedPlant",         new Vector3(-5.3f, 0f, 5.2f),  Vector3.zero,         3, S);
        Place("plantSmall2",         new Vector3( 5.3f, 0f, -1.0f), Vector3.zero,         2, S);
        Place("coatRackStanding",    new Vector3( 4.6f, 0f, -5.0f), Vector3.zero,         3, S);
        Place("cardboardBoxClosed",  new Vector3( 3.4f, 0f, -3.4f), new Vector3(0,20,0),  2, S);
        Place("cardboardBoxOpen",    new Vector3( 4.3f, 0f, -2.6f), new Vector3(0,-25,0), 2, S);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        { list.Add(new EditorBuildSettingsScene(DST, true)); EditorBuildSettings.scenes = list.ToArray(); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Bedroom (reálny nábytok, správna mierka) hotový.");
    }

    // ── HELPERY (rovnaké ako Obývačka) ──
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
        if (prefab == null) { Debug.LogWarning("[Bedroom] model nenájdený: " + name); return null; }
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
