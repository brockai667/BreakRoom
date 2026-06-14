// Assets/Editor/CreateBedroomScene.cs
// Break Room -> Build Bedroom Scene
// Vytvorí NOVÚ mapu "Bedroom": vezme shell Obývačky (steny/podlaha/hráč/manažéri),
// zmaže pôvodný nábytok a postaví vlastný spálňový nábytok (iné predmety!).

#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class CreateBedroomScene
{
    const string SRC = "Assets/Scenes/Obyvacka.unity";
    const string DST = "Assets/Scenes/Bedroom.unity";

    [MenuItem("Break Room/Build Bedroom Scene")]
    static void Build()
    {
        // 1) otvor shell a ulož ako Bedroom (Save As)
        var scene = EditorSceneManager.OpenScene(SRC, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(scene, DST);

        // 2) zmaž všetok zdedený nábytok (všetko s Breakable)
        foreach (var b in Object.FindObjectsByType<Breakable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b != null) Object.DestroyImmediate(b.gameObject);

        // ── MATERIÁLY ──
        Material wood   = Mat(new Color(0.45f, 0.30f, 0.16f));
        Material woodLt = Mat(new Color(0.62f, 0.45f, 0.28f));
        Material dark   = Mat(new Color(0.12f, 0.12f, 0.14f));
        Material metal  = Mat(new Color(0.55f, 0.57f, 0.62f));
        Material sheet  = Mat(new Color(0.80f, 0.84f, 0.92f));   // posteľná bielizeň
        Material blanket= Mat(new Color(0.30f, 0.45f, 0.70f));   // deka
        Material pillow = Mat(new Color(0.92f, 0.92f, 0.95f));
        Material screen = Mat(new Color(0.05f, 0.08f, 0.15f));
        Material lamp   = Mat(new Color(0.95f, 0.85f, 0.45f));
        Material plantG = Mat(new Color(0.20f, 0.50f, 0.25f));
        Material pot    = Mat(new Color(0.65f, 0.35f, 0.22f));
        Material rugC   = Mat(new Color(0.55f, 0.22f, 0.28f));
        Material book1  = Mat(new Color(0.20f, 0.35f, 0.60f));
        Material book2  = Mat(new Color(0.55f, 0.15f, 0.15f));
        Material book3  = Mat(new Color(0.20f, 0.50f, 0.25f));
        Material glass  = Mat(new Color(0.55f, 0.70f, 0.80f));

        // ── POSTEĽ (center-back) ──
        var bed = new GameObject("Bed");
        CreateBox(bed, "Frame",    new Vector3(0f, 0.30f, 2.6f), new Vector3(2.4f, 0.40f, 3.0f), wood);
        CreateBox(bed, "Mattress", new Vector3(0f, 0.58f, 2.7f), new Vector3(2.2f, 0.25f, 2.7f), sheet);
        CreateBox(bed, "Blanket",  new Vector3(0f, 0.66f, 3.3f), new Vector3(2.25f, 0.10f, 1.5f), blanket);
        CreateBox(bed, "Headboard",new Vector3(0f, 0.95f, 4.05f),new Vector3(2.4f, 0.90f, 0.18f), woodLt);
        CreateBox(bed, "PillowL",  new Vector3(-0.55f, 0.78f, 3.7f), new Vector3(0.85f, 0.16f, 0.5f), pillow);
        CreateBox(bed, "PillowR",  new Vector3( 0.55f, 0.78f, 3.7f), new Vector3(0.85f, 0.16f, 0.5f), pillow);
        Breakable_(bed, 9);

        // ── ŠATNÍK (ľavá stena) ──
        var wd = new GameObject("Wardrobe");
        CreateBox(wd, "Body",  new Vector3(-4.0f, 1.30f, 1.2f), new Vector3(0.70f, 2.55f, 1.8f), woodLt);
        CreateBox(wd, "DoorL", new Vector3(-3.66f, 1.30f, 0.78f), new Vector3(0.04f, 2.4f, 0.82f), wood);
        CreateBox(wd, "DoorR", new Vector3(-3.66f, 1.30f, 1.62f), new Vector3(0.04f, 2.4f, 0.82f), wood);
        CreateBox(wd, "HandleL", new Vector3(-3.62f, 1.30f, 1.1f), new Vector3(0.04f, 0.18f, 0.04f), metal);
        CreateBox(wd, "HandleR", new Vector3(-3.62f, 1.30f, 1.3f), new Vector3(0.04f, 0.18f, 0.04f), metal);
        Breakable_(wd, 8);

        // ── KOMODA (pravá-zadná) + TV ──
        var dr = new GameObject("Dresser");
        CreateBox(dr, "Body",   new Vector3(3.4f, 0.55f, 3.2f), new Vector3(1.6f, 1.10f, 0.7f), wood);
        CreateBox(dr, "Drawer1",new Vector3(3.4f, 0.80f, 2.86f), new Vector3(1.4f, 0.30f, 0.04f), woodLt);
        CreateBox(dr, "Drawer2",new Vector3(3.4f, 0.42f, 2.86f), new Vector3(1.4f, 0.30f, 0.04f), woodLt);
        CreateBox(dr, "Knob1",  new Vector3(3.4f, 0.80f, 2.83f), new Vector3(0.08f, 0.08f, 0.05f), metal);
        CreateBox(dr, "Knob2",  new Vector3(3.4f, 0.42f, 2.83f), new Vector3(0.08f, 0.08f, 0.05f), metal);
        Breakable_(dr, 6);

        var tv = new GameObject("TVScreen");
        CreateBox(tv, "Screen", new Vector3(3.4f, 1.55f, 3.45f), new Vector3(1.30f, 0.78f, 0.07f), screen);
        CreateBox(tv, "Frame",  new Vector3(3.4f, 1.55f, 3.49f), new Vector3(1.38f, 0.86f, 0.04f), dark);
        CreateBox(tv, "Stand",  new Vector3(3.4f, 1.12f, 3.4f),  new Vector3(0.10f, 0.10f, 0.10f), metal);
        Breakable_(tv, 3);

        // ── NOČNÉ STOLÍKY + lampa ──
        var n1 = new GameObject("Nightstand_L");
        CreateBox(n1, "Body", new Vector3(-1.55f, 0.32f, 3.9f), new Vector3(0.55f, 0.55f, 0.55f), wood);
        CreateBox(n1, "Drawer", new Vector3(-1.55f, 0.40f, 3.62f), new Vector3(0.46f, 0.18f, 0.04f), woodLt);
        Breakable_(n1, 3);

        var n2 = new GameObject("Nightstand_R");
        CreateBox(n2, "Body", new Vector3(1.55f, 0.32f, 3.9f), new Vector3(0.55f, 0.55f, 0.55f), wood);
        CreateBox(n2, "Drawer", new Vector3(1.55f, 0.40f, 3.62f), new Vector3(0.46f, 0.18f, 0.04f), woodLt);
        Breakable_(n2, 3);

        var lp = new GameObject("BedsideLamp");
        CreateBox(lp, "Base",  new Vector3(-1.55f, 0.62f, 3.9f), new Vector3(0.16f, 0.04f, 0.16f), metal);
        CreateBox(lp, "Pole",  new Vector3(-1.55f, 0.80f, 3.9f), new Vector3(0.04f, 0.30f, 0.04f), metal);
        CreateBox(lp, "Shade", new Vector3(-1.55f, 1.00f, 3.9f), new Vector3(0.26f, 0.20f, 0.26f), lamp);
        Breakable_(lp, 2);

        // ── ZRKADLO (pravá stena) ──
        var mir = new GameObject("Mirror");
        CreateBox(mir, "Frame", new Vector3(4.1f, 1.4f, 0.6f), new Vector3(0.10f, 1.7f, 0.9f), woodLt);
        CreateBox(mir, "Glass", new Vector3(4.04f, 1.4f, 0.6f), new Vector3(0.04f, 1.5f, 0.75f), glass);
        Breakable_(mir, 2);

        // ── POLICA + knihy (ľavá-zadná) ──
        var sh = new GameObject("Bookshelf");
        CreateBox(sh, "Top", new Vector3(-4.1f, 2.10f, 3.4f), new Vector3(0.30f, 0.06f, 1.4f), woodLt);
        CreateBox(sh, "Mid", new Vector3(-4.1f, 1.55f, 3.4f), new Vector3(0.30f, 0.06f, 1.4f), woodLt);
        CreateBox(sh, "Bot", new Vector3(-4.1f, 1.00f, 3.4f), new Vector3(0.30f, 0.06f, 1.4f), woodLt);
        CreateBox(sh, "SideA", new Vector3(-4.1f, 1.55f, 2.75f), new Vector3(0.30f, 1.2f, 0.06f), woodLt);
        CreateBox(sh, "SideB", new Vector3(-4.1f, 1.55f, 4.05f), new Vector3(0.30f, 1.2f, 0.06f), woodLt);
        Breakable_(sh, 5);
        BreakBox("Book_1", new Vector3(-4.08f, 1.72f, 3.0f), new Vector3(0.18f, 0.26f, 0.06f), book1, 1);
        BreakBox("Book_2", new Vector3(-4.08f, 1.72f, 3.2f), new Vector3(0.16f, 0.22f, 0.06f), book2, 1);
        BreakBox("Book_3", new Vector3(-4.08f, 1.72f, 3.4f), new Vector3(0.18f, 0.24f, 0.06f), book3, 1);
        BreakBox("Book_4", new Vector3(-4.08f, 1.16f, 3.6f), new Vector3(0.16f, 0.20f, 0.06f), book1, 1);

        // ── SEDACÍ VAK ──
        var bag = new GameObject("Beanbag");
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Bag"; sphere.transform.position = new Vector3(2.4f, 0.45f, 0.6f);
        sphere.transform.localScale = new Vector3(1.0f, 0.7f, 1.0f);
        sphere.GetComponent<MeshRenderer>().material = Mat(new Color(0.75f, 0.45f, 0.20f));
        sphere.transform.SetParent(bag.transform);
        Breakable_(bag, 3);

        // ── KVETINÁČ ──
        var plant = new GameObject("PottedPlant");
        CreateCyl(plant, "Pot", new Vector3(-3.6f, 0.30f, 4.0f), new Vector3(0.28f, 0.30f, 0.28f), pot);
        var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = "Leaves"; foliage.transform.position = new Vector3(-3.6f, 0.85f, 4.0f);
        foliage.transform.localScale = new Vector3(0.55f, 0.6f, 0.55f);
        foliage.GetComponent<MeshRenderer>().material = plantG;
        foliage.transform.SetParent(plant.transform);
        Breakable_(plant, 2);

        // ── KOBEREC ──
        BreakBox("Rug", new Vector3(0f, 0.02f, 0.6f), new Vector3(3.2f, 0.04f, 2.2f), rugC, 2);

        // ── KÔŠ NA BIELIZEŇ ──
        var basket = new GameObject("LaundryBasket");
        CreateCyl(basket, "Body", new Vector3(-2.8f, 0.30f, 0.4f), new Vector3(0.4f, 0.35f, 0.4f), Mat(new Color(0.8f,0.78f,0.7f)));
        Breakable_(basket, 1);

        // 3) ulož + zaregistruj do Build Settings
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == DST))
        {
            list.Add(new EditorBuildSettingsScene(DST, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Bedroom scena vytvorena (vlastny nabytok) a pridana do Build Settings.");
    }

    // ── HELPERY ──
    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }

    static GameObject CreateBox(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().material = mat;
        if (parent != null) go.transform.SetParent(parent.transform);
        return go;
    }

    static GameObject CreateCyl(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name; go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().material = mat;
        if (parent != null) go.transform.SetParent(parent.transform);
        return go;
    }

    static void Breakable_(GameObject parent, int hp)
    {
        var rb = parent.AddComponent<Rigidbody>(); rb.isKinematic = true;
        if (parent.GetComponent<Collider>() == null) parent.AddComponent<BoxCollider>();
        var br = parent.AddComponent<Breakable>(); br.hp = hp; br.damage = 1;
    }

    static void BreakBox(string name, Vector3 pos, Vector3 scale, Material mat, int hp)
    {
        var go = CreateBox(null, name, pos, scale, mat);
        var rb = go.AddComponent<Rigidbody>(); rb.isKinematic = true;
        var br = go.AddComponent<Breakable>(); br.hp = hp; br.damage = 1;
    }
}
#endif
