using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHit : MonoBehaviour
{
    public float hitDistance = 3f;
    public Camera playerCamera;
    private int layerMask;

    void Start()
    {
        layerMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = playerCamera.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, hitDistance, layerMask))
            {
                // Skontroluj samotny objekt aj jeho parent
                Breakable breakable = hit.collider.GetComponent<Breakable>()
                                   ?? hit.collider.GetComponentInParent<Breakable>();
                if (breakable != null)
                {
                    breakable.Hit();
                    Debug.Log("Zasiahnuty objekt: " + hit.collider.name);
                }
            }
        }
    }
}
