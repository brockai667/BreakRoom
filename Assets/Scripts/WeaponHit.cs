using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHit : MonoBehaviour
{
    public Camera playerCamera;
    public float hitDistance = 4f;

    private int   damage     = 1;
    private float splash     = 0f;
    private float swingSpeed = 1f;
    private bool  isFlamethrower = false;
    private int   layerMask;
    private HandDisplay handDisplay;
    private PauseMenu   pauseMenu;
    private float cooldown = 0f;

    // Plameňomet
    private ParticleSystem flameFX;
    private bool  spraying = false;
    private float fireTick = 0f;

    void Start()
    {
        layerMask = ~LayerMask.GetMask("Player");
        if (PlayerInventory.Instance != null)
            ApplyWeapon(PlayerInventory.Instance.GetEquipped());
        handDisplay = FindObjectOfType<HandDisplay>();
        pauseMenu   = FindObjectOfType<PauseMenu>();
        BuildFlameFX();
    }

    public void ApplyWeapon(WeaponData w)
    {
        damage         = w.damage;
        splash         = w.splashRadius;
        hitDistance    = w.hitDistance;
        swingSpeed     = w.swingSpeed;
        isFlamethrower = w.id == "flamethrower";
        if (!isFlamethrower) StopSpray();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) { StopSpray(); return; }
        if (pauseMenu == null) pauseMenu = FindObjectOfType<PauseMenu>();
        if (pauseMenu != null && pauseMenu.IsPaused) { StopSpray(); return; }
        if (Mouse.current == null) return;

        if (isFlamethrower) { HandleFlamethrower(); return; }

        // Bežné zbrane: klik + cooldown podľa rýchlosti švihu
        if (cooldown > 0f) { cooldown -= Time.deltaTime; return; }
        if (Mouse.current.leftButton.wasPressedThisFrame) Swing();
    }

    // ---------- PLAMEŇOMET ----------
    void HandleFlamethrower()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            if (!spraying) { spraying = true; if (flameFX != null) flameFX.Play(); }
            fireTick -= Time.deltaTime;
            if (fireTick <= 0f) { fireTick = 0.08f; SprayDamage(); }
        }
        else StopSpray();
    }

    void StopSpray()
    {
        if (spraying) { spraying = false; if (flameFX != null) flameFX.Stop(); }
    }

    void SprayDamage()
    {
        if (playerCamera == null) return;
        Vector3 fwd = playerCamera.transform.forward;
        Vector3 center = playerCamera.transform.position + fwd * (hitDistance * 0.6f);
        float radius = Mathf.Max(1.0f, splash);
        foreach (var col in Physics.OverlapSphere(center, radius, layerMask))
        {
            var b = col.GetComponent<Breakable>();
            if (b != null) HitBreakable(b, col.transform.position, fwd);
        }
    }

    void BuildFlameFX()
    {
        if (playerCamera == null) return;
        var go = new GameObject("FlameFX");
        go.transform.SetParent(playerCamera.transform, false);
        go.transform.localPosition = new Vector3(0.25f, -0.25f, 0.6f);
        go.transform.localRotation = Quaternion.identity;

        flameFX = go.AddComponent<ParticleSystem>();
        flameFX.Stop();

        var main = flameFX.main;
        main.startColor      = new Color(1f, 0.55f, 0.1f);
        main.startSize       = 0.35f;
        main.startSpeed      = 7f;
        main.startLifetime   = 0.32f;
        main.maxParticles    = 600;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = flameFX.emission; em.rateOverTime = 130f;

        var sh = flameFX.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 14f; sh.radius = 0.06f;

        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.85f, 0.35f), 0f),
                new GradientColorKey(new Color(1f, 0.4f, 0.05f), 0.5f),
                new GradientColorKey(new Color(0.35f, 0.08f, 0.05f), 1f)
            },
            new[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.7f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        var col = flameFX.colorOverLifetime; col.enabled = true; col.color = new ParticleSystem.MinMaxGradient(grad);

        var sizeMod = flameFX.sizeOverLifetime; sizeMod.enabled = true;
        sizeMod.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1.5f));

        var psr = go.GetComponent<ParticleSystemRenderer>();
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var mat = new Material(shader);
        mat.color = new Color(1f, 0.5f, 0.1f);
        psr.material = mat;
    }

    // ---------- BEŽNÉ ZBRANE ----------
    void Swing()
    {
        if (handDisplay != null) handDisplay.PlaySwing();
        cooldown = 1f / Mathf.Max(0.1f, swingSpeed);

        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, hitDistance, layerMask))
        {
            var b = hit.collider.GetComponent<Breakable>();
            HitBreakable(b, hit.point, ray.direction);

            if (splash > 0f)
            {
                foreach (var col in Physics.OverlapSphere(hit.point, splash, layerMask))
                {
                    if (col == hit.collider) continue;
                    HitBreakable(col.GetComponent<Breakable>(), hit.point, ray.direction);
                }
            }
        }
    }

    void HitBreakable(Breakable b, Vector3 point, Vector3 dir)
    {
        if (b == null) return;
        int orig = b.damage;
        b.damage = damage;
        b.Hit(point, dir);
        if (b != null) b.damage = orig;
    }
}
