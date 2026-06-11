using UnityEngine;

/// Vizuálne efekty pre špeciálne objekty: výbuch a elektrické iskry.
/// Všetko sa tvorí z primitívov/častíc za behu, žiadne assety netreba.
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

    /// Výbuch: záblesk svetla + rozpínajúca sa žiariaca guľa + oblak častíc.
    public static void Explosion(Vector3 center)
    {
        // svetlo
        var lgo = new GameObject("ExplosionLight");
        lgo.transform.position = center;
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.6f, 0.2f);
        l.range = 9f; l.intensity = 8f;

        // žiariaca guľa
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var col = s.GetComponent<Collider>(); if (col != null) Object.Destroy(col);
        s.transform.position = center; s.transform.localScale = Vector3.one * 0.4f;
        s.GetComponent<Renderer>().material = EmissiveMat(new Color(1f, 0.55f, 0.12f));
        s.AddComponent<FxAnim>().Init(FxAnim.Kind.Explosion, l);

        // častice
        Burst(center, 60, new Color(1f, 0.5f, 0.1f), 6f, 0.55f, 0.16f);
    }

    /// Elektrické iskry pri rozbití elektroniky.
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

/// Animuje žiariacu guľu výbuchu (rozpne sa a zhasne) a zhasne svetlo.
public class FxAnim : MonoBehaviour
{
    public enum Kind { Explosion }
    Kind kind; Light light; float t; Renderer rend; Vector3 start;

    public void Init(Kind k, Light l)
    {
        kind = k; light = l; rend = GetComponent<Renderer>();
        start = transform.localScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / 0.4f;
        transform.localScale = start + Vector3.one * Mathf.SmoothStep(0f, 4.5f, k);
        if (light != null) light.intensity = Mathf.Lerp(8f, 0f, k);
        if (rend != null)
        {
            var c = rend.material.color; c.a = Mathf.Clamp01(1f - k);
            rend.material.color = c;
        }
        if (k >= 1f)
        {
            if (light != null) Destroy(light.gameObject);
            Destroy(gameObject);
        }
    }
}
