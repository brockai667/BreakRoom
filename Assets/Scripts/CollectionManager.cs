using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Obrazovka ZBIERKA (scéna "Collection"). Postaví sa celá z kódu, takže
/// netreba nič klikať v editore (rovnaký prístup ako SpecialObjects).
/// Ukazuje progres hráča (level, peniaze, koľko zbraní je odomknutých) a
/// mriežku všetkých zbraní s ich stavom (equipnuté / vlastnené / zamknuté).
public class CollectionManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("CollectionManager");
        DontDestroyOnLoad(go);
        var cm = go.AddComponent<CollectionManager>();
        SceneManager.sceneLoaded += (s, m) => cm.OnScene(s.name);
        cm.OnScene(SceneManager.GetActiveScene().name);
    }

    static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    void OnScene(string scene)
    {
        if (scene != "Collection") return;
        if (PlayerInventory.Instance == null)
            new GameObject("PlayerInventory").AddComponent<PlayerInventory>();
        Build();
    }

    // ---------- STAVBA UI ----------
    void Build()
    {
        // EventSystem (len ak v scéne ešte nie je)
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Vlastný Canvas navrch (prekryje placeholder z prázdnej scény)
        var canvasGO = new GameObject("CollectionCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Pozadie (plné, tmavé)
        var bg = new GameObject("BG");
        bg.transform.SetParent(canvasGO.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.07f, 0.05f, 0.04f, 1f);
        Stretch(bg.GetComponent<RectTransform>());

        // Nadpis
        MakeText(canvasGO, "ZBIERKA", 64, FontStyle.Bold, new Color(1f, 0.9f, 0.45f),
                 new Vector2(0f, 0.88f), new Vector2(1f, 0.99f), TextAnchor.MiddleCenter);

        // Riadok so štatistikami
        int level   = PlayerPrefs.GetInt("Level", 1);
        int money   = PlayerInventory.Instance != null ? PlayerInventory.Instance.Money
                                                        : PlayerPrefs.GetInt("Money", 0);
        int owned   = 0;
        foreach (var w in WeaponData.All)
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.Owns(w.id)) owned++;
        int total = WeaponData.All.Length;

        string stats = $"Level {level}        $ {money}        Odomknuté zbrane: {owned}/{total}";
        MakeText(canvasGO, stats, 26, FontStyle.Bold, new Color(0.8f, 0.85f, 1f),
                 new Vector2(0f, 0.80f), new Vector2(1f, 0.87f), TextAnchor.MiddleCenter);

        // Mriežka kariet
        var gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(canvasGO.transform, false);
        var grt = gridGO.AddComponent<RectTransform>();
        grt.anchorMin = new Vector2(0.5f, 0.5f);
        grt.anchorMax = new Vector2(0.5f, 0.5f);
        grt.pivot     = new Vector2(0.5f, 0.5f);
        grt.anchoredPosition = new Vector2(0, -30);
        var grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(220, 250);
        grid.spacing  = new Vector2(20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.MiddleCenter;
        var fit = gridGO.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fit.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        foreach (var w in WeaponData.All) CreateCard(gridGO.transform, w);

        // Tlačidlo SPÄŤ
        var backGO = new GameObject("BackButton");
        backGO.transform.SetParent(canvasGO.transform, false);
        var backImg = backGO.AddComponent<Image>();
        backImg.color = new Color(0.45f, 0.14f, 0.05f);
        var brt = backGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.06f);
        brt.anchorMax = new Vector2(0.5f, 0.06f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero;
        brt.sizeDelta = new Vector2(260, 64);
        var back = backGO.AddComponent<Button>();
        back.targetGraphic = backImg;
        back.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        MakeTextStretch(backGO, "SPÄŤ DO MENU", 24, FontStyle.Bold, Color.white);
    }

    // ---------- KARTA ZBRANE ----------
    void CreateCard(Transform parent, WeaponData w)
    {
        var inv      = PlayerInventory.Instance;
        bool owned    = inv != null && inv.Owns(w.id);
        bool equipped = inv != null && inv.EquippedId == w.id;
        Rarity(w, out string rarName, out Color rarCol);

        // Rámik = farba rarity
        var card = new GameObject("Card_" + w.id);
        card.transform.SetParent(parent, false);
        var border = card.AddComponent<Image>();
        border.color = owned ? rarCol : new Color(0.3f, 0.3f, 0.32f);

        // Telo
        var body = new GameObject("Body");
        body.transform.SetParent(card.transform, false);
        var bodyImg = body.AddComponent<Image>();
        bodyImg.color = owned ? new Color(0.11f, 0.08f, 0.06f, 1f)
                              : new Color(0.06f, 0.05f, 0.05f, 1f);
        StretchPad(body.GetComponent<RectTransform>(), 4, 4);

        // Ikona (rukoväť + hlavica) — sivá ak zamknuté
        BuildIcon(body, w, owned);

        // Názov
        MakeTextFrac(body, w.displayName, 19, FontStyle.Bold,
                     owned ? new Color(1f, 0.9f, 0.45f) : new Color(0.6f, 0.6f, 0.62f),
                     0.04f, 0.40f, 0.96f, 0.52f, TextAnchor.MiddleCenter);
        // Rarita
        MakeTextFrac(body, rarName, 13, FontStyle.Bold,
                     owned ? rarCol : new Color(0.5f, 0.5f, 0.52f),
                     0.04f, 0.32f, 0.96f, 0.40f, TextAnchor.MiddleCenter);

        // Stav (equipnuté / vlastnené / cena)
        string statusTxt = equipped ? "✓ NASADENÉ"
                         : owned    ? "VLASTNÍŠ"
                         : (w.price == 0 ? "ZADARMO" : "🔒 $" + w.price);
        Color statusCol  = equipped ? new Color(0.35f, 0.9f, 0.4f)
                         : owned    ? new Color(0.55f, 0.7f, 1f)
                                    : new Color(1f, 0.55f, 0.3f);
        MakeTextFrac(body, statusTxt, 16, FontStyle.Bold, statusCol,
                     0.04f, 0.07f, 0.96f, 0.20f, TextAnchor.MiddleCenter);
    }

    void BuildIcon(GameObject parent, WeaponData w, bool owned)
    {
        Color hcol  = owned ? w.handColor   : new Color(0.4f, 0.4f, 0.42f);
        Color hgcol = owned ? w.handleColor : new Color(0.3f, 0.3f, 0.32f);

        var area = new GameObject("IconArea", typeof(RectTransform));
        area.transform.SetParent(parent.transform, false);
        PlaceFrac(area.GetComponent<RectTransform>(), 0.25f, 0.55f, 0.75f, 0.95f);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(area.transform, false);
        var hi = handle.AddComponent<Image>(); hi.color = hgcol;
        var hr = handle.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0f); hr.pivot = new Vector2(0.5f, 0f);
        hr.anchoredPosition = new Vector2(0, 6); hr.sizeDelta = new Vector2(14, 54);

        var head = new GameObject("Head");
        head.transform.SetParent(area.transform, false);
        var hdi = head.AddComponent<Image>(); hdi.color = hcol;
        var hdr = head.GetComponent<RectTransform>();
        hdr.anchorMin = hdr.anchorMax = new Vector2(0.5f, 0f); hdr.pivot = new Vector2(0.5f, 0f);
        hdr.anchoredPosition = new Vector2(0, 44); hdr.sizeDelta = HeadSize(w.id);
    }

    static Vector2 HeadSize(string id)
    {
        switch (id)
        {
            case "fists":        return new Vector2(46, 44);
            case "bat":          return new Vector2(20, 78);
            case "gloves":       return new Vector2(56, 40);
            case "hammer":       return new Vector2(62, 34);
            case "axe":          return new Vector2(56, 56);
            case "sledge":       return new Vector2(72, 44);
            case "flamethrower": return new Vector2(82, 34);
            default:             return new Vector2(40, 62);
        }
    }

    // Rovnaké rarity ako v ShopManager (konzistentný vzhľad)
    static void Rarity(WeaponData w, out string name, out Color col)
    {
        if (w.price == 0)         { name = "COMMON";    col = new Color(0.60f, 0.60f, 0.62f); }
        else if (w.price <= 300)  { name = "UNCOMMON";  col = new Color(0.30f, 0.80f, 0.35f); }
        else if (w.price <= 1200) { name = "RARE";      col = new Color(0.30f, 0.60f, 1.00f); }
        else if (w.price <= 2500) { name = "EPIC";      col = new Color(0.72f, 0.35f, 1.00f); }
        else                      { name = "LEGENDARY"; col = new Color(1.00f, 0.65f, 0.10f); }
    }

    // ---------- UI HELPERY ----------
    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    static void StretchPad(RectTransform r, float padX, float padY)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(padX, padY); r.offsetMax = new Vector2(-padX, -padY);
    }

    static void PlaceFrac(RectTransform r, float xmin, float ymin, float xmax, float ymax)
    {
        r.anchorMin = new Vector2(xmin, ymin); r.anchorMax = new Vector2(xmax, ymax);
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    // text cez anchory na rodičovi (0..1)
    GameObject MakeText(GameObject parent, string txt, int size, FontStyle style, Color col,
        Vector2 anchorMin, Vector2 anchorMax, TextAnchor anchor)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>();
        t.text = txt; t.fontSize = size; t.fontStyle = style; t.color = col;
        t.alignment = anchor; t.font = Font;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return go;
    }

    GameObject MakeTextFrac(GameObject parent, string txt, int size, FontStyle style, Color col,
        float xmin, float ymin, float xmax, float ymax, TextAnchor anchor)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        PlaceFrac(r, xmin, ymin, xmax, ymax);
        var t = go.AddComponent<Text>();
        t.text = txt; t.fontSize = size; t.fontStyle = style; t.color = col;
        t.alignment = anchor; t.font = Font;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return go;
    }

    GameObject MakeTextStretch(GameObject parent, string txt, int size, FontStyle style, Color col)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        Stretch(r);
        var t = go.AddComponent<Text>();
        t.text = txt; t.fontSize = size; t.fontStyle = style; t.color = col;
        t.alignment = TextAnchor.MiddleCenter; t.font = Font;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return go;
    }
}
