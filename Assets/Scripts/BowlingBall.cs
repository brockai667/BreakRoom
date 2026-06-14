using UnityEngine;

/// Hodená bowlingová guľa: letí nízko dopredu, dopadne a kotúľa sa po zemi,
/// pričom ničí všetko v dráhe (zachováva si collider, takže reálne roluje).
public class BowlingBall : MonoBehaviour
{
    public int damage = 6;
    float life = 5f;
    Rigidbody rb;

    public static BowlingBall Throw(Vector3 pos, Vector3 dir, int dmg)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "BowlingBall";
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.5f;

        var rend = go.GetComponent<Renderer>();
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        Color c = new Color(0.10f, 0.10f, 0.22f);
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.7f);
        rend.material = m;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 6f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Vector3 flat = new Vector3(dir.x, 0f, dir.z);
        if (flat.sqrMagnitude < 0.001f) flat = Vector3.forward;
        flat.Normalize();
        rb.linearVelocity = flat * 13f + Vector3.down * 1.2f;
        rb.angularVelocity = Vector3.Cross(Vector3.up, flat) * 16f;   // dopredné kotúľanie

        var bb = go.AddComponent<BowlingBall>();
        bb.damage = dmg;
        bb.rb = rb;
        if (SfxManager.Instance != null) SfxManager.Swing();
        return bb;
    }

    void OnCollisionEnter(Collision col)
    {
        var b = col.collider.GetComponent<Breakable>();
        if (b == null) return;
        Vector3 dir = rb != null ? rb.linearVelocity.normalized : transform.forward;
        int orig = b.damage;
        b.damage = damage;
        b.Hit(col.GetContact(0).point, dir);
        if (b != null) b.damage = orig;
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }
}
