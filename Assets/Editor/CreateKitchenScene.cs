// Assets/Editor/CreateKitchenScene.cs
// Break Room -> Build Kitchen Scene
// Kuchyňa z REÁLNYCH Kenney Furniture modelov, mierka kalibrovaná ako Obývačka (S=0.4).
// Vzor: CreateBedroomScene — otvor funkčnú Obývačku, ulož ako Kitchen, zmaž staré
// breakables a polož kuchynský nábytok (zachová hráča, GM, HUD, pauzu, svetlo, steny).

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateKitchenScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Kitchen.unity";
    const float  S   = 0.4f;   // rovnaká mierka ako Obývačka

    [MenuItem("Break Room/Build Kitchen Scene")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        // zmaž starý nábytok (necháme hráča, kameru, GM, HUD, steny, svetlo)
        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);

        // ── KUCHYNSKÁ LINKA pri zadnej stene (z≈+5.3, čelom do miestnosti -z) ──
        Place("kitchenCabinetCornerInner", new Vector3(-5.2f, 0f, 5.2f), new Vector3(0,180,0), 6, S);
        Place("kitchenSink",               new Vector3(-3.7f, 0f, 5.3f), new Vector3(0,180,0), 5, S);
        Place("kitchenCabinetDrawer",      new Vector3(-2.3f, 0f, 5.3f), new Vector3(0,180,0), 4, S);
        Place("kitchenStove",              new Vector3(-0.8f, 0f, 5.3f), new Vector3(0,180,0), 6, S);
        Place("kitchenCabinet",            new Vector3( 0.7f, 0f, 5.3f), new Vector3(0,180,0), 4, S);
        Place("kitchenCabinetDrawer",      new Vector3( 2.2f, 0f, 5.3f), new Vector3(0,180,0), 4, S);
        Place("kitchenFridgeLarge",        new Vector3( 4.4f, 0f, 5.0f), new Vector3(0,180,0), 8, S);

        // horné skrinky + digestor (y≈1.45)
        Place("kitchenCabinetUpperDouble", new Vector3(-3.0f, 1.45f, 5.5f), new Vector3(0,180,0), 3, S);
        Place("hoodModern",                new Vector3(-0.8f, 1.55f, 5.5f), new Vector3(0,180,0), 3, S);
        Place("kitchenCabinetUpperDouble", new Vector3( 1.4f, 1.45f, 5.5f), new Vector3(0,180,0), 3, S);

        // malé spotrebiče na linke (y≈0.55)
        Place("kitchenCoffeeMachine",      new Vector3(-2.6f, 0.55f, 5.2f), new Vector3(0,180,0), 2, S);
        Place("kitchenBlender",            new Vector3(-2.0f, 0.55f, 5.2f), new Vector3(0,180,0), 2, S);
        Place("kitchenMicrowave",          new Vector3( 0.7f, 0.55f, 5.2f), new Vector3(0,180,0), 3, S);
        Place("toaster",                   new Vector3( 2.2f, 0.55f, 5.2f), new Vector3(0,180,0), 2, S);

        // ── BOČNÁ LINKA pri ľavej stene (x≈-5.3, čelom +x) ──
        Place("kitchenCabinetDrawer",      new Vector3(-5.3f, 0f, 2.8f), new Vector3(0,90,0), 4, S);
        Place("kitchenCabinet",            new Vector3(-5.3f, 0f, 1.3f), new Vector3(0,90,0), 4, S);
        Place("kitchenCabinetUpper",       new Vector3(-5.4f, 1.45f, 2.8f), new Vector3(0,90,0), 3, S);

        // ── KUCHYNSKÝ OSTROV (stred) + barové stoličky ──
        Place("kitchenBarEnd",             new Vector3(-1.5f, 0f, 1.6f), new Vector3(0,0,0),   4, S);
        Place("kitchenBar",                new Vector3(-0.6f, 0f, 1.6f), new Vector3(0,0,0),   4, S);
        Place("kitchenBar",                new Vector3( 0.6f, 0f, 1.6f), new Vector3(0,0,0),   4, S);
        Place("kitchenBarEnd",             new Vector3( 1.5f, 0f, 1.6f), new Vector3(0,180,0), 4, S);
        Place("stoolBar",                  new Vector3(-0.8f, 0f, 0.5f), new Vector3(0,0,0),   2, S);
        Place("stoolBar",                  new Vector3( 0.0f, 0f, 0.5f), new Vector3(0,0,0),   2, S);
        Place("stoolBar",                  new Vector3( 0.8f, 0f, 0.5f), new Vector3(0,0,0),   2, S);

        // ── JEDÁLENSKÝ KÚT (vpravo vpredu) na koberci ──
        Place("rugRound",                  new Vector3( 3.0f, 0.02f, -3.2f), Vector3.zero,      2, S * 1.9f);
        Place("tableRound",                new Vector3( 3.0f, 0f, -3.2f), Vector3.zero,         5, S);
        Place("chairRounded",              new Vector3( 3.0f, 0f, -2.2f), new Vector3(0,180,0), 3, S);
        Place("chairRounded",              new Vector3( 3.0f, 0f, -4.2f), new Vector3(0,0,0),   3, S);
        Place("chairRounded",              new Vector3( 2.0f, 0f, -3.2f), new Vector3(0,90,0),  3, S);
        Place("chairRounded",              new Vector3( 4.0f, 0f, -3.2f), new Vector3(0,-90,0), 3, S);

        // ── DOPLNKY ──
        Place("pottedPlant",               new Vector3(-5.3f, 0f, -5.0f), Vector3.zero,        3, S);
        Place("plantSmall1",               new Vector3( 5.3f, 0f, 2.0f),  Vector3.zero,        2, S);
        Place("trashcan",                  new Vector3(-3.3f, 0f, -4.9f), Vector3.zero,        2, S);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        { list.Add(new EditorBuildSettingsScene(DST, true)); EditorBuildSettings.scenes = list.ToArray(); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Kitchen (reálne Kenney modely, správna mierka) hotová.");
    }

    // ── HELPERY (rovnaké ako Obývačka/Bedroom) ──
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
        if (prefab == null) { Debug.LogWarning("[Kitchen] model nenájdený: " + name); return null; }
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
