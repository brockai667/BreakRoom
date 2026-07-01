using UnityEngine;

/// Vizualne efekty pre specialne objekty: vybuch, iskry a prach.
/// Vsetko sa tvori z primitivov/castic za behu, ziadne assety netreba.
public static class Fx
{
    static Material EmissiveMat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 2.5f); }
        return m;
    }

    /// Vybuch: zablesk svetla + rozpinajuca sa ziariaca gula + oblak castic.
    public static void Explosion(Vector3 center)
    {
        var lgo = new GameObject("ExplosionLight");
        lgo.transform.position = center;
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.6f, 0.2f);
        l.range = 9f; l.intensity = 8f;

        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var col = s.GetComponent<Collider>(); if (col != null) Object.Destroy(col);
        s.transform.position = center; s.transform.localScale = Vector3.one * 0.4f;
        s.GetComponent<Renderer>().material = EmissiveMat(new Color(1f, 0.55f, 0.12f));
        s.AddComponent<FxAnim>().Init(FxAnim.Kind.Explosion, l);

        Burst(center, 60, new Color(1f, 0.5f, 0.1f), 6f, 0.55f, 0.16f);
    }

    /// Elektricke iskry pri rozbiti elektroniky.
    public static void Sparks(Vector3 center, Color tint)
    {
        Burst(center, 26, new Color(0.6f, 0.85f, 1f), 5f, 0.30f, 0.05f);
        var lgo = new GameObject("SparkLight");
        lgo.transform.position = center;
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(0.6f, 0.85f, 1f);
        l.range = 4f; l.intensity = 4f;
        Object.Destroy(lgo, 0.18f);
    }

    /// Trieštenie skla: ostrý svetlomodrý záblesk + rozletujúce sa črepiny.
    public static void Glass(Vector3 center)
    {
        Burst(center, 30, new Color(0.8f, 0.92f, 1f), 6.5f, 0.4f, 0.06f);

        var lgo = new GameObject("GlassLight");
        lgo.transform.position = center;
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(0.8f, 0.92f, 1f); l.range = 3f; l.intensity = 2.5f;
        Object.Destroy(lgo, 0.12f);

        var shardMat = EmissiveMat(new Color(0.78f, 0.9f, 1f));
        for (int i = 0; i < 7; i++)
        {
            var sh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var c = sh.GetComponent<Collider>(); if (c != null) Object.Destroy(c);
            sh.transform.position = center + Random.insideUnitSphere * 0.15f;
            sh.transform.localScale = new Vector3(Random.Range(0.03f, 0.07f), Random.Range(0.06f, 0.14f), 0.012f);
            sh.transform.rotation = Random.rotation;
            sh.GetComponent<Renderer>().sharedMaterial = shardMat;
            var rb = sh.AddComponent<Rigidbody>(); rb.mass = 0.05f;
            rb.AddForce((Random.insideUnitSphere + Vector3.up).normalized * Random.Range(2f, 4f), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f);
            Object.Destroy(sh, 2.5f);
        }
    }

    /// Prachovy oblak pri rozbiti (mierna siva/hneda hmla).
    public static void Dust(Vector3 center, Color tint)
    {
        Color c = Color.Lerp(tint, new Color(0.62f, 0.57f, 0.5f), 0.6f);
        Burst(center, 16, c, 2.0f, 0.7f, 0.22f);
    }

    static void Burst(Vector3 pos, int count, Color color, float speed, float life, float size)
    {
        var go = new GameObject("Burst");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop();
        var main = ps.main;
        main.startColor = color; main.startSpeed = speed; main.startSize = size;
        main.startLifetime = life; main.maxParticles = count + 10;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission; em.enabled = true; em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.15f;
        var col = ps.colorOverLifetime; col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color * 0.6f, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var psr = go.GetComponent<ParticleSystemRenderer>();
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        psr.material = new Material(shader) { color = color };

        ps.Play();
        Object.Destroy(go, life + 0.5f);
    }
}

/// Animuje ziariacu gulu vybuchu (rozpne sa a zhasne) a zhasne svetlo.
public class FxAnim : MonoBehaviour
{
    /// Typ animácie (zatiaľ len Explosion).
    public enum Kind { Explosion }
    Kind kind; Light glowLight; float t; Renderer rend; Vector3 start;

    /// Naštartuje animáciu daného typu, prepojí sa so svetlom vybuchu (zhasína spolu s ňou).
    public void Init(Kind k, Light l)
    {
        kind = k; glowLight = l; rend = GetComponent<Renderer>();
        start = transform.localScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / 0.4f;
        transform.localScale = start + Vector3.one * Mathf.SmoothStep(0f, 4.5f, k);
        if (glowLight != null) glowLight.intensity = Mathf.Lerp(8f, 0f, k);
        if (rend != null)
        {
            var c = rend.material.color; c.a = Mathf.Clamp01(1f - k);
            rend.material.color = c;
        }
        if (k >= 1f)
        {
            if (glowLight != null) Destroy(glowLight.gameObject);
            Destroy(gameObject);
        }
    }
}
