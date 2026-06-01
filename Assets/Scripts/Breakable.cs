using UnityEngine;

public class Breakable : MonoBehaviour
{
    public int hp = 3;
    public int damage = 1;
    public int xpValue = 10;
    public int fragmentCount = 7;

    public void Hit(Vector3 hitPoint, Vector3 swingDir)
    {
        hp -= damage;
        StartCoroutine(ShakeOnHit());
        if (hp <= 0) Break(hitPoint, swingDir);
    }

    // Legacy overload (no hit info)
    public void Hit()
    {
        Hit(transform.position, Vector3.forward);
    }

    void Break(Vector3 hitPoint, Vector3 swingDir)
    {
        if (XPManager.Instance != null)
            XPManager.Instance.AddXP(xpValue);

        Color col = GetComponent<Renderer>().material.color;
        Vector3 size = transform.lossyScale;
        float maxDim = Mathf.Max(size.x, size.y, size.z);

        for (int i = 0; i < fragmentCount; i++)
        {
            float s = UnityEngine.Random.Range(0.20f, 0.55f);
            Vector3 fragScale = new Vector3(
                Mathf.Max(0.04f, size.x * s * UnityEngine.Random.Range(0.5f, 1.2f)),
                Mathf.Max(0.04f, size.y * s * UnityEngine.Random.Range(0.5f, 1.2f)),
                Mathf.Max(0.04f, size.z * s * UnityEngine.Random.Range(0.5f, 1.2f))
            );

            // Random scatter within object bounds
            Vector3 scatter = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized * maxDim * 0.35f;

            GameObject frag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frag.transform.position = transform.position + scatter;
            frag.transform.localScale = fragScale;
            frag.transform.rotation = UnityEngine.Random.rotation;

            // Slightly varied colour
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader.name == "Hidden/InternalErrorShader")
                mat = new Material(Shader.Find("Standard"));
            float dark = UnityEngine.Random.Range(0.75f, 1.05f);
            mat.color = new Color(
                Mathf.Clamp01(col.r * dark),
                Mathf.Clamp01(col.g * dark),
                Mathf.Clamp01(col.b * dark));
            frag.GetComponent<Renderer>().material = mat;

            Rigidbody rb = frag.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Baseball-bat force: outward + swing direction + up arc
            Vector3 outward = (frag.transform.position - hitPoint).normalized;
            Vector3 forceDir = (outward * 1.5f + swingDir * 2.5f + Vector3.up * UnityEngine.Random.Range(0.4f, 1.4f)).normalized;
            float power = UnityEngine.Random.Range(350f, 700f);
            rb.AddForce(forceDir * power);

            // Random tumble
            Vector3 torque = new Vector3(
                UnityEngine.Random.Range(-400f, 400f),
                UnityEngine.Random.Range(-400f, 400f),
                UnityEngine.Random.Range(-400f, 400f));
            rb.AddTorque(torque);

            Destroy(frag, 7f);
        }

        Destroy(gameObject);
    }

    System.Collections.IEnumerator ShakeOnHit()
    {
        Vector3 origin = transform.localPosition;
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            transform.localPosition = origin + new Vector3(
                UnityEngine.Random.Range(-0.03f, 0.03f),
                UnityEngine.Random.Range(-0.03f, 0.03f),
                UnityEngine.Random.Range(-0.03f, 0.03f));
            yield return null;
        }
        if (this != null) transform.localPosition = origin;
    }
}
