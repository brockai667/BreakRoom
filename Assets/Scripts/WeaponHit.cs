using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHit : MonoBehaviour
{
    public float hitDistance = 3.5f;
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
                new Vector3(Screen.width / 2f, Screen.height / 2f, 0));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, hitDistance, layerMask))
            {
                Breakable breakable = hit.collider.GetComponent<Breakable>();
                if (breakable != null)
                    breakable.Hit(hit.point, ray.direction);
            }
        }
    }
}
