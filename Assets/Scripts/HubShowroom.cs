using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Profesionálnejšie lobby: okolo pódia so zbraňou postaví "showroom" —
/// stupienok, bodový reflektor zhora, stage-backdrop (orientovaný podľa kamery),
/// akcentový prstenec a teplé ambient svetlá. Self-bootstrapping, len v Hube.
public class HubShowroom : MonoBehaviour
{
    static readonly Color ACCENT = new Color(1f, 0.46f, 0.14f);
    GameObject root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("HubShowroom");
        DontDestroyOnLoad(go);
        var hs = go.AddComponent<HubShowroom>();
        SceneManager.sceneLoaded += (s, m) => hs.OnScene(s.name);
        hs.OnScene(SceneManager.GetActiveScene().name);
    }

    void OnScene(string scene)
    {
        if (root != null) { Destroy(root); root = null; }
        if (scene != "Hub") return;
        StartCoroutine(BuildNextFrame());
    }

    IEnumerator BuildNextFrame()
    {
        yield return null;   // počkaj na objekty v scéne
        Build();
    }

    void Build()
    {
        root = new GameObject("ShowroomDecor");

        // pozícia pódia (kde sa točí zbraň) + orientácia podľa kamery
        var wp = WeaponPreview.Instance != null ? WeaponPreview.Instance : FindFirstObjectByType<WeaponPreview>();
        Vector3 podium = wp != null ? wp.transform.position : new Vector3(0f, 1.2f, 0f);
        var cam = Camera.main;
        Vector3 camPos = cam != null ? cam.transform.position : podium + new Vector3(0, 1.5f, -6f);

        Vector3 fwd = podium - camPos; fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd);

        float floorY = 0.02f;
        Vector3 baseC = new Vector3(podium.x, floorY, podium.z);

        // ── STUPIENOK pod zbraňou ──
        Cyl("PodiumBase", baseC + Vector3.up * 0.09f, new Vector3(2.3f, 0.09f, 2.3f), Mat(new Color(0.10f, 0.11f, 0.14f)));
        Cyl("PodiumStep", baseC + Vector3.up * 0.20f, new Vector3(1.8f, 0.06f, 1.8f), Mat(new Color(0.16f, 0.17f, 0.21f)));
        // akcentový prstenec (žiariaci)
        Cyl("PodiumRing", baseC + Vector3.up * 0.265f, new Vector3(1.95f, 0.02f, 1.95f), Emissive(ACCENT));

        // ── BODOVÝ REFLEKTOR zhora ──
        var spotGO = new GameObject("PodiumSpot"); spotGO.transform.SetParent(root.transform, false);
        spotGO.transform.position = podium + Vector3.up * 3.4f - fwd * 0.3f;
        spotGO.transform.rotation = Quaternion.LookRotation((podium - spotGO.transform.position).normalized, Vector3.up);
        var spot = spotGO.AddComponent<Light>();
        spot.type = LightType.Spot; spot.spotAngle = 55f; spot.range = 9f;
        spot.color = new Color(1f, 0.96f, 0.85f); spot.intensity = 6.5f;

        // ── STAGE BACKDROP za pódiom (orientovaný k hráčovi) ──
        Vector3 backC = podium + fwd * 4.2f; backC.y = 0f;
        Quaternion face = Quaternion.LookRotation(-fwd, Vector3.up);
        Box("Backdrop", backC + Vector3.up * 2.6f, new Vector3(9f, 5.2f, 0.25f), face, Mat(new Color(0.13f, 0.14f, 0.18f)));
        // akcentové pruhy
        Box("StripeA", backC + Vector3.up * 4.6f + fwd * -0.14f, new Vector3(9f, 0.18f, 0.05f), face, Emissive(ACCENT));
        Box("StripeB", backC + Vector3.up * 0.7f + fwd * -0.14f, new Vector3(9f, 0.12f, 0.05f), face, Emissive(new Color(0.9f, 0.4f, 0.1f)));
        // bočné piliere
        Box("PillarL", backC + right *  4.3f + Vector3.up * 2.6f, new Vector3(0.4f, 5.2f, 0.4f), face, Mat(new Color(0.09f, 0.10f, 0.13f)));
        Box("PillarR", backC + right * -4.3f + Vector3.up * 2.6f, new Vector3(0.4f, 5.2f, 0.4f), face, Mat(new Color(0.09f, 0.10f, 0.13f)));

        // ── TEPLÉ AMBIENT SVETLÁ ──
        Point("WarmL", podium + right *  3.5f + Vector3.up * 2.4f, new Color(1f, 0.7f, 0.4f), 2.2f, 8f);
        Point("WarmR", podium + right * -3.5f + Vector3.up * 2.4f, new Color(1f, 0.7f, 0.4f), 2.2f, 8f);
        Point("BackGlow", backC + Vector3.up * 2.5f - fwd * 0.6f, ACCENT, 1.6f, 7f);

        // ── STAGE PODLAHA (mat pod showroomom) ──
        Vector3 matC = (baseC + backC) * 0.5f; matC.y = 0.04f;
        Box("StageMat",    matC,                       new Vector3(7.6f, 0.05f, 7.0f), Quaternion.identity, Mat(new Color(0.14f, 0.15f, 0.18f)));
        Box("StageMatRim", matC + Vector3.up * 0.006f, new Vector3(7.0f, 0.06f, 6.4f), Quaternion.identity, Emissive(new Color(0.45f, 0.2f, 0.05f)));

        // ── STĹPIKY + LANO okolo pódia ──
        float pr = 1.8f;
        Vector3[] posts = {
            baseC + right * pr + fwd * pr,
            baseC - right * pr + fwd * pr,
            baseC - right * pr - fwd * pr,
            baseC + right * pr - fwd * pr,
        };
        foreach (var pp in posts)
        {
            Cyl("Post", pp + Vector3.up * 0.42f, new Vector3(0.07f, 0.42f, 0.07f), Mat(new Color(0.20f, 0.21f, 0.25f)));
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var bc = ball.GetComponent<Collider>(); if (bc != null) Destroy(bc);
            ball.name = "PostTop"; ball.transform.SetParent(root.transform, false);
            ball.transform.position = pp + Vector3.up * 0.88f; ball.transform.localScale = Vector3.one * 0.14f;
            ball.GetComponent<Renderer>().sharedMaterial = Emissive(ACCENT);
        }
        for (int i = 0; i < posts.Length; i++)
            Rope(posts[i] + Vector3.up * 0.72f, posts[(i + 1) % posts.Length] + Vector3.up * 0.72f);

        // ── STENA SO ZBRAŇAMI (display rack na backdrope) ──
        var all = WeaponData.All;
        int shown = Mathf.Min(all.Length, 7);
        for (int i = 0; i < shown; i++)
        {
            float k = shown > 1 ? (i / (float)(shown - 1) - 0.5f) : 0f;
            Vector3 wpos = backC + right * (k * 6.8f) + Vector3.up * 2.6f - fwd * 0.34f;
            Box("Peg", wpos - Vector3.up * 0.5f, new Vector3(0.5f, 0.06f, 0.18f), face, Mat(new Color(0.10f, 0.11f, 0.14f)));
            var wm = WeaponPreview.BuildModel(all[i]);
            wm.name = "WallWeapon_" + all[i].id;
            wm.transform.SetParent(root.transform, false);
            wm.transform.position = wpos;
            wm.transform.localScale = Vector3.one * 0.52f;
            wm.transform.rotation = face;
        }

        // ── TRACK SVETLÁ (galériové osvetlenie zhora) ──
        for (int i = -1; i <= 1; i += 2)
        {
            var tg = new GameObject("TrackSpot"); tg.transform.SetParent(root.transform, false);
            tg.transform.position = backC + right * (i * 2.6f) + Vector3.up * 4.7f - fwd * 0.4f;
            tg.transform.rotation = Quaternion.LookRotation((podium - tg.transform.position).normalized, Vector3.up);
            var l = tg.AddComponent<Light>(); l.type = LightType.Spot; l.spotAngle = 42f; l.range = 8f;
            l.color = new Color(0.85f, 0.9f, 1f); l.intensity = 2.6f;
        }
    }

    void Rope(Vector3 a, Vector3 b)
    {
        Vector3 mid = (a + b) * 0.5f;
        float len = Vector3.Distance(a, b);
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var c = g.GetComponent<Collider>(); if (c != null) Destroy(c);
        g.name = "Rope"; g.transform.SetParent(root.transform, false);
        g.transform.position = mid;
        g.transform.rotation = Quaternion.LookRotation((b - a).normalized, Vector3.up);
        g.transform.localScale = new Vector3(0.03f, 0.03f, len);
        g.GetComponent<Renderer>().sharedMaterial = Emissive(new Color(0.85f, 0.55f, 0.15f));
    }

    // ── HELPERY ──
    GameObject Box(string name, Vector3 pos, Vector3 scale, Quaternion rot, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var c = g.GetComponent<Collider>(); if (c != null) Destroy(c);
        g.name = name; g.transform.SetParent(root.transform, false);
        g.transform.position = pos; g.transform.localScale = scale; g.transform.rotation = rot;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    GameObject Cyl(string name, Vector3 pos, Vector3 scale, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var c = g.GetComponent<Collider>(); if (c != null) Destroy(c);
        g.name = name; g.transform.SetParent(root.transform, false);
        g.transform.position = pos; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    void Point(string name, Vector3 pos, Color col, float intensity, float range)
    {
        var go = new GameObject(name); go.transform.SetParent(root.transform, false);
        go.transform.position = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point; l.color = col; l.intensity = intensity; l.range = range;
    }

    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }

    static Material Emissive(Color c)
    {
        var m = Mat(c);
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 2.2f); }
        return m;
    }
}
