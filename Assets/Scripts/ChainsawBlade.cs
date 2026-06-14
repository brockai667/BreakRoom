using UnityEngine;

/// Animuje "reťaz" motorovej píly: posúva zuby po hornej a spodnej hrane lišty
/// v opačných smeroch (slučka). Pri držaní útoku sa reťaz točí rýchlejšie.
public class ChainsawBlade : MonoBehaviour
{
    public Transform[] top;
    public Transform[] bottom;
    public float zMin;
    public float zMax;
    public float baseSpeed = 1.4f;

    float phase;

    void Update()
    {
        float span = zMax - zMin;
        if (span <= 0.001f) return;

        float sp = baseSpeed;
        var m = UnityEngine.InputSystem.Mouse.current;
        if (m != null && m.leftButton.isPressed) sp *= 3.2f;

        phase = Mathf.Repeat(phase + sp * Time.unscaledDeltaTime, span);
        Place(top, span, +1f);
        Place(bottom, span, -1f);
    }

    void Place(Transform[] arr, float span, float dir)
    {
        if (arr == null || arr.Length == 0) return;
        float spacing = span / arr.Length;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == null) continue;
            float z = Mathf.Repeat(i * spacing + dir * phase, span);
            var p = arr[i].localPosition;
            p.z = zMin + z;
            arr[i].localPosition = p;
        }
    }
}
