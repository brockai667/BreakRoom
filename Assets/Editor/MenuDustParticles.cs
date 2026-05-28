using UnityEngine;

/// <summary>
/// Dust / ember particle effect pre MainMenu pozadie.
/// Pridaj tento skript na nový prázdny GameObject "DustParticles" v MainMenu scéne.
/// POŽIADAVKA: Canvas musí byť na Render Mode = "Screen Space - Camera"
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class MenuDustParticles : MonoBehaviour
{
    [Header("Množstvo")]
    public int maxParticles = 80;
    public float emissionRate = 8f;

    [Header("Pohyb")]
    public float minSpeed = 0.05f;
    public float maxSpeed = 0.3f;
    public float driftStrength = 0.15f;   // horizontálne kolísanie

    [Header("Veľkosť")]
    public float minSize = 0.03f;
    public float maxSize = 0.09f;

    [Header("Farby — ember / dust")]
    public Color colorEmber = new Color(1f, 0.75f, 0.2f, 1f);   // zlatá iskra
    public Color colorDust  = new Color(0.9f, 0.85f, 1.0f, 1f); // bielo-modrý prach

    [Header("Plocha generovania (šírka x výška)")]
    public float spawnWidth  = 20f;
    public float spawnY      = -7f;   // spodok obrazovky (world space)

    // ----------------------------------------------------------------

    void Awake()
    {
        ConfigureParticles();
    }

    void ConfigureParticles()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        // ── MAIN ─────────────────────────────────────────────────────
        var main = ps.main;
        main.loop             = true;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize        = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor       = new ParticleSystem.MinMaxGradient(colorEmber, colorDust);
        main.maxParticles     = maxParticles;
        main.gravityModifier  = -0.015f;   // mierne stúpanie

        // ── EMISSION ──────────────────────────────────────────────────
        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        // ── SHAPE — horizont. pás na spodku ──────────────────────────
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(spawnWidth, 0.2f, 0.1f);
        shape.position  = new Vector3(0f, spawnY, 0f);

        // ── VELOCITY OVER LIFETIME — organický drift ──────────────────
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(-driftStrength, driftStrength);
        vel.y       = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
        vel.z       = new ParticleSystem.MinMaxCurve(0f, 0f);

        // ── NOISE — prirodzené kolísanie ──────────────────────────────
        var noise = ps.noise;
        noise.enabled     = true;
        noise.strength    = new ParticleSystem.MinMaxCurve(0.25f);
        noise.frequency   = 0.4f;
        noise.scrollSpeed = 0.08f;
        noise.damping     = true;

        // ── COLOR OVER LIFETIME — fade in → svit → fade out ──────────
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(colorEmber,            0.0f),
                new GradientColorKey(Color.white,           0.4f),
                new GradientColorKey(colorDust,             0.75f),
                new GradientColorKey(colorEmber,            1.0f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.0f,  0.00f),
                new GradientAlphaKey(0.75f, 0.12f),
                new GradientAlphaKey(0.55f, 0.70f),
                new GradientAlphaKey(0.0f,  1.00f),
            }
        );
        col.color = grad;

        // ── SIZE OVER LIFETIME — narástne, potom zmenší ───────────────
        var sz = ps.sizeOverLifetime;
        sz.enabled = true;
        var szCurve = new AnimationCurve(
            new Keyframe(0.00f, 0.0f, 0f, 4f),
            new Keyframe(0.15f, 1.0f, 0f, 0f),
            new Keyframe(0.85f, 0.8f, 0f, 0f),
            new Keyframe(1.00f, 0.0f, -4f, 0f)
        );
        sz.size = new ParticleSystem.MinMaxCurve(1f, szCurve);

        // ── ROTATION OVER LIFETIME — pomalé točenie ───────────────────
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-25f * Mathf.Deg2Rad, 25f * Mathf.Deg2Rad);

        // ── RENDERER ──────────────────────────────────────────────────
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode  = ParticleSystemRenderMode.Billboard;
        rend.sortingOrder = 1;  // medzi BackgroundCanvas (0) a UICanvas (2)

        // Skús nájsť vhodný shader (Universal Render Pipeline alebo fallback)
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit")
                     ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        if (shader != null)
        {
            rend.material = new Material(shader);
        }
        else
        {
            Debug.LogWarning("[MenuDustParticles] Nepodarilo sa nájsť particle shader. " +
                             "Prirad material manuálne v Inspectore.");
        }
    }
}
