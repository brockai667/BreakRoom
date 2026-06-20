using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
        // jemný akcentový prstenec (nie žiariaci tanier)
        Cyl("PodiumRing", baseC + Vector3.up * 0.255f, new Vector3(1.86f, 0.015f, 1.86f), Emissive(new Color(0.7f, 0.32f, 0.08f)));

        // ── BODOVÝ REFLEKTOR zhora ──
        var spotGO = new GameObject("PodiumSpot"); spotGO.transform.SetParent(root.transform, false);
        spotGO.transform.position = podium + Vector3.up * 3.4f - fwd * 0.3f;
        spotGO.transform.rotation = Quaternion.LookRotation((podium - spotGO.transform.position).normalized, Vector3.up);
        var spot = spotGO.AddComponent<Light>();
        spot.type = LightType.Spot; spot.spotAngle = 55f; spot.range = 9f;
        spot.color = new Color(1f, 0.96f, 0.85f); spot.intensity = 6.5f;

        // ── STAGE BACKDROP za pódiom (orientovaný k hráčovi) ──
        Vector3 backC = podium + fwd * 5.6f; backC.y = 0f;
        Quaternion face = Quaternion.LookRotation(-fwd, Vector3.up);
        Box("Backdrop", backC + Vector3.up * 2.6f, new Vector3(9f, 5.2f, 0.25f), face, Mat(new Color(0.13f, 0.14f, 0.18f)));
        // akcentové pruhy
        Box("StripeA", backC + Vector3.up * 4.6f + fwd * -0.14f, new Vector3(9f, 0.18f, 0.05f), face, Emissive(ACCENT));
        Box("StripeB", backC + Vector3.up * 0.7f + fwd * -0.14f, new Vector3(9f, 0.12f, 0.05f), face, Emissive(new Color(0.9f, 0.4f, 0.1f)));
        // bočné piliere
        Box("PillarL", backC + right *  4.3f + Vector3.up * 2.6f, new Vector3(0.4f, 5.2f, 0.4f), face, Mat(new Color(0.09f, 0.10f, 0.13f)));
        Box("PillarR", backC + right * -4.3f + Vector3.up * 2.6f, new Vector3(0.4f, 5.2f, 0.4f), face, Mat(new Color(0.09f, 0.10f, 0.13f)));

        // ── AMBIENT SVETLÁ (neutrálnejšie, nech scéna nie je hnedá/mútna) ──
        Point("WarmL", podium + right *  3.5f + Vector3.up * 2.4f, new Color(1f, 0.92f, 0.82f), 1.7f, 8f);
        Point("WarmR", podium + right * -3.5f + Vector3.up * 2.4f, new Color(0.9f, 0.93f, 1f), 1.4f, 8f);
        Point("BackGlow", backC + Vector3.up * 2.5f - fwd * 0.6f, ACCENT, 1.1f, 6f);

        // ── STAGE PODLAHA (mat pod showroomom) ──
        Vector3 matC = (baseC + backC) * 0.5f; matC.y = 0.04f;
        Box("StageMat",    matC,                       new Vector3(7.6f, 0.05f, 7.0f), Quaternion.identity, Mat(new Color(0.16f, 0.17f, 0.20f)));
        Box("StageMatRim", matC + Vector3.up * 0.006f, new Vector3(7.0f, 0.06f, 6.4f), Quaternion.identity, Emissive(new Color(0.30f, 0.14f, 0.04f)));

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
        Box("WeaponShelf", backC + Vector3.up * 2.12f - fwd * 0.26f, new Vector3(7.6f, 0.14f, 0.34f), face, Mat(new Color(0.09f, 0.10f, 0.13f)));
        for (int i = 0; i < shown; i++)
        {
            float k = shown > 1 ? (i / (float)(shown - 1) - 0.5f) : 0f;
            Vector3 wpos = backC + right * (k * 6.8f) + Vector3.up * 2.6f - fwd * 0.34f;
            Box("Peg", wpos - Vector3.up * 0.5f, new Vector3(0.5f, 0.06f, 0.18f), face, Mat(new Color(0.10f, 0.11f, 0.14f)));
            var wm = WeaponPreview.BuildModel(all[i]);
            wm.name = "WallWeapon_" + all[i].id;
            wm.transform.SetParent(root.transform, false);
            wm.transform.position = wpos;
            wm.transform.localScale = Vector3.one * 0.82f;
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

        // ── DOPLNKOVÉ PROPY (zaplnenie priestoru) ──
        Vector3 left = -right;
        // recepčný pult vľavo
        Box("Counter",    baseC + left * 4.0f + fwd * 0.4f + Vector3.up * 0.55f, new Vector3(2.4f, 1.1f, 0.8f), face, Mat(new Color(0.20f, 0.16f, 0.12f)));
        Box("CounterTop", baseC + left * 4.0f + fwd * 0.4f + Vector3.up * 1.13f, new Vector3(2.6f, 0.08f, 1.0f), face, Mat(new Color(0.30f, 0.24f, 0.16f)));
        // stoh debien vľavo-vzadu
        for (int i = 0; i < 3; i++)
            Box("Crate" + i, baseC + left * 3.6f + fwd * 3.2f + Vector3.up * (0.4f + i * 0.62f), new Vector3(0.7f, 0.6f, 0.7f), face, Mat(new Color(0.42f, 0.30f, 0.16f)));
        // rastliny v rohoch
        Plant(baseC + left * 4.4f - fwd * 1.2f);
        Plant(baseC + right * 4.4f - fwd * 1.2f);

        // ── VYLEPŠENIE IZBY + PRÉMIOVÉ TLAČIDLÁ ──
        PolishRoom();
        AddRoomDecor();
        StyleUI();
    }

    // Viac "vecí" do lobby: nástenné trimy, plagáty, automat, skrinky,
    // stropné svetlá a rastliny — aby izba žila a nebola prázdna.
    void AddRoomDecor()
    {
        Color warm = new Color(1f, 0.5f, 0.16f);
        Color cool = new Color(0.25f, 0.6f, 1f);
        var id = Quaternion.identity;

        // ── nástenné akcentové trimy (po obvode) ──
        Box("TrimBack", new Vector3(0f, 2.3f, 5.85f), new Vector3(15.6f, 0.12f, 0.06f), id, Emissive(warm));
        Box("TrimLeft", new Vector3(-7.85f, 2.3f, 0f), new Vector3(0.06f, 0.12f, 15.6f), id, Emissive(warm));

        // ── plagáty / podsvietené panely na ľavej stene ──
        Box("Poster1", new Vector3(-7.84f, 3.5f, -1.4f), new Vector3(0.05f, 1.5f, 2.0f), id, Emissive(new Color(1f, 0.35f, 0.12f)));
        Box("Poster2", new Vector3(-7.84f, 3.5f, 1.6f),  new Vector3(0.05f, 1.5f, 2.0f), id, Emissive(new Color(0.2f, 0.7f, 0.95f)));
        Box("Poster3", new Vector3(-7.84f, 3.5f, 4.4f),  new Vector3(0.05f, 1.5f, 2.0f), id, Emissive(new Color(0.85f, 0.2f, 0.5f)));

        // ── nápojový automat (break room vibe) v ľavom rohu ──
        Box("Vending",      new Vector3(-6.6f, 1.25f, 4.6f), new Vector3(1.5f, 2.5f, 0.95f), id, Mat(new Color(0.42f, 0.12f, 0.12f)));
        Box("VendingGlass", new Vector3(-6.6f, 1.55f, 4.10f), new Vector3(1.05f, 1.5f, 0.05f), id, Mat(new Color(0.5f, 0.58f, 0.68f)));
        Box("VendingTop",   new Vector3(-6.6f, 2.56f, 4.55f), new Vector3(1.55f, 0.14f, 1.0f), id, Emissive(new Color(0.9f, 0.45f, 0.14f)));

        // ── kovové skrinky pri ľavej stene ──
        for (int i = 0; i < 3; i++)
            Box("Locker" + i, new Vector3(-7.2f, 1.15f, 1.3f - i * 0.95f), new Vector3(0.85f, 2.3f, 0.7f), id,
                Mat(new Color(0.26f, 0.34f, 0.42f)));

        // ── stropné svetelné lišty + bodové svetlá ──
        for (int i = -1; i <= 1; i++)
        {
            Box("Ceil" + i, new Vector3(i * 3.4f, 5.75f, 2.5f), new Vector3(2.6f, 0.1f, 0.4f), id, Emissive(new Color(1f, 0.95f, 0.85f)));
            Point("CeilGlow" + i, new Vector3(i * 3.4f, 5.2f, 2.5f), new Color(1f, 0.93f, 0.8f), 1.1f, 7f);
        }

        // ── viac rastlín ──
        Plant(new Vector3(-7f, 0f, -2.2f));
        Plant(new Vector3(-4.4f, 0f, 5.2f));
        Plant(new Vector3(7f, 0f, -2.4f));
    }

    // Lepšie materiály stien/podlahy + teplejšie svetlo (nie fádna sivá).
    void PolishRoom()
    {
        // čistá chladná industriálna paleta (ako hangár v Main Menu) + oranžové akcenty
        Recolor("Podlaha",     new Color(0.20f, 0.21f, 0.24f));
        Recolor("Stena_Zadna", new Color(0.28f, 0.29f, 0.34f));
        Recolor("Stena_Lava",  new Color(0.25f, 0.26f, 0.31f));
        Recolor("Stena_Prava", new Color(0.25f, 0.26f, 0.31f));
        Recolor("Stage",       new Color(0.17f, 0.18f, 0.22f));   // pôvodná krikľavo červená platforma → neutrál
        Recolor("Stol",        new Color(0.30f, 0.24f, 0.18f));
        Recolor("PodiumBase",  new Color(0.14f, 0.15f, 0.18f));

        // jasnejší kľúčový svit (scéna bola mútna/tmavá)
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l != null && l.type == LightType.Directional)
            {
                l.color = new Color(1f, 0.96f, 0.9f); l.intensity = 1.35f;
                l.transform.rotation = Quaternion.Euler(46f, -26f, 0f);
            }

        RenderSettings.ambientLight = new Color(0.36f, 0.37f, 0.42f);

        // jemné akcentové svetlá do priestoru izby
        Point("RoomGlowL", new Vector3(-6f, 3.4f, 4f), new Color(1f, 0.6f, 0.3f), 1.6f, 14f);
        Point("RoomGlowR", new Vector3( 6f, 3.4f, 4f), new Color(0.35f, 0.55f, 1f), 1.3f, 14f);
    }

    void Recolor(string name, Color c)
    {
        var go = GameObject.Find(name);
        if (go == null) return;
        var r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = Mat(c);
    }

    // Prémiové zaoblené tlačidlá (ako nové Main Menu) — runtime reštýl scénových tlačidiel.
    void StyleUI()
    {
        // taby (farbu prepína HubManager, my dáme zaoblený tvar + text)
        StyleBtn("PlayTab",    UITheme.Panel,    22, false);
        StyleBtn("LoadoutTab", UITheme.Panel,    22, false);
        StyleBtn("ShopTab",    UITheme.Panel,    22, false);
        // ostatné tlačidlá s plnou farbou
        StyleBtn("BackBtn",    UITheme.Panel,    20, true);
        StyleBtn("MapBtn",     UITheme.PanelLight,24, true);
        StyleBtn("StartBtn",   UITheme.Good,     34, true);

        // horná lišta tmavšia + akcentový spodný prúžok
        var top = GameObject.Find("TopBar");
        if (top != null)
        {
            var img = top.GetComponent<Image>();
            if (img != null) img.color = new Color(0.08f, 0.08f, 0.10f, 0.96f);
            if (top.transform.Find("AccentLine") == null)
            {
                var line = new GameObject("AccentLine"); line.transform.SetParent(top.transform, false);
                var li = line.AddComponent<Image>(); li.color = UITheme.Accent;
                var lr = line.GetComponent<RectTransform>();
                lr.anchorMin = new Vector2(0, 0); lr.anchorMax = new Vector2(1, 0); lr.pivot = new Vector2(0.5f, 0);
                lr.sizeDelta = new Vector2(0, 3); lr.anchoredPosition = Vector2.zero;
            }
        }
    }

    void StyleBtn(string name, Color col, int fontSize, bool setColor)
    {
        var go = GameObject.Find(name);
        if (go == null) return;
        var img = go.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = UITheme.Rounded(14); img.type = Image.Type.Sliced;
            if (setColor) img.color = col;
        }
        var btn = go.GetComponent<Button>();
        if (btn != null && img != null) UITheme.Hover(btn, col, col);
        UITheme.Shadow(go, new Vector2(0, -3), 0.4f);
        var lbl = go.GetComponentInChildren<Text>();
        if (lbl != null)
        {
            lbl.fontSize = fontSize; lbl.fontStyle = FontStyle.Bold;
            lbl.color = name == "StartBtn" ? Color.white : new Color(1f, 0.9f, 0.6f);
        }
    }

    void Plant(Vector3 at)
    {
        Cyl("PlantPot", at + Vector3.up * 0.3f, new Vector3(0.45f, 0.3f, 0.45f), Mat(new Color(0.35f, 0.22f, 0.14f)));
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var c = g.GetComponent<Collider>(); if (c != null) Destroy(c);
        g.name = "PlantTop"; g.transform.SetParent(root.transform, false);
        g.transform.position = at + Vector3.up * 1.0f; g.transform.localScale = new Vector3(0.8f, 1.1f, 0.8f);
        g.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.20f, 0.45f, 0.22f));
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
