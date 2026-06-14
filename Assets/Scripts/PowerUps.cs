using System.Collections;
using UnityEngine;

/// Power-upy padajuce z rozbitych veci. Kazdy druh ma vlastny tvar a farbu,
/// aby bolo na prvy pohlad jasne, ze ide o bonus (nie o nahodnu kocku).
/// Rage = x2 damage, Frenzy = slow-mo, Cash = bonus penazi, Quake = plosny smash.
public class PowerUps : MonoBehaviour
{
    public enum Kind { Rage, Frenzy, Cash, Quake }
    static PowerUps inst;

    // Kolko pickupov je prave v scene (cap, nech sa nehromadia).
    public static int ActiveCount = 0;
    const int MAX_ACTIVE = 4;

    float rageTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (inst != null) return;
        var go = new GameObject("PowerUps");
        DontDestroyOnLoad(go);
        inst = go.AddComponent<PowerUps>();
    }

    void Update()
    {
        if (rageTimer > 0f) rageTimer -= Time.deltaTime;
    }

    /// Aktualny damage multiplikator (Rage).
    public static float DamageMult() => (inst != null && inst.rageTimer > 0f) ? 2f : 1f;

    /// Z rozbitia obcas vypadne power-up (menej casto + s capom na pocet).
    public static void MaybeDrop(Vector3 pos)
    {
        if (inst == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.roundActive) return;
        if (ActiveCount >= MAX_ACTIVE) return;
        if (Random.value > 0.028f) return;
        Kind k = (Kind)Random.Range(0, 4);
        inst.Spawn(k, pos + Vector3.up * 0.7f);
    }

    void Spawn(Kind k, Vector3 pos)
    {
        // Root = neviditelny drziak (rotuje a vznasa sa). Tvar je dieta.
        var go = new GameObject("PowerUp");
        go.transform.position = pos;

        var body = BuildShape(k);
        body.transform.SetParent(go.transform, false);
        body.transform.localPosition = Vector3.zero;

        // Svetelny prstenec pod predmetom (halo), aby ziaril aj na zemi.
        var lgo = new GameObject("PU_Light");
        lgo.transform.SetParent(go.transform, false);
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = Tint(k); l.range = 3.8f; l.intensity = 2.6f;

        var p = go.AddComponent<PowerUpPickup>();
        p.kind = k;
        p.body = body.transform;
    }

    /// Postavi tvar specificky pre druh power-upu (citatelnost).
    static GameObject BuildShape(Kind k)
    {
        var mat = EmissiveMat(Tint(k));
        GameObject g;
        switch (k)
        {
            case Kind.Rage:   // cervena gula = sila
                g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                g.transform.localScale = Vector3.one * 0.5f;
                break;
            case Kind.Frenzy: // modra kapsula = "lektvar" spomalenia
                g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                g.transform.localScale = new Vector3(0.34f, 0.34f, 0.34f);
                break;
            case Kind.Cash:   // zlata minca = peniaze
                g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                g.transform.localScale = new Vector3(0.52f, 0.07f, 0.52f);
                g.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                break;
            default:          // Quake: fialova kocka-diamant = otras
                g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.transform.localScale = Vector3.one * 0.46f;
                g.transform.localEulerAngles = new Vector3(45f, 0f, 45f);
                break;
        }
        g.name = "PU_Body";
        var col = g.GetComponent<Collider>(); if (col != null) Destroy(col);
        g.GetComponent<Renderer>().material = mat;
        return g;
    }

    public static void Collect(Kind k, Vector3 at)
    {
        if (inst != null) inst.Activate(k, at);
    }

    void Activate(Kind k, Vector3 at)
    {
        switch (k)
        {
            case Kind.Rage:
                rageTimer = 6f;
                Show("RAGE!  x2 DAMAGE");
                break;
            case Kind.Frenzy:
                StartCoroutine(Frenzy());
                Show("FRENZY!  SLOW MOTION");
                break;
            case Kind.Cash:
                if (GameManager.Instance != null) GameManager.Instance.AddRoundMoney(150);
                if (SfxManager.Instance != null) SfxManager.Coin();
                Show("CASH!  +$150");
                break;
            case Kind.Quake:
                Quake(at);
                Show("QUAKE!");
                break;
        }
    }

    IEnumerator Frenzy()
    {
        Time.timeScale = 0.45f;
        float t = 0f;
        while (t < 4f) { t += Time.unscaledDeltaTime; yield return null; }
        // neobnovuj ak je hra pauznuta (nech to neodpauzne)
        var pm = FindFirstObjectByType<PauseMenu>();
        if ((pm == null || !pm.IsPaused) && Time.timeScale < 1f) Time.timeScale = 1f;
    }

    void Quake(Vector3 at)
    {
        var cam = Camera.main;
        Vector3 center = cam != null ? cam.transform.position : at;
        CameraShaker.Shake(0.4f, 0.5f);
        if (SfxManager.Instance != null) SfxManager.Boom(center);
        foreach (var c in Physics.OverlapSphere(center, 5.5f))
        {
            if (c == null) continue;
            var b = c.GetComponent<Breakable>();
            if (b == null) continue;
            int orig = b.damage;
            b.damage = 999;
            b.Hit(b.transform.position, (b.transform.position - center).normalized);
            if (b != null) b.damage = orig;
        }
    }

    void Show(string s)
    {
        if (Announcer.Instance != null) Announcer.Show(s, true);
    }

    static Color Tint(Kind k)
        => k == Kind.Rage   ? new Color(1f, 0.2f, 0.15f)
         : k == Kind.Frenzy ? new Color(0.3f, 0.8f, 1f)
         : k == Kind.Cash   ? new Color(1f, 0.82f, 0.15f)
                            : new Color(0.7f, 0.35f, 1f);

    static Material EmissiveMat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 2.2f); }
        return m;
    }
}

/// Spravanie pickupu: vznasa sa, otaca, pri priblizeni hraca sa rozleti k nemu
/// (magnet) a aktivuje efekt. Kratsia zivotnost, nech zostava prehladne.
public class PowerUpPickup : MonoBehaviour
{
    public PowerUps.Kind kind;
    public Transform body;
    float baseY;
    float life = 14f;
    bool collected;

    void Start()
    {
        baseY = transform.position.y;
        PowerUps.ActiveCount++;
    }

    void OnDestroy()
    {
        PowerUps.ActiveCount--;
    }

    void Update()
    {
        // Telo sa stale otaca (citatelne ako bonus).
        if (body != null)
            body.Rotate(0f, 110f * Time.unscaledDeltaTime, 0f, Space.World);

        var cam = Camera.main;
        if (cam != null)
        {
            float d = Vector3.Distance(cam.transform.position, transform.position);

            // Magnet: ked je hrac blizko, predmet sa k nemu rozletí.
            if (d < 6f)
            {
                Vector3 target = cam.transform.position - Vector3.up * 0.3f;
                transform.position = Vector3.MoveTowards(transform.position, target,
                                       (8f - d) * 1.6f * Time.unscaledDeltaTime);
            }
            else
            {
                var p = transform.position;
                p.y = baseY + Mathf.Sin(Time.unscaledTime * 2.5f) * 0.14f;
                transform.position = p;
            }

            if (!collected && d < 1.7f)
            {
                collected = true;
                PowerUps.Collect(kind, transform.position);
                Destroy(gameObject);
                return;
            }
        }

        life -= Time.unscaledDeltaTime;
        if (life <= 0f) Destroy(gameObject);
    }
}
