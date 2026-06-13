using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI Root")]
    public Transform cardContainer;   // obsah GridLayoutGroup
    public Text moneyText;

    void Start()
    {
        if (cardContainer == null)
            cardContainer = GameObject.Find("CardContainer")?.transform;
        if (moneyText == null)
            moneyText = GameObject.Find("MoneyText")?.GetComponent<Text>();

        if (PlayerInventory.Instance == null)
            new GameObject("PlayerInventory").AddComponent<PlayerInventory>();

        UpdateMoney();
        BuildCards();
        PlayerInventory.Instance.OnChanged += RefreshCards;
    }

    void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnChanged -= RefreshCards;
    }

    void UpdateMoney()
    {
        if (moneyText != null && PlayerInventory.Instance != null)
            moneyText.text = "$" + PlayerInventory.Instance.Money;
    }

    void BuildCards()
    {
        if (cardContainer == null) return;
        foreach (Transform child in cardContainer) Destroy(child.gameObject);
        foreach (var w in WeaponData.All) CreateCard(w);
    }

    // ---------- RARITA ----------
    static void Rarity(WeaponData w, out string name, out Color col)
    {
        if (w.price == 0)        { name = "COMMON";    col = new Color(0.60f, 0.60f, 0.62f); }
        else if (w.price <= 300) { name = "UNCOMMON";  col = new Color(0.30f, 0.80f, 0.35f); }
        else if (w.price <= 1200){ name = "RARE";      col = new Color(0.30f, 0.60f, 1.00f); }
        else if (w.price <= 2500){ name = "EPIC";      col = new Color(0.72f, 0.35f, 1.00f); }
        else                     { name = "LEGENDARY"; col = new Color(1.00f, 0.65f, 0.10f); }
    }

    // ---------- KARTA ----------
    void CreateCard(WeaponData w)
    {
        bool owned    = PlayerInventory.Instance.Owns(w.id);
        bool equipped = PlayerInventory.Instance.EquippedId == w.id;
        Rarity(w, out string rarName, out Color rarCol);

        // Rámik = farba rarity (veľkosť riadi GridLayoutGroup)
        var card = new GameObject("Card_" + w.id);
        card.transform.SetParent(cardContainer, false);
        var border = card.AddComponent<Image>();
        border.sprite = UITheme.Rounded(16); border.type = Image.Type.Sliced;
        border.color = rarCol;

        // Telo karty (tmavé, mierne odsadené od rámika)
        var body = new GameObject("Body"); body.transform.SetParent(card.transform, false);
        var bodyImg = body.AddComponent<Image>(); bodyImg.color = new Color(0.10f, 0.07f, 0.05f, 0.99f);
        bodyImg.sprite = UITheme.Rounded(13); bodyImg.type = Image.Type.Sliced;
        Stretch(body.GetComponent<RectTransform>(), 4, 4);

        // Hover efekt
        var hover = card.AddComponent<CardHover>();
        hover.border = border; hover.normal = rarCol; hover.highlight = Color.white;

        // Klik na kartu = náhľad na pódiu
        var cardBtn = body.AddComponent<Button>(); cardBtn.targetGraphic = bodyImg;
        string pid = w.id;
        cardBtn.onClick.AddListener(() => Preview(pid));

        // Ikona zbrane (silueta)
        BuildIcon(body, w);

        // Názov
        MakeText(body, w.displayName, 19, FontStyle.Bold, new Color(1f, 0.9f, 0.45f),
                 0.04f, 0.47f, 0.96f, 0.57f, TextAnchor.MiddleCenter);
        // Rarita
        MakeText(body, rarName, 12, FontStyle.Bold, rarCol,
                 0.04f, 0.40f, 0.96f, 0.47f, TextAnchor.MiddleCenter);
        // DMG / splash
        string splashTxt = w.splashRadius > 0 ? $"Splash {w.splashRadius:0.0}m" : "Bez splash";
        MakeText(body, $"DMG {w.damage}    {splashTxt}", 12, FontStyle.Normal, new Color(0.75f, 0.85f, 1f),
                 0.04f, 0.33f, 0.96f, 0.40f, TextAnchor.MiddleCenter);

        // Tlačidlo (kúpiť / equip / equipped)
        string btnLabel = equipped ? "✓ EQUIPPED" : owned ? "EQUIP" : (w.price == 0 ? "FREE" : "$" + w.price);
        Color  btnColor = equipped ? new Color(0.10f, 0.45f, 0.12f)
                        : owned    ? new Color(0.12f, 0.32f, 0.60f)
                                   : new Color(0.55f, 0.16f, 0.05f);

        var btnGO = new GameObject("BuyBtn"); btnGO.transform.SetParent(body.transform, false);
        var btnImg = btnGO.AddComponent<Image>(); btnImg.color = btnColor;
        btnImg.sprite = UITheme.Rounded(10); btnImg.type = Image.Type.Sliced;
        PlaceFrac(btnGO.GetComponent<RectTransform>(), 0.10f, 0.07f, 0.90f, 0.20f);
        var btn = btnGO.AddComponent<Button>(); btn.targetGraphic = btnImg; UITheme.Hover(btn, btnColor, btnColor);
        MakeTextRect(btnGO, btnLabel, 15, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        btn.onClick.AddListener(() => OnCardClick(pid));
        if (equipped) btn.interactable = false;

        // UPGRADE tlacidlo (len vlastnene zbrane, do max levelu)
        if (owned)
        {
            int lvl = PlayerInventory.Instance.UpgradeLevel(pid);
            bool maxed = lvl >= PlayerInventory.MAX_UPGRADE;
            int cost = PlayerInventory.Instance.UpgradeCost(pid);
            string upLabel = maxed ? "MAX UPGRADE" : ("UPGRADE Lv" + lvl + "  $" + cost);
            Color upColor = maxed ? new Color(0.20f, 0.20f, 0.22f) : new Color(0.35f, 0.55f, 0.12f);
            var upGO = new GameObject("UpgradeBtn"); upGO.transform.SetParent(body.transform, false);
            var upImg = upGO.AddComponent<Image>(); upImg.color = upColor;
            upImg.sprite = UITheme.Rounded(10); upImg.type = Image.Type.Sliced;
            PlaceFrac(upGO.GetComponent<RectTransform>(), 0.10f, 0.205f, 0.90f, 0.30f);
            var upBtn = upGO.AddComponent<Button>(); upBtn.targetGraphic = upImg; UITheme.Hover(upBtn, upColor, upColor);
            MakeTextRect(upGO, upLabel, 12, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            if (maxed) upBtn.interactable = false;
            else upBtn.onClick.AddListener(() => OnUpgradeClick(pid));
        }

        // Badge OWNED v rohu (ak vlastní a nie je equipnuté)
        if (owned && !equipped)
        {
            var badge = new GameObject("Badge"); badge.transform.SetParent(body.transform, false);
            var bImg = badge.AddComponent<Image>(); bImg.color = new Color(0.12f, 0.32f, 0.60f, 0.95f);
            bImg.sprite = UITheme.Rounded(8); bImg.type = Image.Type.Sliced;
            PlaceFrac(badge.GetComponent<RectTransform>(), 0.55f, 0.88f, 0.97f, 0.98f);
            MakeTextRect(badge, "OWNED", 11, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        }
    }

    void BuildIcon(GameObject parent, WeaponData w)
    {
        var area = new GameObject("IconArea", typeof(RectTransform));
        area.transform.SetParent(parent.transform, false);
        PlaceFrac(area.GetComponent<RectTransform>(), 0.2f, 0.58f, 0.8f, 0.95f);

        var handle = new GameObject("Handle"); handle.transform.SetParent(area.transform, false);
        var hi = handle.AddComponent<Image>(); hi.color = w.handleColor;
        var hr = handle.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0f); hr.pivot = new Vector2(0.5f, 0f);
        hr.anchoredPosition = new Vector2(0, 4); hr.sizeDelta = new Vector2(14, 52);

        var head = new GameObject("Head"); head.transform.SetParent(area.transform, false);
        var hdi = head.AddComponent<Image>(); hdi.color = w.handColor;
        var hdr = head.GetComponent<RectTransform>();
        hdr.anchorMin = hdr.anchorMax = new Vector2(0.5f, 0f); hdr.pivot = new Vector2(0.5f, 0f);
        hdr.anchoredPosition = new Vector2(0, 42); hdr.sizeDelta = HeadSize(w.id);
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

    void Preview(string id)
    {
        if (WeaponPreview.Instance != null) WeaponPreview.Instance.Show(WeaponData.Get(id));
    }

    void OnCardClick(string id)
    {
        var inv = PlayerInventory.Instance;
        if (inv.Owns(id)) inv.Equip(id);
        else inv.TryBuy(id);
        Preview(id);
        RefreshCards();
    }

    void OnUpgradeClick(string id)
    {
        if (PlayerInventory.Instance.TryUpgrade(id)) SfxManager.Coin();
        Preview(id);
        RefreshCards();
    }

    void RefreshCards() { UpdateMoney(); BuildCards(); }

    // ---------- HELPERY ----------
    static void Stretch(RectTransform r, float padX, float padY)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(padX, padY); r.offsetMax = new Vector2(-padX, -padY);
    }

    static void PlaceFrac(RectTransform r, float xmin, float ymin, float xmax, float ymax)
    {
        r.anchorMin = new Vector2(xmin, ymin); r.anchorMax = new Vector2(xmax, ymax);
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    GameObject MakeText(GameObject parent, string txt, int size, FontStyle style, Color col,
        float xmin, float ymin, float xmax, float ymax, TextAnchor anchor)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        PlaceFrac(r, xmin, ymin, xmax, ymax);
        var t = go.AddComponent<Text>(); t.text = txt; t.fontSize = size; t.fontStyle = style;
        t.color = col; t.alignment = anchor; t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    GameObject MakeTextRect(GameObject parent, string txt, int size, FontStyle style, Color col, TextAnchor anchor)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>(); t.text = txt; t.fontSize = size; t.fontStyle = style;
        t.color = col; t.alignment = anchor; t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    public void GoBack() { SceneManager.LoadScene("MainMenu"); }
}
