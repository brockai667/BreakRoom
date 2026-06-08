using UnityEngine;

public class Breakable : MonoBehaviour
{
    public int hp           = 3;
    public int damage       = 1;
    public int xpValue      = 10;
    public int fragmentCount= 7;

    public void Hit(Vector3 hitPoint, Vector3 swingDir)
    {
        hp -= damage;
        StartCoroutine(ShakeOnHit());
        if (hp <= 0) Break(hitPoint, swingDir);
    }

    public void Hit() { Hit(transform.position, Vector3.forward); }

    void Break(Vector3 hitPoint, Vector3 swingDir)
    {
        // XP
        if (XPManager.Instance != null) XPManager.Instance.AddXP(xpValue);
        // Peniaze za kolo - nazbierajú sa a pripočítajú animovane v hube na konci kola
        if (GameManager.Instance != null) GameManager.Instance.AddRoundMoney(xpValue / 2);
        else if (PlayerInventory.Instance != null) PlayerInventory.Instance.AddMoney(xpValue / 2);
        // Skóre
        if (GameManager.Instance != null) GameManager.Instance.RegisterDestroy();

        SpawnFragments(hitPoint, swingDir);
        Destroy(gameObject);
    }

    void SpawnFragments(Vector3 hitPoint, Vector3 swingDir)
    {
        // Funguje aj pre prefaby kde renderer/collider je v deťoch
        var rend = GetComponent<Renderer>();
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        Color col = rend != null ? rend.material.color : new Color(0.6f, 0.6f, 0.62f);

        var coll = GetComponent<Collider>();
        Vector3 size = coll != null ? coll.bounds.size
                     : rend != null ? rend.bounds.size
                     : transform.lossyScale;
        size = new Vector3(Mathf.Max(0.1f, size.x), Mathf.Max(0.1f, size.y), Mathf.Max(0.1f, size.z));
        float maxDim = Mathf.Max(size.x, size.y, size.z);

        for (int i = 0; i < fragmentCount; i++)
        {
            float s = UnityEngine.Random.Range(0.20f, 0.55f);
            Vector3 fragScale = new Vector3(
                Mathf.Max(0.04f, size.x * s * UnityEngine.Random.Range(0.5f, 1.2f)),
                Mathf.Max(0.04f, size.y * s * UnityEngine.Random.Range(0.5f, 1.2f)),
                Mathf.Max(0.04f, size.z * s * UnityEngine.Random.Range(0.5f, 1.2f)));

            Vector3 scatter = new Vector3(
                UnityEngine.Random.Range(-1f,1f),
                UnityEngine.Random.Range(-1f,1f),
                UnityEngine.Random.Range(-1f,1f)).normalized * maxDim * 0.35f;

            var frag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frag.transform.position    = transform.position + scatter;
            frag.transform.localScale  = fragScale;
            frag.transform.rotation    = UnityEngine.Random.rotation;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader.name == "Hidden/InternalErrorShader") mat = new Material(Shader.Find("Standard"));
            float d = UnityEngine.Random.Range(0.72f, 1.05f);
            mat.color = new Color(Mathf.Clamp01(col.r*d), Mathf.Clamp01(col.g*d), Mathf.Clamp01(col.b*d));
            frag.GetComponent<Renderer>().material = mat;

            var rb = frag.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Vector3 outward  = (frag.transform.position - hitPoint).normalized;
            Vector3 forceDir = (outward * 1.5f + swingDir * 2.5f + Vector3.up * UnityEngine.Random.Range(0.4f, 1.4f)).normalized;
            rb.AddForce(forceDir * UnityEngine.Random.Range(380f, 720f));
            rb.AddTorque(new Vector3(
                UnityEngine.Random.Range(-400f,400f),
                UnityEngine.Random.Range(-400f,400f),
                UnityEngine.Random.Range(-400f,400f)));

            Destroy(frag, 7f);
        }
    }

    System.Collections.IEnumerator ShakeOnHit()
    {
        Vector3 origin = transform.localPosition;
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            transform.localPosition = origin + new Vector3(
                UnityEngine.Random.Range(-0.03f,0.03f),
                UnityEngine.Random.Range(-0.03f,0.03f),
                UnityEngine.Random.Range(-0.03f,0.03f));
            yield return null;
        }
        if (this != null) transform.localPosition = origin;
    }
}
