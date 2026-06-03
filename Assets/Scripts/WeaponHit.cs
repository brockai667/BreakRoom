using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHit : MonoBehaviour
{
    public Camera playerCamera;
    public float hitDistance = 4f;

    private int damage      = 1;
    private float splash    = 0f;
    private float swingSpeed= 1f;
    private int layerMask;

    private HandDisplay handDisplay;
    private float cooldown = 0f;

    void Start()
    {
        layerMask = ~LayerMask.GetMask("Player");

        // Načítaj vybranú zbraň
        if (PlayerInventory.Instance != null)
            ApplyWeapon(PlayerInventory.Instance.GetEquipped());

        handDisplay = FindObjectOfType<HandDisplay>();
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
        if (cooldown > 0f) { cooldown -= Time.deltaTime; return; }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Swing();
        }
    }

    void Swing()
    {
        // Animácia v ruke
        if (handDisplay != null) handDisplay.PlaySwing();

        cooldown = 1f / Mathf.Max(0.1f, swingSpeed);

        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, hitDistance, layerMask))
        {
            HitBreakable(hit.collider.GetComponent<Breakable>(), hit.point, ray.direction);

            // Splash poškodenie
            if (splash > 0f)
            {
                Collider[] cols = Physics.OverlapSphere(hit.point, splash, layerMask);
                foreach (var col in cols)
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
        // Override damage amount
        int origDmg = b.damage;
        b.damage = damage;
        b.Hit(point, dir);
        b.damage = origDmg;
    }
}
