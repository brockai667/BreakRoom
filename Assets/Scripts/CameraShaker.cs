using UnityEngine;

/// Krátky otras kamery (pri výbuchu / silnom náraze). Pridá sa za behu na
/// hlavnú kameru. Volá sa staticky: CameraShaker.Shake(dĺžka, sila).
public class CameraShaker : MonoBehaviour
{
    Transform cam;
    Vector3 baseLocal;
    float dur, t, mag;

    public static void Shake(float duration, float magnitude)
    {
        var c = Camera.main;
        if (c == null) return;
        var sh = c.GetComponent<CameraShaker>();
        if (sh == null) sh = c.gameObject.AddComponent<CameraShaker>();
        sh.Trigger(duration, magnitude);
    }

    void Trigger(float d, float m)
    {
        cam = transform;
        if (t <= 0f) baseLocal = cam.localPosition;   // zachyť základ len keď nehýbeme
        dur = Mathf.Max(0.01f, d); t = dur; mag = m;
    }

    void LateUpdate()
    {
        if (t <= 0f) return;
        t -= Time.deltaTime;
        float k = Mathf.Clamp01(t / dur);
        if (t > 0f) cam.localPosition = baseLocal + Random.insideUnitSphere * mag * k;
        else        cam.localPosition = baseLocal;
    }
}
