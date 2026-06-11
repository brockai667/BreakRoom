using UnityEngine;

/// Rozbitná vec. Veľké veci sa rozbijú na menšie kúsky, ktoré sa dajú
/// rozbíjať ďalej (viacstupňové ničenie). Odmena (peniaze) aj XP sa
/// odvíjajú od veľkosti veci, takže väčšie/ťažšie veci platia viac a
/// celkové zarobené peniaze dávajú zmysel.
public class Breakable : MonoBehaviour
{
    public int hp            = 3;
    public int damage        = 1;

    [Header("Hodnoty (-1 = automaticky podľa veľkosti)")]
    public int xpValue       = -1;   // XP po rozbití
    public int reward        = -1;   // peniaze po rozbití
    public int fragmentCount = 6;    // počet vizuálnych úlomkov

    [Header("Viacstupňové ničenie")]
    public int subdivideStages = -1; // -1 = automaticky podľa veľkosti; 0 = už sa ďalej nedelí
    public int childPieces     = 0;  // koľko menších rozbitných kúskov vznikne

    // Úlomok vytvorený za behu má hodnoty nastavené priamo (nededuj zo
    // serializácie). [NonSerialized] => scénové objekty to majú vždy false.
    [System.NonSerialized] public bool isChunk = false;

    // Špeciálne typy (nastavuje SpecialObjects za behu)
    [System.NonSerialized] public bool golden;      // bonusové peniaze
    [System.NonSerialized] public bool explosive;   // výbuch + reťazová reakcia
    [System.NonSerialized] public bool electronic;  // iskry pri rozbití

    bool   broken;      // už rozbité? (proti dvojitému rozbitiu / reťazi)
    bool   configured;
    bool   counted;     // zaregistrovaná v GameManager.liveBreakables?
    Color  baseColor = new Color(0.6f, 0.6f, 0.62f);
    Vector3 worldSize = Vector3.one;

    void Start() { Configure(); }

    void OnDestroy()
    {
        if (counted && GameManager.Instance != null) GameManager.Instance.UnregisterBreakable();
    }

    // Zistí farbu/veľkosť. Pre scénové veci odvodí odmenu/XP/delenie z
    // veľkosti (raz). Úlomky majú hodnoty nastavené ručne -> nechá ich tak.
    void Configure()
    {
        if (configured) return;
        configured = true;

        // Zaregistruj sa do počtu živých vecí (kúsky sú predregistrované v SpawnChunks)
        if (!counted && GameManager.Instance != null)
        { GameManager.Instance.RegisterBreakable(); counted = true; }

        var rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;

        var coll = GetComponent<Collider>();
        worldSize = coll != null ? coll.bounds.size
                  : rend != null ? rend.bounds.size
                  : transform.lossyScale;
        worldSize = new Vector3(Mathf.Max(0.05f, worldSize.x),
                                Mathf.Max(0.05f, worldSize.y),
                                Mathf.Max(0.05f, worldSize.z));
        float maxDim = Mathf.Max(worldSize.x, worldSize.y, worldSize.z);

        if (isChunk) return;   // hodnoty už nastavené ručne

        // --- Scénový objekt: odvoď všetko z veľkosti ---
        // Stupne delenia: väčšie veci sa rozbijú na viac menších kúskov
        if      (maxDim >= 1.3f) { subdivideStages = 2; childPieces = 4; }
        else if (maxDim >= 0.7f) { subdivideStages = 1; childPieces = 3; }
        else                     { subdivideStages = 0; childPieces = 0; }

        // Odmena a XP podľa veľkosti (väčšie = viac peňazí -> platenie sedí)
        reward  = Mathf.Max(2, Mathf.RoundToInt(3f + maxDim * 6f));
        xpValue = Mathf.Max(5, Mathf.RoundToInt(6f + maxDim * 10f));
    }

    public void Hit(Vector3 hitPoint, Vector3 swingDir)
    {
        if (broken) return;
        if (!configured) Configure();
        hp -= damage;
        StartCoroutine(ShakeOnHit());
        if (hp <= 0) Break(hitPoint, swingDir);
        else SfxManager.Hit(hitPoint);
    }

    public void Hit() { Hit(transform.position, Vector3.forward); }

    void Break(Vector3 hitPoint, Vector3 swingDir)
    {
        if (broken) return;
        broken = true;

        int pay = golden ? reward * 5 : reward;
        if (XPManager.Instance != null) XPManager.Instance.AddXP(xpValue);
        if (GameManager.Instance != null)
        { GameManager.Instance.AddRoundMoney(pay); GameManager.Instance.RegisterDestroy(); }
        else if (PlayerInventory.Instance != null) PlayerInventory.Instance.AddMoney(pay);

        // Zvuk + hlášky
        SfxManager.Break(hitPoint);
        if (golden)
        {
            SfxManager.Coin();
            if (Announcer.Instance != null) Announcer.Show("ZLATÁ VEC!  +$" + pay, true);
        }
        else if (!isChunk && Random.value < 0.12f)
            Announcer.Smash();                       // občasná smash hláška

        // Špeciálne efekty
        if (electronic) { Fx.Sparks(transform.position, baseColor); SfxManager.Zap(transform.position); }
        if (explosive)  Explode(transform.position);

        // Veľké veci -> menšie rozbitné kúsky (výbušné sa rovno vyparia)
        if (!explosive && subdivideStages > 0 && childPieces > 0)
            SpawnChunks(hitPoint, swingDir);

        SpawnFragments(hitPoint, swingDir);
        Destroy(gameObject);
    }

    // Výbuch: reťazová reakcia, otras kamery, bonus.
    void Explode(Vector3 center)
    {
        SfxManager.Boom(center);
        CameraShaker.Shake(0.35f, 0.45f);
        Fx.Explosion(center);
        if (Announcer.Instance != null) Announcer.Show("BOOM!", true);
        if (GameManager.Instance != null) GameManager.Instance.AddRoundMoney(15);

        const float R = 3.2f;
        foreach (var col in Physics.OverlapSphere(center, R))
        {
            if (col == null) continue;
            var rb = col.attachedRigidbody;
            if (rb != null) rb.AddExplosionForce(650f, center, R, 1.8f);
            var b = col.GetComponent<Breakable>();
            if (b != null && b != this) b.KillFromExplosion(center);
        }
    }

    // Zničenie výbuchom suseda (reťazí ďalšie výbuchy).
    public void KillFromExplosion(Vector3 source)
    {
        if (broken || this == null) return;
        if (!configured) Configure();
        Vector3 dir = (transform.position - source).normalized;
        hp = 0;
        Break(transform.position, dir);
    }

    // Menšie kúsky, ktoré sa dajú rozbíjať ďalej (o stupeň menej)
    void SpawnChunks(Vector3 hitPoint, Vector3 swingDir)
    {
        float maxDim = Mathf.Max(worldSize.x, worldSize.y, worldSize.z);
        int   nextStage  = subdivideStages - 1;
        int   nextPieces = Mathf.Max(2, childPieces - 1);
        int   childReward = Mathf.Max(1, reward / 4);
        int   childXp     = Mathf.Max(2, xpValue / 3);

        for (int i = 0; i < childPieces; i++)
        {
            Vector3 cs = new Vector3(
                Mathf.Max(0.12f, worldSize.x * Random.Range(0.35f, 0.55f)),
                Mathf.Max(0.12f, worldSize.y * Random.Range(0.35f, 0.55f)),
                Mathf.Max(0.12f, worldSize.z * Random.Range(0.35f, 0.55f)));

            Vector3 scatter = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f))
                              .normalized * maxDim * 0.25f;

            var chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunk.name = "Chunk";
            chunk.transform.position   = transform.position + scatter + Vector3.up * 0.05f;
            chunk.transform.localScale = cs;
            chunk.transform.rotation   = Random.rotation;

            var mat = NewMat();
            float d = Random.Range(0.78f, 1.05f);
            mat.color = new Color(Mathf.Clamp01(baseColor.r * d), Mathf.Clamp01(baseColor.g * d), Mathf.Clamp01(baseColor.b * d));
            ApplyColor(mat);
            chunk.GetComponent<Renderer>().material = mat;

            var rb = chunk.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Vector3 outward = (chunk.transform.position - hitPoint).normalized;
            Vector3 force = (outward * 1.4f + swingDir * 2f + Vector3.up * Random.Range(0.6f, 1.6f)).normalized;
            rb.AddForce(force * Random.Range(180f, 360f));
            rb.AddTorque(Random.insideUnitSphere * 220f);

            var b = chunk.AddComponent<Breakable>();
            b.isChunk = true;                     // hodnoty nastavené ručne
            b.hp = 1;
            b.damage = 1;
            b.reward = childReward;
            b.xpValue = childXp;
            b.fragmentCount = 4;
            b.subdivideStages = nextStage;        // bounded -> nakoniec 0
            b.childPieces = nextStage > 0 ? nextPieces : 0;

            // Predregistruj hneď (nie až v Start), aby auto-koniec nefalšoval
            // počas jedného snímku medzi zánikom starého kusu a vznikom nových
            b.counted = true;
            if (GameManager.Instance != null) GameManager.Instance.RegisterBreakable();

            Destroy(chunk, 40f);                  // upracenie, ak ich hráč nerozbije
        }
    }

    // Vizuálne úlomky (nerozbitné, zmiznú)
    void SpawnFragments(Vector3 hitPoint, Vector3 swingDir)
    {
        Vector3 size = worldSize;
        float maxDim = Mathf.Max(size.x, size.y, size.z);

        for (int i = 0; i < fragmentCount; i++)
        {
            float s = Random.Range(0.20f, 0.5f);
            Vector3 fragScale = new Vector3(
                Mathf.Max(0.04f, size.x * s * Random.Range(0.5f, 1.1f)),
                Mathf.Max(0.04f, size.y * s * Random.Range(0.5f, 1.1f)),
                Mathf.Max(0.04f, size.z * s * Random.Range(0.5f, 1.1f)));

            Vector3 scatter = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f))
                              .normalized * maxDim * 0.35f;

            var frag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frag.transform.position   = transform.position + scatter;
            frag.transform.localScale = fragScale;
            frag.transform.rotation   = Random.rotation;

            var mat = NewMat();
            float d = Random.Range(0.72f, 1.05f);
            mat.color = new Color(Mathf.Clamp01(baseColor.r * d), Mathf.Clamp01(baseColor.g * d), Mathf.Clamp01(baseColor.b * d));
            ApplyColor(mat);
            frag.GetComponent<Renderer>().material = mat;

            var rb = frag.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Vector3 outward  = (frag.transform.position - hitPoint).normalized;
            Vector3 forceDir = (outward * 1.5f + swingDir * 2.5f + Vector3.up * Random.Range(0.4f, 1.4f)).normalized;
            rb.AddForce(forceDir * Random.Range(380f, 720f));
            rb.AddTorque(new Vector3(Random.Range(-400f, 400f), Random.Range(-400f, 400f), Random.Range(-400f, 400f)));

            Destroy(frag, 7f);
        }
    }

    static Material NewMat()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader") mat = new Material(Shader.Find("Standard"));
        return mat;
    }

    static void ApplyColor(Material m)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", m.color);
    }

    System.Collections.IEnumerator ShakeOnHit()
    {
        // Voľný kúsok s fyzikou netras (bil by sa to s Rigidbody)
        var rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic) yield break;

        Vector3 origin = transform.localPosition;
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            transform.localPosition = origin + new Vector3(
                Random.Range(-0.03f, 0.03f),
                Random.Range(-0.03f, 0.03f),
                Random.Range(-0.03f, 0.03f));
            yield return null;
        }
        if (this != null) transform.localPosition = origin;
    }
}
