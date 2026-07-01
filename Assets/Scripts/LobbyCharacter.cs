using UnityEngine;

/// Low-poly robotník (Synty / One Armed Robber štýl) do pozadia lobby:
/// modrá kombinéza, oranžová prilba, opretý o veľké kladivo. Postavený
/// z primitívov (rovnaký prístup ako zbrane a ruky), s jemnou idle
/// animáciou. Spawnuje sa za behu, takže netreba prestavovať Hub scénu.
public class LobbyCharacter : MonoBehaviour
{
    static readonly Color NAVY      = new Color(0.16f, 0.22f, 0.40f);
    static readonly Color NAVY_DARK = new Color(0.10f, 0.14f, 0.27f);
    static readonly Color ORANGE    = new Color(0.95f, 0.45f, 0.08f);
    static readonly Color SKIN       = new Color(0.86f, 0.67f, 0.52f);
    static readonly Color DARK       = new Color(0.10f, 0.10f, 0.12f);
    static readonly Color METAL      = new Color(0.55f, 0.57f, 0.62f);
    static readonly Color EYE        = new Color(0.06f, 0.06f, 0.08f);

    float phase;
    float bobBase;

    // Zaisti, že v scéne sú postavičky (zavolá HubManager.Start)
    public static void EnsureInScene()
    {
        if (FindFirstObjectByType<LobbyCharacter>() != null) return;
        // Robotníci tesne pri bariére po bokoch pódia (mimo pultu/debien, neprekrývajú sa)
        Build(new Vector3(-2.2f, 0f, -1.3f), 202f, 1.0f);
        Build(new Vector3( 2.4f, 0f, -1.3f), 158f, 0.95f);
    }

    /// Postaví jednu postavičku na danej pozícii nôh (feet), s natočením yRot a mierkou scale.
    public static GameObject Build(Vector3 feet, float yRot, float scale)
    {
        var root = new GameObject("LobbyWorker");
        root.transform.position = feet;
        root.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        root.transform.localScale = Vector3.one * scale;
        var lc = root.AddComponent<LobbyCharacter>();
        lc.phase = Random.Range(0f, 10f);
        lc.BuildBody(root.transform);
        return root;
    }

    void BuildBody(Transform p)
    {
        // --- NOHY ---
        Box(p, "LegL", new Vector3(-0.13f, 0.46f,  0f), new Vector3(0.19f, 0.84f, 0.23f), NAVY);
        Box(p, "LegR", new Vector3( 0.13f, 0.46f,  0f), new Vector3(0.19f, 0.84f, 0.23f), NAVY);
        // --- TOPÁNKY ---
        Box(p, "BootL", new Vector3(-0.13f, 0.08f, 0.06f), new Vector3(0.23f, 0.16f, 0.32f), DARK);
        Box(p, "BootR", new Vector3( 0.13f, 0.08f, 0.06f), new Vector3(0.23f, 0.16f, 0.32f), DARK);
        // --- TRUP (kombinéza) ---
        Box(p, "Torso", new Vector3(0f, 1.18f, 0f), new Vector3(0.54f, 0.70f, 0.36f), NAVY);
        Box(p, "Belt",  new Vector3(0f, 0.86f, 0f), new Vector3(0.56f, 0.10f, 0.38f), NAVY_DARK);
        Box(p, "Zip",   new Vector3(0f, 1.22f, 0.185f), new Vector3(0.05f, 0.62f, 0.02f), NAVY_DARK);
        // --- RAMENÁ / RUKY ---
        Box(p, "ArmL", new Vector3(-0.36f, 1.20f, 0f), new Vector3(0.17f, 0.60f, 0.19f), NAVY);
        Box(p, "ArmR", new Vector3( 0.36f, 1.20f, 0f), new Vector3(0.17f, 0.60f, 0.19f), NAVY);
        Box(p, "HandL", new Vector3(-0.36f, 0.90f, 0.02f), new Vector3(0.14f, 0.15f, 0.16f), SKIN);
        // pravá ruka drží kladivo (posunutá k násade)
        Box(p, "HandR", new Vector3( 0.44f, 1.02f, 0.16f), new Vector3(0.14f, 0.15f, 0.16f), SKIN);
        // --- KRK / HLAVA ---
        Box(p, "Neck", new Vector3(0f, 1.55f, 0f), new Vector3(0.15f, 0.10f, 0.15f), SKIN);
        Box(p, "Head", new Vector3(0f, 1.73f, 0f), new Vector3(0.29f, 0.31f, 0.28f), SKIN);
        Box(p, "EyeL", new Vector3(-0.07f, 1.75f, 0.145f), new Vector3(0.05f, 0.06f, 0.02f), EYE);
        Box(p, "EyeR", new Vector3( 0.07f, 1.75f, 0.145f), new Vector3(0.05f, 0.06f, 0.02f), EYE);
        // --- PRILBA (oranžová) ---
        Sphere(p, "HelmetDome", new Vector3(0f, 1.93f, 0f), new Vector3(0.34f, 0.26f, 0.35f), ORANGE);
        Box(p,    "HelmetBrim", new Vector3(0f, 1.86f, 0.10f), new Vector3(0.40f, 0.05f, 0.20f), ORANGE);
        Box(p,    "HelmetRidge",new Vector3(0f, 2.0f, 0f), new Vector3(0.07f, 0.10f, 0.34f), ORANGE);

        // --- KLADIVO (opreté vedľa tela) ---
        Cyl(p, "HammerHandle", new Vector3(0.46f, 0.66f, 0.16f), new Vector3(0.055f, 0.66f, 0.055f), new Vector3(4f, 0f, 0f), new Color(0.45f, 0.30f, 0.16f));
        Box(p, "HammerHead",   new Vector3(0.50f, 1.30f, 0.18f), new Vector3(0.26f, 0.20f, 0.22f), METAL);

        bobBase = transform.localPosition.y;
    }

    // ---------- IDLE ANIMÁCIA ----------
    void Update()
    {
        float t = Time.time + phase;
        // jemné dýchanie hore-dole
        float bob = Mathf.Sin(t * 1.6f) * 0.02f;
        var lp = transform.localPosition;
        transform.localPosition = new Vector3(lp.x, bobBase + bob, lp.z);
    }

    // ---------- HELPERY ----------
    static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 scale, Color col)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Strip(g, name, parent, pos, scale, col);
        return g;
    }

    static GameObject Sphere(Transform parent, string name, Vector3 pos, Vector3 scale, Color col)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Strip(g, name, parent, pos, scale, col);
        return g;
    }

    static GameObject Cyl(Transform parent, string name, Vector3 pos, Vector3 scale, Vector3 euler, Color col)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Strip(g, name, parent, pos, scale, col);
        g.transform.localEulerAngles = euler;
        return g;
    }

    static void Strip(GameObject g, string name, Transform parent, Vector3 pos, Vector3 scale, Color col)
    {
        g.name = name;
        var c = g.GetComponent<Collider>(); if (c != null) Destroy(c);  // prop, nech neprekáža
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mat(col);
    }

    static Material Mat(Color col)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = col;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
        return m;
    }
}
