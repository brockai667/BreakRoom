// Assets/Editor/CreateGarageScene.cs
// Break Room -> Build Garage Scene
// Skutočná dielňa z REÁLNYCH Kenney Factory Kit modelov (nie kópia, nie kocky).

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateGarageScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Garage.unity";
    const string FAC = "Assets/kenney_factory-kit_3.0/Models/FBX format/";

    [MenuItem("Break Room/Build Garage Scene")]
    static void Build()
    {
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        // zmaž zdedený nábytok
        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);

        // ── DIELŇA z factory-kit modelov ──
        // veľké stroje / pracovné stoly
        Spawn("machine",          new Vector3(-2.6f, 0f, 3.2f), 90f,  2.0f, 10);
        Spawn("machine-bed",      new Vector3( 2.6f, 0f, 3.2f), -90f, 2.0f, 10);
        Spawn("machine-fortified",new Vector3( 0.0f, 0f, 4.0f), 0f,   2.0f, 12);
        // nádrže / sudy (hoppery)
        Spawn("hopper-round",     new Vector3( 3.6f, 0f, 1.0f), 0f,   1.8f, 6);
        Spawn("hopper-square",    new Vector3(-3.6f, 0f, 1.2f), 0f,   1.8f, 6);
        Spawn("hopper-high-round",new Vector3( 3.8f, 0f, 3.6f), 0f,   1.6f, 6);
        // dopravník cez stred
        Spawn("conveyor-long",    new Vector3( 0.0f, 0f, 1.6f), 0f,   2.2f, 8);
        // debny
        Spawn("box-large",        new Vector3(-3.4f, 0f, -1.0f), 20f, 1.4f, 4);
        Spawn("box-wide",         new Vector3( 1.6f, 0f, 0.2f), -15f, 1.3f, 3);
        Spawn("box-long",         new Vector3(-1.6f, 0f, -1.6f), 35f, 1.3f, 3);
        Spawn("box-small",        new Vector3( 0.6f, 0f, -2.2f), 10f, 1.3f, 2);
        Spawn("box-small",        new Vector3( 2.6f, 0f, -1.2f), -25f,1.3f, 2);
        // ozubené kolesá + páky + kužeľ
        Spawn("cog-a",            new Vector3(-2.2f, 0.6f, 0.4f), 0f, 1.6f, 4);
        Spawn("cog-c",            new Vector3( 3.1f, 0.6f, -1.4f), 0f,1.6f, 4);
        Spawn("lever-single",     new Vector3(-3.2f, 0f, 2.6f), 0f,   1.6f, 2);
        Spawn("cone",             new Vector3( 1.4f, 0f, -2.6f), 0f,  1.6f, 2);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        {
            list.Add(new EditorBuildSettingsScene(DST, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("✅ Garage scena z Kenney Factory Kit modelov hotova.");
    }

    // ── HELPERY ──
    static GameObject Spawn(string fbx, Vector3 pos, float yRot, float scale, int hp)
    {
        string path = FAC + fbx + ".fbx";
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (model == null) { Debug.LogWarning("[Garage] chyba model: " + path); return null; }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        if (go == null) go = Object.Instantiate(model);
        go.name = fbx;
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        go.transform.localScale = Vector3.one * scale;

        FitBox(go);
        var rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true;
        var br = go.AddComponent<Breakable>(); br.hp = hp; br.damage = 1;
        return go;
    }

    // BoxCollider fitnutý na kombinované local bounds všetkých meshov.
    static void FitBox(GameObject go)
    {
        var mfs = go.GetComponentsInChildren<MeshFilter>();
        bool any = false; Bounds local = new Bounds();
        foreach (var mf in mfs)
        {
            if (mf.sharedMesh == null) continue;
            Matrix4x4 m = go.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;
            Vector3 c = mf.sharedMesh.bounds.center, e = mf.sharedMesh.bounds.extents;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                    {
                        Vector3 p = m.MultiplyPoint3x4(c + new Vector3(e.x * sx, e.y * sy, e.z * sz));
                        if (!any) { local = new Bounds(p, Vector3.zero); any = true; }
                        else local.Encapsulate(p);
                    }
        }
        var bc = go.AddComponent<BoxCollider>();
        if (any) { bc.center = local.center; bc.size = local.size; }
    }
}
#endif
