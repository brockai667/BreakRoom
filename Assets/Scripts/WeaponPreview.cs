using UnityEngine;

/// 3D náhľad zbrane na pódiu v hube + model v ruke (FirstPersonHands).
/// Postaví jednoduchý low-poly model z primitív podľa WeaponData tak, aby
/// každá zbraň vyzerala podľa svojho názvu (pílka má lištu so zubami atď.).
public class WeaponPreview : MonoBehaviour
{
    public static WeaponPreview Instance;

    private GameObject current;
    /// Rýchlosť rotácie modelu na pódiu (stupne/s).
    public float spinSpeed = 35f;

    void Awake() { Instance = this; }

    void Update()
    {
        if (current != null)
            current.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    /// Zobrazí (a nahradí prípadný predošlý) 3D model danej zbrane na pódiu.
    public void Show(WeaponData w)
    {
        if (w == null) return;
        if (current != null) Destroy(current);
        current = BuildModel(w);
        current.transform.SetParent(transform, false);
        current.transform.localPosition = Vector3.zero;
        current.SetActive(true);
    }

    /// Zapne/vypne viditeľnosť aktuálneho modelu na pódiu.
    public void SetVisible(bool v)
    {
        if (current != null) current.SetActive(v);
    }

    // ---------- STAVBA MODELU ----------
    /// Postaví low-poly model zbrane z primitív podľa w.id (tvar špecifický pre danú zbraň).
    public static GameObject BuildModel(WeaponData w)
    {
        var root = new GameObject("WeaponModel_" + w.id);
        var handleMat = Mat(w.handleColor);
        var headMat   = Mat(w.handColor);

        switch (w.id)
        {
            case "fists":
                Add(root, PrimitiveType.Cube, new Vector3(-0.22f, 0, 0), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, headMat);
                Add(root, PrimitiveType.Cube, new Vector3( 0.22f, 0, 0), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, headMat);
                break;
            case "bat":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.35f, 0), new Vector3(0.10f, 0.30f, 0.10f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cylinder, new Vector3(0,  0.25f, 0), new Vector3(0.17f, 0.40f, 0.17f), Quaternion.identity, headMat);
                break;
            case "gloves":
                Add(root, PrimitiveType.Cube, new Vector3(-0.20f, 0, 0), new Vector3(0.32f, 0.32f, 0.34f), Quaternion.identity, headMat);
                Add(root, PrimitiveType.Cube, new Vector3( 0.20f, 0, 0), new Vector3(0.32f, 0.32f, 0.34f), Quaternion.identity, headMat);
                break;
            case "hammer":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.15f, 0), new Vector3(0.07f, 0.45f, 0.07f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0,  0.45f, 0), new Vector3(0.50f, 0.24f, 0.24f), Quaternion.identity, headMat);
                break;
            case "axe":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.10f, 0), new Vector3(0.07f, 0.50f, 0.07f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0.20f, 0.42f, 0), new Vector3(0.40f, 0.42f, 0.06f), Quaternion.Euler(0, 0, -12f), headMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0.02f, 0.45f, 0), new Vector3(0.10f, 0.30f, 0.10f), Quaternion.identity, headMat);
                break;
            case "sledge":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.15f, 0), new Vector3(0.08f, 0.48f, 0.08f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0,  0.50f, 0), new Vector3(0.58f, 0.32f, 0.32f), Quaternion.identity, headMat);
                break;
            case "crowbar":
                BuildCrowbar(root, handleMat, headMat);
                break;
            case "katana":
                BuildKatana(root, handleMat, headMat);
                break;
            case "chainsaw":
                BuildChainsaw(root, handleMat, headMat);
                break;
            case "bowling":
                BuildBowling(root, handleMat, headMat);
                break;
            case "grenade":
                BuildGrenade(root, handleMat, headMat);
                break;
            case "pan":
                BuildPan(root, handleMat, headMat);
                break;
            case "wrench":
                BuildWrench(root, handleMat, headMat);
                break;
            case "golf":
                BuildGolf(root, handleMat, headMat);
                break;
            case "pickaxe":
                BuildPickaxe(root, handleMat, headMat);
                break;
            case "mace":
                BuildMace(root, handleMat, headMat);
                break;
            case "flamethrower":
                Add(root, PrimitiveType.Cube,     new Vector3(0, 0, -0.05f), new Vector3(0.34f, 0.32f, 0.62f), Quaternion.identity, headMat);
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.20f, -0.18f), new Vector3(0.07f, 0.18f, 0.07f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cylinder, new Vector3(0, 0.04f, 0.55f), new Vector3(0.07f, 0.22f, 0.07f), Quaternion.Euler(90, 0, 0), handleMat);
                Add(root, PrimitiveType.Cylinder, new Vector3(0.16f, -0.02f, -0.05f), new Vector3(0.16f, 0.18f, 0.16f), Quaternion.Euler(0, 0, 90), Mat(new Color(0.55f,0.12f,0.10f)));
                break;
            default:
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.10f, 0), new Vector3(0.07f, 0.45f, 0.07f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0,  0.45f, 0), new Vector3(0.42f, 0.26f, 0.26f), Quaternion.identity, headMat);
                break;
        }
        return root;
    }

    // --- Páčidlo (crowbar): ohnutý kovový prút ---
    static void BuildCrowbar(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.05f, 0), new Vector3(0.08f, 0.55f, 0.08f), Quaternion.identity, headMat);
        // ohyb hore
        Add(root, PrimitiveType.Cylinder, new Vector3(0.12f, 0.48f, 0), new Vector3(0.08f, 0.16f, 0.08f), Quaternion.Euler(0, 0, 55f), headMat);
        // rozdvojený pazúr
        Add(root, PrimitiveType.Cube, new Vector3(0.26f, 0.58f, 0.03f), new Vector3(0.06f, 0.16f, 0.05f), Quaternion.Euler(0, 0, 35f), headMat);
        Add(root, PrimitiveType.Cube, new Vector3(0.26f, 0.58f, -0.03f), new Vector3(0.06f, 0.16f, 0.05f), Quaternion.Euler(0, 0, 35f), headMat);
        // spodný hrot
        Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.36f, 0), new Vector3(0.06f, 0.10f, 0.06f), Quaternion.Euler(18f, 0, 0), headMat);
    }

    // --- Katana: dlhá tenká čepeľ + záštita + rukoväť ---
    static void BuildKatana(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.42f, 0), new Vector3(0.07f, 0.22f, 0.07f), Quaternion.identity, handleMat);   // rukoväť
        Add(root, PrimitiveType.Cube,     new Vector3(0, -0.18f, 0), new Vector3(0.22f, 0.04f, 0.12f), Quaternion.identity, handleMat);   // záštita (tsuba)
        Add(root, PrimitiveType.Cube,     new Vector3(0, 0.28f, 0),  new Vector3(0.06f, 0.86f, 0.02f), Quaternion.identity, headMat);     // čepeľ
        Add(root, PrimitiveType.Cube,     new Vector3(0, 0.74f, 0.01f), new Vector3(0.055f, 0.10f, 0.02f), Quaternion.Euler(0, 0, 18f), headMat); // hrot
    }

    // --- Motorová píla: telo + lišta so zubami (animované) ---
    static void BuildChainsaw(GameObject root, Material handleMat, Material headMat)
    {
        Material toothMat = Mat(new Color(0.18f, 0.18f, 0.20f));

        // telo
        Add(root, PrimitiveType.Cube, new Vector3(0, 0, -0.18f), new Vector3(0.34f, 0.40f, 0.46f), Quaternion.identity, headMat);
        // zadná rukoväť
        Add(root, PrimitiveType.Cube, new Vector3(0, -0.20f, -0.42f), new Vector3(0.14f, 0.30f, 0.16f), Quaternion.Euler(18f, 0, 0), handleMat);
        // horná rukoväť (oblúk)
        Add(root, PrimitiveType.Cube, new Vector3(0, 0.30f, -0.16f), new Vector3(0.12f, 0.10f, 0.46f), Quaternion.identity, handleMat);
        Add(root, PrimitiveType.Cube, new Vector3(0, 0.16f, 0.06f), new Vector3(0.12f, 0.22f, 0.10f), Quaternion.identity, handleMat);

        // vodiaca lišta (plochá, dopredu)
        float barZ = 0.46f;
        Add(root, PrimitiveType.Cube, new Vector3(0, 0.02f, barZ), new Vector3(0.05f, 0.20f, 0.80f), Quaternion.identity, handleMat);

        // zuby reťaze na hornej a spodnej hrane lišty
        float zMin = barZ - 0.40f, zMax = barZ + 0.40f;
        int n = 9;
        var top = new Transform[n];
        var bottom = new Transform[n];
        float topY = 0.02f + 0.12f, botY = 0.02f - 0.12f;
        for (int i = 0; i < n; i++)
        {
            top[i]    = Add(root, PrimitiveType.Cube, new Vector3(0, topY, zMin), new Vector3(0.055f, 0.05f, 0.06f), Quaternion.identity, toothMat).transform;
            bottom[i] = Add(root, PrimitiveType.Cube, new Vector3(0, botY, zMin), new Vector3(0.055f, 0.05f, 0.06f), Quaternion.identity, toothMat).transform;
        }
        var blade = root.AddComponent<ChainsawBlade>();
        blade.top = top; blade.bottom = bottom; blade.zMin = zMin; blade.zMax = zMax;
    }

    // --- Bowlingová guľa: tmavá guľa s 3 dierami ---
    static void BuildBowling(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.72f, Quaternion.identity, headMat);
        Material holeMat = Mat(new Color(0.04f, 0.04f, 0.08f));
        Add(root, PrimitiveType.Cylinder, new Vector3(-0.08f, 0.32f, 0.10f), new Vector3(0.06f, 0.06f, 0.06f), Quaternion.identity, holeMat);
        Add(root, PrimitiveType.Cylinder, new Vector3(0.08f, 0.32f, 0.10f), new Vector3(0.06f, 0.06f, 0.06f), Quaternion.identity, holeMat);
        Add(root, PrimitiveType.Cylinder, new Vector3(0f, 0.30f, 0.22f), new Vector3(0.06f, 0.06f, 0.06f), Quaternion.identity, holeMat);
    }

    // --- Granát: zelené telo + poistka + páčka ---
    static void BuildGrenade(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.5f, 0.56f, 0.5f), Quaternion.identity, headMat);
        Material steel = Mat(new Color(0.45f, 0.46f, 0.5f));
        Add(root, PrimitiveType.Cylinder, new Vector3(0, 0.30f, 0), new Vector3(0.20f, 0.08f, 0.20f), Quaternion.identity, steel); // poistka
        Add(root, PrimitiveType.Cube,     new Vector3(0.12f, 0.34f, 0), new Vector3(0.05f, 0.04f, 0.16f), Quaternion.Euler(0, 0, 8f), steel); // páčka
        Add(root, PrimitiveType.Cylinder, new Vector3(0.20f, 0.36f, 0), new Vector3(0.12f, 0.02f, 0.12f), Quaternion.Euler(90, 0, 0), steel); // krúžok
    }

    // --- Panvica: rukoväť + plochý disk + okraj ---
    static void BuildPan(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.42f, 0), new Vector3(0.07f, 0.26f, 0.07f), Quaternion.identity, handleMat); // rukoväť
        Add(root, PrimitiveType.Cylinder, new Vector3(0,  0.02f, 0), new Vector3(0.62f, 0.04f, 0.62f), Quaternion.identity, headMat);   // dno panvice
        Add(root, PrimitiveType.Cylinder, new Vector3(0,  0.07f, 0), new Vector3(0.62f, 0.02f, 0.62f), Quaternion.identity, handleMat); // okraj
    }

    // --- Trubkový kľúč: rukoväť + pevná a pohyblivá čeľusť (tvar C) ---
    static void BuildWrench(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.18f, 0), new Vector3(0.10f, 0.42f, 0.10f), Quaternion.identity, handleMat); // rukoväť
        Add(root, PrimitiveType.Cube,     new Vector3(0,  0.34f, 0), new Vector3(0.16f, 0.20f, 0.14f), Quaternion.identity, headMat);   // hlava
        Add(root, PrimitiveType.Cube,     new Vector3(-0.12f, 0.52f, 0), new Vector3(0.20f, 0.10f, 0.12f), Quaternion.Euler(0,0,20f), headMat); // horná čeľusť
        Add(root, PrimitiveType.Cube,     new Vector3(-0.12f, 0.30f, 0), new Vector3(0.16f, 0.09f, 0.12f), Quaternion.Euler(0,0,-12f), headMat); // dolná čeľusť
    }

    // --- Golfová palica: dlhý tenký driek + uhlová hlava ---
    static void BuildGolf(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Cylinder, new Vector3(0, 0.36f, 0), new Vector3(0.045f, 0.62f, 0.045f), Quaternion.identity, handleMat); // grip+driek
        Add(root, PrimitiveType.Cylinder, new Vector3(0, 0.66f, 0), new Vector3(0.06f, 0.14f, 0.06f), Quaternion.identity, handleMat);   // grip vrch
        Add(root, PrimitiveType.Cube,     new Vector3(0.06f, -0.28f, 0), new Vector3(0.22f, 0.12f, 0.14f), Quaternion.Euler(0,0,-22f), headMat); // hlava (wood)
    }

    // --- Krompáč: násada + zahnutý hrot na oboch stranách ---
    static void BuildPickaxe(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.10f, 0), new Vector3(0.07f, 0.52f, 0.07f), Quaternion.identity, handleMat); // násada
        Add(root, PrimitiveType.Cylinder, new Vector3(0.30f, 0.45f, 0), new Vector3(0.06f, 0.32f, 0.06f), Quaternion.Euler(0,0,80f), headMat); // pravý hrot
        Add(root, PrimitiveType.Cylinder, new Vector3(-0.30f, 0.45f, 0), new Vector3(0.06f, 0.32f, 0.06f), Quaternion.Euler(0,0,100f), headMat); // ľavý hrot
        Add(root, PrimitiveType.Cube,     new Vector3(0, 0.45f, 0), new Vector3(0.10f, 0.12f, 0.10f), Quaternion.identity, headMat); // stred
    }

    // --- Palcát: násada + ostnatá guľa ---
    static void BuildMace(GameObject root, Material handleMat, Material headMat)
    {
        Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.18f, 0), new Vector3(0.08f, 0.42f, 0.08f), Quaternion.identity, handleMat); // násada
        Add(root, PrimitiveType.Sphere,   new Vector3(0, 0.42f, 0), Vector3.one * 0.42f, Quaternion.identity, headMat); // guľa
        // osem ostňov
        Vector3[] dirs = {
            new Vector3(0,1,0), new Vector3(0,-1,0), new Vector3(1,0,0), new Vector3(-1,0,0),
            new Vector3(0,0,1), new Vector3(0,0,-1), new Vector3(0.7f,0.7f,0), new Vector3(-0.7f,0.7f,0)
        };
        foreach (var d in dirs)
        {
            Vector3 dir = d.normalized;
            var rot = Quaternion.FromToRotation(Vector3.up, dir);
            Add(root, PrimitiveType.Cube, new Vector3(0,0.42f,0) + dir * 0.26f, new Vector3(0.10f, 0.16f, 0.10f), rot, headMat);
        }
    }

    static GameObject Add(GameObject root, PrimitiveType t, Vector3 pos, Vector3 scale, Quaternion rot, Material m)
    {
        var g = GameObject.CreatePrimitive(t);
        var col = g.GetComponent<Collider>();
        if (col != null) { if (Application.isPlaying) Destroy(col); else DestroyImmediate(col); }
        g.transform.SetParent(root.transform, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.transform.localRotation = rot;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader")
            m = new Material(Shader.Find("Standard"));
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
}
