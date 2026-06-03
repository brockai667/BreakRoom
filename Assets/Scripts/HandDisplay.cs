using UnityEngine;
using UnityEngine.UI;

/// Zobrazuje zbraň v pravom dolnom rohu a hrá animáciu švihu pri kliknutí.
public class HandDisplay : MonoBehaviour
{
    [Header("UI refs (auto-vyplnené)")]
    public RectTransform handleRect;   // rukoväť (spodná časť)
    public RectTransform bladeRect;    // hlava zbrane (vrchná časť)
    public Image         handleImg;
    public Image         bladeImg;
    public Text          weaponNameText;

    private Vector3 restPos;           // kľudová pozícia
    private bool swinging = false;
    private float swingT  = 0f;
    private const float SWING_DUR = 0.18f;

    void Start()
    {
        restPos = handleRect.localPosition;

        if (PlayerInventory.Instance != null)
            SetWeapon(PlayerInventory.Instance.GetEquipped());
        else
            SetWeapon(WeaponData.All[0]);
    }

    public void SetWeapon(WeaponData w)
    {
        if (handleImg != null) handleImg.color = w.handleColor;
        if (bladeImg  != null) bladeImg.color  = w.handColor;
        if (weaponNameText != null) weaponNameText.text = w.displayName;

        // Prispôsob tvar zbrane podľa ID
        if (bladeRect != null)
        {
            switch (w.id)
            {
                case "fists":
                    bladeRect.sizeDelta = new Vector2(55, 55); break;
                case "bat":
                    bladeRect.sizeDelta = new Vector2(22, 120); break;
                case "gloves":
                    bladeRect.sizeDelta = new Vector2(65, 65); break;
                case "hammer":
                    bladeRect.sizeDelta = new Vector2(70, 55); break;
                case "axe":
                    bladeRect.sizeDelta = new Vector2(65, 90); break;
                case "sledge":
                    bladeRect.sizeDelta = new Vector2(85, 70); break;
                case "flamethrower":
                    bladeRect.sizeDelta = new Vector2(110, 45); break;
                default:
                    bladeRect.sizeDelta = new Vector2(40, 100); break;
            }
        }
    }

    public void PlaySwing()
    {
        if (swinging) return;
        swinging = true;
        swingT   = 0f;
    }

    void Update()
    {
        if (!swinging) return;

        swingT += Time.deltaTime / SWING_DUR;
        float t = swingT;

        // Animácia: rýchle švihnutie hore-doprava, potom späť
        float swing = Mathf.Sin(t * Mathf.PI);       // 0→1→0
        Vector3 swingOffset = new Vector3(40f * swing, 55f * swing, 0f);
        float   swingRotate = -35f * swing;

        handleRect.localPosition = restPos + swingOffset;
        handleRect.localEulerAngles = new Vector3(0, 0, swingRotate);

        if (t >= 1f)
        {
            swinging = false;
            handleRect.localPosition    = restPos;
            handleRect.localEulerAngles = Vector3.zero;
        }
    }
}
