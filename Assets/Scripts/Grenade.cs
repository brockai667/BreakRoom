using System.Collections;
using UnityEngine;

/// Hodeny granat: leti, po fuse case vybuchne a plosne posiela rozbitne veci.
public class Grenade : MonoBehaviour
{
    public float fuse   = 1.4f;
    public float radius = 3.6f;
    public int   damage = 8;
    bool exploded;

    /// Vytvorí a odhodí granát z pos v smere dir (guľová fyzika + rotácia).
    public static Grenade Throw(Vector3 pos, Vector3 dir)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Grenade";
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.22f;

        var rend = go.GetComponent<Renderer>();
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        Color c = new Color(0.22f, 0.3f, 0.16f);
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        rend.material = m;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.4f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(dir.normalized * 9f + Vector3.up * 2.5f, ForceMode.VelocityChange);
        rb.AddTorque(Random.insideUnitSphere * 5f);

        return go.AddComponent<Grenade>();
    }

    void Start() { StartCoroutine(FuseTimer()); }

    IEnumerator FuseTimer()
    {
        yield return new WaitForSeconds(fuse);
        Boom();
    }

    void Boom()
    {
        if (exploded) return;
        exploded = true;
        Vector3 p = transform.position;

        Fx.Explosion(p);
        CameraShaker.Shake(0.35f, 0.42f);
        if (SfxManager.Instance != null) SfxManager.Boom(p);

        foreach (var col in Physics.OverlapSphere(p, radius))
        {
            if (col == null) continue;
            var rb = col.attachedRigidbody;
            if (rb != null && !rb.isKinematic) rb.AddExplosionForce(520f, p, radius, 1.6f);
            var b = col.GetComponent<Breakable>();
            if (b == null) continue;
            int orig = b.damage;
            b.damage = damage;
            b.Hit(b.transform.position, (b.transform.position - p).normalized);
            if (b != null) b.damage = orig;
        }

        Destroy(gameObject);
    }
}
