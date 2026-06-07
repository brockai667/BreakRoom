using UnityEngine;

/// 3D náhľad zbrane na pódiu v hube. Postaví jednoduchý model z primitív
/// podľa WeaponData a pomaly ho otáča.
public class WeaponPreview : MonoBehaviour
{
    public static WeaponPreview Instance;

    private GameObject current;
    public float spinSpeed = 35f;

    void Awake() { Instance = this; }

    void Update()
    {
        if (current != null)
            current.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void Show(WeaponData w)
    {
        if (w == null) return;
        if (current != null) Destroy(current);
        current = Build(w);
        current.transform.SetParent(transform, false);
        current.transform.localPosition = Vector3.zero;
        current.SetActive(true);
    }

    public void SetVisible(bool v)
    {
        if (current != null) current.SetActive(v);
    }

    // ---------- STAVBA MODELU ----------
    static GameObject Build(WeaponData w)
    {
        var root = new GameObject("WeaponModel_" + w.id);
        var handleMat = Mat(w.handleColor);
        var headMat   = Mat(w.handColor);

        switch (w.id)
        {
            case "fists":
                Add(root, PrimitiveType.Cube, new Vector3(-0.22f, 0, 0), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, headMat);
                Add(root, PrimitiveType.Cube, new Vector3( 0.22f, 0, 0), new Vector3(0.34f, 0.34f, 0.34f), Quaternion.identity, headMat);
                break;
            case "bat":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.35f, 0), new Vector3(0.10f, 0.30f, 0.10f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cylinder, new Vector3(0,  0.25f, 0), new Vector3(0.17f, 0.40f, 0.17f), Quaternion.identity, headMat);
                break;
            case "gloves":
                Add(root, PrimitiveType.Cube, new Vector3(-0.20f, 0, 0), new Vector3(0.32f, 0.32f, 0.34f), Quaternion.identity, headMat);
                Add(root, PrimitiveType.Cube, new Vector3( 0.20f, 0, 0), new Vector3(0.32f, 0.32f, 0.34f), Quaternion.identity, headMat);
                break;
            case "hammer":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.15f, 0), new Vector3(0.07f, 0.45f, 0.07f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0,  0.45f, 0), new Vector3(0.50f, 0.24f, 0.24f), Quaternion.identity, headMat);
                break;
            case "axe":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.10f, 0), new Vector3(0.07f, 0.50f, 0.07f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0.22f, 0.45f, 0), new Vector3(0.40f, 0.42f, 0.06f), Quaternion.identity, headMat);
                break;
            case "sledge":
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.15f, 0), new Vector3(0.08f, 0.48f, 0.08f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0,  0.50f, 0), new Vector3(0.58f, 0.32f, 0.32f), Quaternion.identity, headMat);
                break;
            case "flamethrower":
                Add(root, PrimitiveType.Cube,     new Vector3(0, 0, -0.05f), new Vector3(0.34f, 0.32f, 0.62f), Quaternion.identity, headMat);
                Add(root, PrimitiveType.Cylinder, new Vector3(0, 0.04f, 0.55f), new Vector3(0.07f, 0.22f, 0.07f), Quaternion.Euler(90, 0, 0), handleMat);
                break;
            default:
                Add(root, PrimitiveType.Cylinder, new Vector3(0, -0.10f, 0), new Vector3(0.07f, 0.45f, 0.07f), Quaternion.identity, handleMat);
                Add(root, PrimitiveType.Cube,     new Vector3(0,  0.45f, 0), new Vector3(0.42f, 0.26f, 0.26f), Quaternion.identity, headMat);
                break;
        }
        return root;
    }

    static void Add(GameObject root, PrimitiveType t, Vector3 pos, Vector3 scale, Quaternion rot, Material m)
    {
        var g = GameObject.CreatePrimitive(t);
        var col = g.GetComponent<Collider>(); if (col != null) Destroy(col);
        g.transform.SetParent(root.transform, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.transform.localRotation = rot;
        g.GetComponent<Renderer>().sharedMaterial = m;
    }

    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader")
            m = new Material(Shader.Find("Standard"));
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
}
