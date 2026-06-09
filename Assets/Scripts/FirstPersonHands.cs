using UnityEngine;

/// 3D ruka v prvej osobe, ktorá drží vybavenú zbraň a hrá animáciu úderu.
/// Vytvára sa za behu pod kamerou hráča (netreba upravovať scény).
public class FirstPersonHands : MonoBehaviour
{
    Transform pivot;        // animovaný koreň ruky
    GameObject weaponGO;    // držaná zbraň

    bool swinging; float swingT; const float DUR = 0.25f;
    bool flame;             // plameňomet - držanie

    static readonly Vector3 RestPos   = new Vector3(0.4f, -0.4f, 0.82f);
    static readonly Vector3 RestEuler = new Vector3(6f, -18f, 6f);

    public static FirstPersonHands Create(Camera cam)
    {
        var root = new GameObject("FirstPersonHands");
        root.transform.SetParent(cam.transform, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        var fp = root.AddComponent<FirstPersonHands>();
        fp.Build();
        return fp;
    }

    void Build()
    {
        var pv = new GameObject("HandPivot");
        pivot = pv.transform;
        pivot.SetParent(transform, false);
        pivot.localPosition = RestPos;
        pivot.localEulerAngles = RestEuler;

        // predlaktie
        AddBox("Forearm", new Vector3(0.02f, -0.12f, -0.26f), new Vector3(0.085f, 0.085f, 0.4f));
        // ruka (päsť)
        AddBox("Hand",    new Vector3(0f, -0.03f, 0.02f),  new Vector3(0.11f, 0.12f, 0.13f));

        if (PlayerInventory.Instance != null) SetWeapon(PlayerInventory.Instance.GetEquipped());
    }

    void AddBox(string name, Vector3 pos, Vector3 scale)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var col = g.GetComponent<Collider>(); if (col != null) Destroy(col);
        g.name = name; g.transform.SetParent(pivot, false);
        g.transform.localPosition = pos; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = SkinMat();
    }

    public void SetWeapon(WeaponData w)
    {
        if (weaponGO != null) Destroy(weaponGO);
        flame = w != null && w.id == "flamethrower";
        if (w == null || w.id == "fists") return;   // päsť = samotná ruka, bez modelu
        weaponGO = WeaponPreview.BuildModel(w);
        weaponGO.transform.SetParent(pivot, false);
        weaponGO.transform.localPosition = new Vector3(0f, 0.03f, 0.14f);
        weaponGO.transform.localScale = Vector3.one * 0.38f;
        weaponGO.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
    }

    public void PlaySwing()
    {
        if (!swinging) { swinging = true; swingT = 0f; }
    }

    void Update()
    {
        if (pivot == null) return;

        if (swinging)
        {
            swingT += Time.deltaTime / DUR;
            float t = Mathf.Clamp01(swingT);
            float s = Mathf.Sin(t * Mathf.PI);  // 0->1->0
            pivot.localPosition    = RestPos   + new Vector3(-0.06f * s, -0.20f * s, 0.16f * s);
            pivot.localEulerAngles = RestEuler + new Vector3(60f * s, 0f, -22f * s);
            if (t >= 1f) { swinging = false; pivot.localPosition = RestPos; pivot.localEulerAngles = RestEuler; }
        }
        else
        {
            // jemné dýchanie / pri plameňomete mierne chvenie
            float bobAmp = flame && UnityEngine.InputSystem.Mouse.current != null
                           && UnityEngine.InputSystem.Mouse.current.leftButton.isPressed ? 0.012f : 0.004f;
            float bob = Mathf.Sin(Time.time * (flame ? 22f : 2f)) * bobAmp;
            pivot.localPosition = RestPos + new Vector3(0f, bob, 0f);
            pivot.localEulerAngles = RestEuler;
        }
    }

    static Material SkinMat()
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        var c = new Color(0.86f, 0.67f, 0.52f);
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
}
