// Assets/Editor/SceneBuilder.cs
// Break Room -> Build Office Scene

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneBuilder : Editor
{
    [MenuItem("Break Room/Build Office Scene")]
    static void BuildScene()
    {
        // ── MATERIALY ─────────────────────────────────────────────────
        Material wood    = CreateMat("Wood",    new Color(0.45f, 0.28f, 0.12f));
        Material dark    = CreateMat("Dark",    new Color(0.12f, 0.12f, 0.14f));
        Material metal   = CreateMat("Metal",   new Color(0.55f, 0.55f, 0.60f));
        Material white   = CreateMat("White",   new Color(0.92f, 0.90f, 0.85f));
        Material screen  = CreateMat("Screen",  new Color(0.05f, 0.08f, 0.15f));
        Material red     = CreateMat("Red",     new Color(0.65f, 0.08f, 0.08f));
        Material book1   = CreateMat("Book1",   new Color(0.20f, 0.35f, 0.60f));
        Material book2   = CreateMat("Book2",   new Color(0.55f, 0.15f, 0.15f));
        Material book3   = CreateMat("Book3",   new Color(0.20f, 0.50f, 0.25f));
        Material lampShade = CreateMat("LampShade", new Color(0.9f, 0.8f, 0.3f));
        Material mug     = CreateMat("Mug",     new Color(0.85f, 0.82f, 0.78f));
        Material paper   = CreateMat("Paper",   new Color(0.95f, 0.93f, 0.88f));

        // ── ZMAZ STARE NABYTKOVE OBJEKTY ─────────────────────────────
        string[] toDelete = { "Stolička", "Stolička (1)", "Stolička (2)",
                               "OfficeDesk", "Monitor", "PCTower", "Lamp",
                               "Bookshelf", "FilingCabinet", "OfficeChair" };
        foreach (string n in toDelete)
        {
            GameObject old = GameObject.Find(n);
            if (old != null) DestroyImmediate(old);
        }

        // ═══════════════════════════════════════════════════════════════
        // PRACOVNY STOL (pri zadnej stene)
        // ═══════════════════════════════════════════════════════════════
        GameObject desk = new GameObject("OfficeDesk");

        // Doska stola
        var top = CreateBox(desk, "DeskTop", new Vector3(0, 0.85f, 3.2f),
                            new Vector3(3.8f, 0.10f, 1.8f), wood);

        // Nohy stola
        CreateBox(desk, "Leg_FL", new Vector3(-1.8f, 0.42f, 2.4f), new Vector3(0.10f, 0.84f, 0.10f), dark);
        CreateBox(desk, "Leg_FR", new Vector3( 1.8f, 0.42f, 2.4f), new Vector3(0.10f, 0.84f, 0.10f), dark);
        CreateBox(desk, "Leg_BL", new Vector3(-1.8f, 0.42f, 4.0f), new Vector3(0.10f, 0.84f, 0.10f), dark);
        CreateBox(desk, "Leg_BR", new Vector3( 1.8f, 0.42f, 4.0f), new Vector3(0.10f, 0.84f, 0.10f), dark);

        // Priehradka pod stolom
        CreateBox(desk, "DeskShelf", new Vector3(0, 0.40f, 3.9f),
                  new Vector3(3.8f, 0.05f, 1.2f), wood);

        AddBreakable(desk, 8);

        // ═══════════════════════════════════════════════════════════════
        // MONITOR
        // ═══════════════════════════════════════════════════════════════
        GameObject monitor = new GameObject("Monitor");

        // Stojan
        CreateBox(monitor, "Stand_Base", new Vector3(0.3f, 0.90f, 3.4f),
                  new Vector3(0.35f, 0.06f, 0.28f), metal);
        CreateBox(monitor, "Stand_Pole", new Vector3(0.3f, 1.08f, 3.46f),
                  new Vector3(0.06f, 0.30f, 0.06f), metal);

        // Obrazovka
        CreateBox(monitor, "Screen",     new Vector3(0.3f, 1.40f, 3.50f),
                  new Vector3(1.05f, 0.65f, 0.07f), screen);
        CreateBox(monitor, "Frame",      new Vector3(0.3f, 1.40f, 3.54f),
                  new Vector3(1.10f, 0.70f, 0.04f), dark);

        AddBreakable(monitor, 3);

        // ═══════════════════════════════════════════════════════════════
        // PC VEZA
        // ═══════════════════════════════════════════════════════════════
        GameObject pc = new GameObject("PCTower");
        CreateBox(pc, "Tower", new Vector3(1.6f, 1.20f, 3.6f),
                  new Vector3(0.32f, 0.72f, 0.55f), dark);
        CreateBox(pc, "Button", new Vector3(1.6f, 1.35f, 3.32f),
                  new Vector3(0.05f, 0.05f, 0.02f), metal);
        CreateBox(pc, "DVD",    new Vector3(1.6f, 1.20f, 3.32f),
                  new Vector3(0.24f, 0.02f, 0.02f), metal);
        AddBreakable(pc, 4);

        // ═══════════════════════════════════════════════════════════════
        // KLAVESNICA
        // ═══════════════════════════════════════════════════════════════
        GameObject kb = new GameObject("Keyboard");
        CreateBox(kb, "Keys", new Vector3(0.3f, 0.80f, 3.20f),
                  new Vector3(0.55f, 0.025f, 0.22f), dark);
        AddBreakable(kb, 2);

        // ═══════════════════════════════════════════════════════════════
        // KANCELARSKA LAMPA
        // ═══════════════════════════════════════════════════════════════
        GameObject lamp = new GameObject("DeskLamp");
        CreateBox(lamp, "Base",    new Vector3(-1.1f, 0.80f, 3.3f),
                  new Vector3(0.14f, 0.025f, 0.14f), metal);
        CreateBox(lamp, "Arm1",    new Vector3(-1.1f, 1.00f, 3.3f),
                  new Vector3(0.03f, 0.35f, 0.03f), metal);
        CreateBox(lamp, "Arm2",    new Vector3(-1.1f, 1.18f, 3.18f),
                  new Vector3(0.03f, 0.03f, 0.28f), metal);
        CreateBox(lamp, "Shade",   new Vector3(-1.1f, 1.10f, 3.06f),
                  new Vector3(0.18f, 0.12f, 0.14f), lampShade);
        AddBreakable(lamp, 2);

        // ═══════════════════════════════════════════════════════════════
        // SALKKA (mug)
        // ═══════════════════════════════════════════════════════════════
        GameObject mugObj = new GameObject("CoffeeMug");
        var mugBody = CreateCylinder(mugObj, "Body",
                  new Vector3(-0.5f, 0.87f, 3.3f),
                  new Vector3(0.07f, 0.07f, 0.07f), mug);
        AddBreakable(mugObj, 1);

        // ═══════════════════════════════════════════════════════════════
        // PAPIERE na stole
        // ═══════════════════════════════════════════════════════════════
        CreateBreakableBox("Papers_1", new Vector3(-0.8f, 0.79f, 3.15f),
                           new Vector3(0.28f, 0.01f, 0.22f), paper, 1);
        CreateBreakableBox("Papers_2", new Vector3(-0.75f, 0.80f, 3.18f),
                           new Vector3(0.26f, 0.01f, 0.20f), paper, 1);

        // ═══════════════════════════════════════════════════════════════
        // POLICA (na lavej stene)
        // ═══════════════════════════════════════════════════════════════
        GameObject shelf = new GameObject("Bookshelf");
        CreateBox(shelf, "Board_Top",  new Vector3(-4.6f, 2.20f, 1.0f),
                  new Vector3(0.12f, 0.06f, 1.80f), wood);
        CreateBox(shelf, "Board_Mid",  new Vector3(-4.6f, 1.50f, 1.0f),
                  new Vector3(0.12f, 0.06f, 1.80f), wood);
        CreateBox(shelf, "Board_Bot",  new Vector3(-4.6f, 0.85f, 1.0f),
                  new Vector3(0.12f, 0.06f, 1.80f), wood);
        CreateBox(shelf, "Side_Left",  new Vector3(-4.6f, 1.55f, 1.85f),
                  new Vector3(0.12f, 1.50f, 0.06f), wood);
        CreateBox(shelf, "Side_Right", new Vector3(-4.6f, 1.55f, 0.15f),
                  new Vector3(0.12f, 1.50f, 0.06f), wood);
        AddBreakable(shelf, 6);

        // Knihy na polici
        CreateBreakableBox("Book_1", new Vector3(-4.55f, 1.65f, 0.35f),
                           new Vector3(0.06f, 0.24f, 0.18f), book1, 1);
        CreateBreakableBox("Book_2", new Vector3(-4.55f, 1.65f, 0.58f),
                           new Vector3(0.06f, 0.22f, 0.16f), book2, 1);
        CreateBreakableBox("Book_3", new Vector3(-4.55f, 1.65f, 0.78f),
                           new Vector3(0.06f, 0.26f, 0.18f), book3, 1);
        CreateBreakableBox("Book_4", new Vector3(-4.55f, 1.65f, 1.00f),
                           new Vector3(0.06f, 0.20f, 0.16f), book1, 1);
        CreateBreakableBox("Book_5", new Vector3(-4.55f, 1.65f, 1.20f),
                           new Vector3(0.06f, 0.28f, 0.18f), book2, 1);

        // ═══════════════════════════════════════════════════════════════
        // KARTOTEKA (filing cabinet)
        // ═══════════════════════════════════════════════════════════════
        GameObject cabinet = new GameObject("FilingCabinet");
        CreateBox(cabinet, "Body",    new Vector3(-1.9f, 0.65f, 4.3f),
                  new Vector3(0.50f, 1.30f, 0.55f), metal);
        CreateBox(cabinet, "Drawer1", new Vector3(-1.9f, 1.05f, 4.02f),
                  new Vector3(0.44f, 0.28f, 0.04f), dark);
        CreateBox(cabinet, "Drawer2", new Vector3(-1.9f, 0.65f, 4.02f),
                  new Vector3(0.44f, 0.28f, 0.04f), dark);
        CreateBox(cabinet, "Drawer3", new Vector3(-1.9f, 0.28f, 4.02f),
                  new Vector3(0.44f, 0.28f, 0.04f), dark);
        CreateBox(cabinet, "Handle1", new Vector3(-1.9f, 1.05f, 3.99f),
                  new Vector3(0.12f, 0.025f, 0.025f), metal);
        CreateBox(cabinet, "Handle2", new Vector3(-1.9f, 0.65f, 3.99f),
                  new Vector3(0.12f, 0.025f, 0.025f), metal);
        AddBreakable(cabinet, 5);

        // ═══════════════════════════════════════════════════════════════
        // KANCELARSKA STOLICKA
        // ═══════════════════════════════════════════════════════════════
        GameObject chair = new GameObject("OfficeChair");
        CreateBox(chair, "Seat",    new Vector3(0, 0.58f, 2.2f),
                  new Vector3(0.60f, 0.08f, 0.60f), dark);
        CreateBox(chair, "Back",    new Vector3(0, 0.95f, 2.52f),
                  new Vector3(0.60f, 0.65f, 0.07f), dark);
        CreateBox(chair, "Pole",    new Vector3(0, 0.25f, 2.2f),
                  new Vector3(0.06f, 0.44f, 0.06f), metal);
        CreateBox(chair, "Wheel_F", new Vector3(0,    0.05f, 1.9f),
                  new Vector3(0.08f, 0.05f, 0.08f), dark);
        CreateBox(chair, "Wheel_B", new Vector3(0,    0.05f, 2.5f),
                  new Vector3(0.08f, 0.05f, 0.08f), dark);
        CreateBox(chair, "Wheel_L", new Vector3(-0.28f, 0.05f, 2.2f),
                  new Vector3(0.08f, 0.05f, 0.08f), dark);
        CreateBox(chair, "Wheel_R", new Vector3( 0.28f, 0.05f, 2.2f),
                  new Vector3(0.08f, 0.05f, 0.08f), dark);
        AddBreakable(chair, 4);

        // ═══════════════════════════════════════════════════════════════
        // KOS NA PAPIER
        // ═══════════════════════════════════════════════════════════════
        GameObject bin = new GameObject("TrashBin");
        CreateBox(bin, "Body", new Vector3(1.8f, 0.22f, 2.0f),
                  new Vector3(0.28f, 0.42f, 0.28f), dark);
        AddBreakable(bin, 1);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Office scena postavena! Ctrl+S uloz.");
    }

    // ── HELPER FUNKCIE ────────────────────────────────────────────────

    static Material CreateMat(string name, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.name  = name;
        return mat;
    }

    static Material CreateMat(string name, Color32 color) =>
        CreateMat(name, (Color)color);

    static GameObject CreateBox(GameObject parent, string name,
                                Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position   = pos;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().material = mat;
        if (parent != null) go.transform.SetParent(parent.transform);
        return go;
    }

    static GameObject CreateCylinder(GameObject parent, string name,
                                     Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.position   = pos;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().material = mat;
        if (parent != null) go.transform.SetParent(parent.transform);
        return go;
    }

    static void AddBreakable(GameObject parent, int hp)
    {
        var rb = parent.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        var col = parent.GetComponent<Collider>();
        if (col == null) parent.AddComponent<BoxCollider>();

        var br = parent.AddComponent<Breakable>();
        br.hp     = hp;
        br.damage = 1;
    }

    static void CreateBreakableBox(string name, Vector3 pos,
                                   Vector3 scale, Material mat, int hp)
    {
        var go = CreateBox(null, name, pos, scale, mat);
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        var br = go.AddComponent<Breakable>();
        br.hp     = hp;
        br.damage = 1;
    }
}
#endif
