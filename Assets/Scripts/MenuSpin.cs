using UnityEngine;

/// Pomaly otáča objekt okolo Y (zbraň na pódiu v Main Menu).
public class MenuSpin : MonoBehaviour
{
    public float speed = 28f;
    public float bob = 0.08f;
    float baseY;

    void Start() { baseY = transform.localPosition.y; }

    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f, Space.World);
        var p = transform.localPosition;
        p.y = baseY + Mathf.Sin(Time.time * 1.4f) * bob;
        transform.localPosition = p;
    }
}
