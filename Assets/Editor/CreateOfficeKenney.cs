// Assets/Editor/CreateOfficeKenney.cs
// Break Room -> Build Office Scene
// Skutočná open-space kancelária z REÁLNYCH Kenney modelov (mierka S=0.4 ako ostatné izby).
// Vzor: CreateKitchenScene – otvor funkčnú Obývačku, ulož ako Office, zmaž breakables,
// polož kancelársky nábytok do usporiadaného layoutu (rady stolov, knižnice, zasadačka).

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateOfficeKenney
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Office.unity";
    const float  S   = 0.4f;

    [MenuItem("BreakRoom/Art/Build Office (Kenney)")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);

        // zmaž veľký koberec z Obývačky (je nerozbíjateľný, preto ostal – farbí podlahu)
        var oldRug = GameObject.Find("rugRectangle");
        if (oldRug != null) Object.DestroyImmediate(oldRug);

        // hráča posuň k vstupu (nech sa nespawnuje v nábytku)
        var pl = GameObject.Find("Player");
        if (pl != null) pl.transform.position = new Vector3(0f, 1f, -5.2f);

        // ── RAD STOLOV pri zadnej stene (stôl čelom do miestnosti -z, stolička pred ním) ──
        float[] deskX = { -3.4f, 0f, 3.4f };
        foreach (float x in deskX)
        {
            Place("desk",           new Vector3(x, 0f, 4.5f),  new Vector3(0,180,0), 6, S);
            Place("chairDesk",      new Vector3(x, 0f, 3.4f),  new Vector3(0,0,0),   4, S);
            Place("computerScreen", new Vector3(x, 0.62f, 4.7f), new Vector3(0,180,0), 3, S);
        }
        Place("laptop", new Vector3(0.5f, 0.62f, 4.3f), new Vector3(0,180,0), 2, S);
        Place("books",  new Vector3(-3.0f, 0.62f, 4.6f), new Vector3(0,180,0), 1, S);

        // ── DRUHÝ RAD STOLOV (uprostred, čelom +z) ──
        Place("desk",           new Vector3(-2.2f, 0f, 0.6f), new Vector3(0,0,0),  6, S);
        Place("chairDesk",      new Vector3(-2.2f, 0f, 1.7f), new Vector3(0,180,0),4, S);
        Place("computerScreen", new Vector3(-2.2f, 0.62f, 0.4f), new Vector3(0,0,0), 3, S);
        Place("desk",           new Vector3( 2.2f, 0f, 0.6f), new Vector3(0,0,0),  6, S);
        Place("chairDesk",      new Vector3( 2.2f, 0f, 1.7f), new Vector3(0,180,0),4, S);
        Place("computerScreen", new Vector3( 2.2f, 0.62f, 0.4f), new Vector3(0,0,0), 3, S);
        Place("laptop",         new Vector3( 2.5f, 0.62f, 0.8f), new Vector3(0,0,0), 2, S);

        // ── KNIŽNICE pri ľavej stene (čelom +x) ──
        Place("bookcaseClosedDoors", new Vector3(-5.4f, 0f, -3.5f), new Vector3(0,90,0), 7, S);
        Place("bookcaseOpen",        new Vector3(-5.4f, 0f, -1.0f), new Vector3(0,90,0), 6, S);
        Place("books",               new Vector3(-5.2f, 0.5f, -1.0f), new Vector3(0,90,0), 1, S);
        Place("bookcaseClosed",      new Vector3(-5.4f, 0f,  1.5f), new Vector3(0,90,0), 7, S);

        // ── KARTOTÉKY / skrine pri pravej stene (čelom -x) ──
        Place("cabinetBedDrawer", new Vector3(5.4f, 0f, -3.5f), new Vector3(0,-90,0), 5, S);
        Place("cabinetBedDrawer", new Vector3(5.4f, 0f, -1.8f), new Vector3(0,-90,0), 5, S);
        Place("cabinetBedDrawer", new Vector3(5.4f, 0f,  1.6f), new Vector3(0,-90,0), 5, S);
        Place("computerScreen",   new Vector3(5.2f, 0.9f, -1.8f), new Vector3(0,-90,0), 2, S);

        // ── ZASADAČKA (okrúhly stôl + stoličky) vľavo-vpredu, mimo spawnu ──
        Vector3 mt = new Vector3(-3.2f, 0f, -3.4f);
        Place("tableRound",   mt, Vector3.zero, 5, S);
        Place("chairDesk",    mt + new Vector3(0f, 0f, 1.2f),  new Vector3(0,180,0), 3, S);
        Place("chairDesk",    mt + new Vector3(0f, 0f,-1.2f),  new Vector3(0,0,0),   3, S);
        Place("chairDesk",    mt + new Vector3(-1.3f,0f, 0f),  new Vector3(0,90,0),  3, S);
        Place("chairDesk",    mt + new Vector3( 1.3f,0f, 0f),  new Vector3(0,-90,0), 3, S);

        // ── DOPLNKY: rastliny, vešiak, koše, stolová lampa ──
        Place("pottedPlant",      new Vector3(-5.3f, 0f, 5.2f), Vector3.zero, 3, S);
        Place("pottedPlant",      new Vector3( 5.3f, 0f, 5.2f), Vector3.zero, 3, S);
        Place("plantSmall2",      new Vector3( 3.0f, 0.62f, 4.6f), Vector3.zero, 1, S);
        Place("coatRackStanding", new Vector3( 4.8f, 0f, -5.0f), Vector3.zero, 3, S);
        Place("trashcan",         new Vector3(-2.0f, 0f, 3.0f), Vector3.zero, 2, S);
        Place("trashcan",         new Vector3( 2.0f, 0f, 3.0f), Vector3.zero, 2, S);
        Place("lampSquareTable",  new Vector3(-2.2f, 0.62f, 0.8f), Vector3.zero, 1, S);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        { list.Add(new EditorBuildSettingsScene(DST, true)); EditorBuildSettings.scenes = list.ToArray(); }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Office (reálna kancelária z Kenney modelov) hotová.");
    }

    // ── HELPERY (rovnaké ako Kitchen/Bedroom) ──
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
        if (prefab == null) { Debug.LogWarning("[Office] model nenájdený: " + name); return null; }
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
