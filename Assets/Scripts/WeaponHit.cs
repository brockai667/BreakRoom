using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHit : MonoBehaviour
{
    public Camera playerCamera;
    public float hitDistance = 4f;

    private int   damage     = 1;
    private float splash     = 0f;
    private float swingSpeed = 1f;
    private int   layerMask;
    private HandDisplay handDisplay;
    private PauseMenu   pauseMenu;
    private float cooldown = 0f;

    void Start()
    {
        layerMask = ~LayerMask.GetMask("Player");
        if (PlayerInventory.Instance != null)
            ApplyWeapon(PlayerInventory.Instance.GetEquipped());
        handDisplay = FindObjectOfType<HandDisplay>();
        pauseMenu   = FindObjectOfType<PauseMenu>();
    }

    public void ApplyWeapon(WeaponData w)
    {
        damage      = w.damage;
        splash      = w.splashRadius;
        hitDistance = w.hitDistance;
        swingSpeed  = w.swingSpeed;
    }

    void Update()
    {
        // Neswinguj ak kolo skončilo alebo hra je pauzovaná
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) return;
        if (pauseMenu == null) pauseMenu = FindObjectOfType<PauseMenu>();
        if (pauseMenu != null && pauseMenu.IsPaused) return;

        if (cooldown > 0f) { cooldown -= Time.deltaTime; return; }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Swing();
    }

    void Swing()
    {
        if (handDisplay != null) handDisplay.PlaySwing();

        // Cooldown: 1/swingSpeed sekúnd medzi údermi
        cooldown = 1f / Mathf.Max(0.1f, swingSpeed);

        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hitDistance, layerMask))
        {
            var b = hit.collider.GetComponent<Breakable>();
            HitBreakable(b, hit.point, ray.direction);

            // Splash
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
        // Ak b bol zničený, netreba obnovovať damage (objekt je gone)
        if (b != null) b.damage = orig;
    }
}
