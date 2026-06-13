using System.Collections;
using UnityEngine;

/// Power-upy padajuce z rozbitych veci. Pickup sa zoberie priblizenim hraca.
/// Rage = x2 damage, Frenzy = slow-mo, Cash = bonus penazi, Quake = plosny smash.
public class PowerUps : MonoBehaviour
{
    public enum Kind { Rage, Frenzy, Cash, Quake }
    static PowerUps inst;

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

    /// Z rozbitia obcas vypadne power-up.
    public static void MaybeDrop(Vector3 pos)
    {
        if (inst == null) return;
        if (GameManager.Instance == null || !GameManager.Instance.roundActive) return;
        if (Random.value > 0.045f) return;
        Kind k = (Kind)Random.Range(0, 4);
        inst.Spawn(k, pos + Vector3.up * 0.6f);
    }

    void Spawn(Kind k, Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "PowerUp";
        var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.42f;
        go.GetComponent<Renderer>().material = EmissiveMat(Tint(k));
        var p = go.AddComponent<PowerUpPickup>();
        p.kind = k;

        var lgo = new GameObject("PU_Light");
        lgo.transform.SetParent(go.transform, false);
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = Tint(k); l.range = 3.5f; l.intensity = 2.2f;
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

/// Spravanie pickupu: vznasa sa, otaca, a pri priblizeni hraca aktivuje efekt.
public class PowerUpPickup : MonoBehaviour
{
    public PowerUps.Kind kind;
    float baseY;
    float life = 25f;

    void Start() { baseY = transform.position.y; }

    void Update()
    {
        transform.Rotate(0f, 90f * Time.unscaledDeltaTime, 0f, Space.World);
        var p = transform.position;
        p.y = baseY + Mathf.Sin(Time.unscaledTime * 2.5f) * 0.12f;
        transform.position = p;

        var cam = Camera.main;
        if (cam != null && Vector3.Distance(cam.transform.position, transform.position) < 1.7f)
        {
            PowerUps.Collect(kind, transform.position);
            Destroy(gameObject);
            return;
        }

        life -= Time.unscaledDeltaTime;
        if (life <= 0f) Destroy(gameObject);
    }
}
