using System.Collections;
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
    private FirstPersonHands fpHands;
    private PauseMenu   pauseMenu;
    private float cooldown = 0f;

    // Plamenomet
    private ParticleSystem flameFX;
    private bool  spraying = false;
    private float fireTick = 0f;

    void Start()
    {
        layerMask = ~LayerMask.GetMask("Player");
        handDisplay = FindFirstObjectByType<HandDisplay>();
        pauseMenu   = FindFirstObjectByType<PauseMenu>();
        if (playerCamera != null) fpHands = FirstPersonHands.Create(playerCamera);
        var hud = GameObject.Find("HandDisplayRoot"); if (hud != null) hud.SetActive(false);
        if (PlayerInventory.Instance != null)
            ApplyWeapon(PlayerInventory.Instance.GetEquipped());
        BuildFlameFX();
    }

    public void ApplyWeapon(WeaponData w)
    {
        int up = PlayerInventory.Instance != null ? PlayerInventory.Instance.UpgradeLevel(w.id) : 0;
        damage         = w.damage + up;                 // +1 damage za upgrade level
        splash         = w.splashRadius + up * 0.1f;    // +0.1 splash za level
        hitDistance    = w.hitDistance + Perks.ReachBonus();
        swingSpeed     = w.swingSpeed * Perks.SwingMult();
        isFlamethrower = w.id == "flamethrower";
        if (!isFlamethrower) StopSpray();
        if (fpHands != null) fpHands.SetWeapon(w);
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) { StopSpray(); return; }
        if (pauseMenu == null) pauseMenu = FindFirstObjectByType<PauseMenu>();
        if (pauseMenu != null && pauseMenu.IsPaused) { StopSpray(); return; }
        if (Mouse.current == null) return;

        if (isFlamethrower) { HandleFlamethrower(); return; }

        if (cooldown > 0f) { cooldown -= Time.deltaTime; return; }
        if (Mouse.current.leftButton.wasPressedThisFrame) Swing();
    }

    // ---------- PLAMENOMET ----------
    void HandleFlamethrower()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            if (!spraying) { spraying = true; if (flameFX != null) flameFX.Play(); SfxManager.FlameOn(); }
            fireTick -= Time.deltaTime;
            if (fireTick <= 0f) { fireTick = 0.08f; SprayDamage(); }
        }
        else StopSpray();
    }

    void StopSpray()
    {
        if (spraying) { spraying = false; if (flameFX != null) flameFX.Stop(); SfxManager.FlameOff(); }
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

    // ---------- BEZNE ZBRANE ----------
    void Swing()
    {
        if (handDisplay != null) handDisplay.PlaySwing();
        if (fpHands != null) fpHands.PlaySwing();
        SfxManager.Swing();
        cooldown = 1f / Mathf.Max(0.1f, swingSpeed);

        // Uder dopadne az v momente kontaktu animacie (realistickejsie)
        float impactDelay = Mathf.Clamp(0.14f / Mathf.Max(0.4f, swingSpeed), 0.05f, 0.28f);
        StartCoroutine(DoHitAfter(impactDelay));
    }

    IEnumerator DoHitAfter(float delay)
    {
        float t = 0f;
        while (t < delay) { t += Time.deltaTime; yield return null; }
        if (pauseMenu != null && pauseMenu.IsPaused) yield break;
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) yield break;
        PerformHit();
    }

    void PerformHit()
    {
        if (playerCamera == null) return;

        bool connected = false;
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, hitDistance, layerMask))
        {
            var b = hit.collider.GetComponent<Breakable>();
            if (b != null) connected = true;
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

        // Pocit z uderu: otras kamery + mikro hit-stop podla sily zbrane
        if (connected)
        {
            CameraShaker.Shake(0.08f, 0.025f + damage * 0.006f);
            Juice.HitStop(Mathf.Clamp(0.012f + damage * 0.004f, 0.012f, 0.05f));
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
