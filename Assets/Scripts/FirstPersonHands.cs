using UnityEngine;

/// 3D ruka v prvej osobe, ktora drzi vybavenu zbran a hra animaciu uderu.
/// Swing je podla typu zbrane (kladivo zvrchu, palka bokom, sekera diagonalne,
/// past/rukavice rovny jab). Vytvara sa za behu pod kamerou hraca.
public class FirstPersonHands : MonoBehaviour
{
    enum Style { Jab, Horizontal, Overhead, Diagonal, None }

    Transform pivot;
    GameObject weaponGO;

    bool swinging; float swingT; float dur = 0.25f;
    Style style = Style.Jab;
    bool flame;

    static readonly Vector3 RestPos   = new Vector3(0.4f, -0.4f, 0.82f);
    static readonly Vector3 RestEuler = new Vector3(6f, -18f, 6f);

    /// Vytvorí ruky ako dieťa danej kamery a rovno ich postaví (Build).
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

        AddBox("Forearm", new Vector3(0.02f, -0.12f, -0.26f), new Vector3(0.085f, 0.085f, 0.4f));
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

    /// Vymení model v ruke a nastaví štýl/rýchlosť švihu podľa zbrane (null/fists = holé ruky).
    public void SetWeapon(WeaponData w)
    {
        if (weaponGO != null) Destroy(weaponGO);
        flame = w != null && (w.id == "flamethrower" || w.id == "chainsaw");

        // Styl svihu + dlzka podla rychlosti
        string id = w != null ? w.id : "fists";
        style = id == "hammer" || id == "sledge" ? Style.Overhead
              : id == "bat" || id == "katana"     ? Style.Horizontal
              : id == "axe" || id == "crowbar"    ? Style.Diagonal
              : id == "bowling"                   ? Style.Overhead
              : id == "flamethrower"              ? Style.None
              :                                     Style.Jab;
        float sp = w != null ? w.swingSpeed : 1f;
        dur = Mathf.Clamp(0.30f / Mathf.Max(0.4f, sp), 0.14f, 0.5f);

        if (w == null || w.id == "fists") return;   // past = samotna ruka, bez modelu
        weaponGO = WeaponPreview.BuildModel(w);
        weaponGO.transform.SetParent(pivot, false);
        weaponGO.transform.localPosition = new Vector3(0f, 0.03f, 0.14f);
        weaponGO.transform.localScale = Vector3.one * 0.38f;
        weaponGO.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
    }

    /// Spustí animáciu úderu (ak už nebeží).
    public void PlaySwing()
    {
        if (!swinging) { swinging = true; swingT = 0f; }
    }

    void Update()
    {
        if (pivot == null) return;

        if (swinging)
        {
            swingT += Time.deltaTime / Mathf.Max(0.05f, dur);
            float t = Mathf.Clamp01(swingT);
            float s = Mathf.Sin(t * Mathf.PI);   // 0->1->0

            Vector3 dp, de;
            switch (style)
            {
                case Style.Overhead:    // kladivo/sledge: zvrchu nadol
                    dp = new Vector3(0f, -0.04f * s, 0.10f * s);
                    de = new Vector3(98f * s, 0f, 0f);
                    break;
                case Style.Horizontal:  // palka: svih bokom
                    dp = new Vector3(-0.20f * s, -0.02f * s, 0.06f * s);
                    de = new Vector3(8f * s, -75f * s, -12f * s);
                    break;
                case Style.Diagonal:    // sekera: diagonalny sek
                    dp = new Vector3(-0.10f * s, -0.05f * s, 0.10f * s);
                    de = new Vector3(72f * s, -28f * s, -38f * s);
                    break;
                default:                // jab: rovny uder dopredu
                    dp = new Vector3(0f, -0.02f * s, 0.24f * s);
                    de = new Vector3(20f * s, 0f, 0f);
                    break;
            }
            pivot.localPosition    = RestPos   + dp;
            pivot.localEulerAngles = RestEuler + de;
            if (t >= 1f) { swinging = false; pivot.localPosition = RestPos; pivot.localEulerAngles = RestEuler; }
        }
        else
        {
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
