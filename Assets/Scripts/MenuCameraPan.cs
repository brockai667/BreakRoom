using UnityEngine;

/// Pomalý kinematický pohyb kamery v Main Menu — jemne obieha okolo pódia
/// (sem-tam) a mierne dýcha, aby scéna žila.
public class MenuCameraPan : MonoBehaviour
{
    public Vector3 focus = new Vector3(0f, 1.1f, 0f);
    public float radius = 6.2f;
    public float height = 2.6f;
    public float baseAngleDeg = 180f;   // 180 = kamera pred pódiom (-z), pozerá +z
    public float sweepDeg = 14f;
    public float sweepSpeed = 0.12f;
    public float lookHeight = 1.3f;

    void LateUpdate()
    {
        float a = (baseAngleDeg + Mathf.Sin(Time.time * sweepSpeed * Mathf.PI * 2f) * sweepDeg) * Mathf.Deg2Rad;
        float h = height + Mathf.Sin(Time.time * 0.18f) * 0.25f;
        transform.position = focus + new Vector3(Mathf.Sin(a) * radius, h, Mathf.Cos(a) * radius);
        transform.LookAt(focus + Vector3.up * (lookHeight - focus.y));
    }
}
