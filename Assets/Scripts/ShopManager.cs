using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// Obrazovka obchodu (Shop). Generuje mriežku kariet zbraní z WeaponData
/// (rarita podľa ceny, 3D ikona cez WeaponIcon s 2D fallbackom), rieši
/// kúpu/equip/upgrade cez PlayerInventory a náhľad na pódiu cez WeaponPreview.
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

        // väčšie karty, nech sa zmestí 3D ikona aj väčší text
        if (cardContainer != null)
        {
            var grid = cardContainer.GetComponent<GridLayoutGroup>();
            if (grid != null) { grid.cellSize = new Vector2(228, 322); grid.spacing = new Vector2(22, 22); }
        }

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
        MakeText(body, w.displayName, 25, FontStyle.Bold, new Color(1f, 0.9f, 0.45f),
                 0.04f, 0.455f, 0.96f, 0.56f, TextAnchor.MiddleCenter);
        // Rarita
        MakeText(body, rarName, 15, FontStyle.Bold, rarCol,
                 0.04f, 0.395f, 0.96f, 0.455f, TextAnchor.MiddleCenter);
        // DMG / splash
        string splashTxt = w.splashRadius > 0 ? $"Splash {w.splashRadius:0.0}m" : "No splash";
        MakeText(body, $"DMG {w.damage}    {splashTxt}", 15, FontStyle.Bold, new Color(0.78f, 0.87f, 1f),
                 0.04f, 0.315f, 0.96f, 0.39f, TextAnchor.MiddleCenter);

        // Tlačidlo (kúpiť / equip / equipped / zamknuté levelom / PREMIUM)
        int    needLvl = WeaponData.UnlockLevel(w.id);
        bool   premium = w.id == "flamethrower";          // prémiový = za reálne peniaze (IAP)
        bool   locked  = !owned && !premium && XPManager.SavedLevel < needLvl;
        string btnLabel = equipped ? "✓ EQUIPPED"
                        : owned    ? "EQUIP"
                        : premium  ? "PREMIUM  €2.99"
                        : locked   ? ("LOCKED  Lvl " + needLvl)
                        : (w.price == 0 ? "FREE" : "$" + w.price);
        Color  btnColor = equipped ? new Color(0.10f, 0.45f, 0.12f)
                        : owned    ? new Color(0.12f, 0.32f, 0.60f)
                        : premium  ? new Color(0.85f, 0.65f, 0.10f)
                        : locked   ? new Color(0.18f, 0.18f, 0.20f)
                                   : new Color(0.55f, 0.16f, 0.05f);

        var btnGO = new GameObject("BuyBtn"); btnGO.transform.SetParent(body.transform, false);
        var btnImg = btnGO.AddComponent<Image>(); btnImg.color = btnColor;
        btnImg.sprite = UITheme.Rounded(10); btnImg.type = Image.Type.Sliced;
        PlaceFrac(btnGO.GetComponent<RectTransform>(), 0.10f, 0.07f, 0.90f, 0.20f);
        var btn = btnGO.AddComponent<Button>(); btn.targetGraphic = btnImg; UITheme.Hover(btn, btnColor, btnColor);
        MakeTextRect(btnGO, btnLabel, 19, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        btn.onClick.AddListener(() => OnCardClick(pid));
        if (equipped || locked) btn.interactable = false;

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
            MakeTextRect(upGO, upLabel, 15, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
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
            MakeTextRect(badge, "OWNED", 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        }
    }

    void BuildIcon(GameObject parent, WeaponData w)
    {
        var area = new GameObject("IconArea", typeof(RectTransform));
        area.transform.SetParent(parent.transform, false);
        PlaceFrac(area.GetComponent<RectTransform>(), 0.08f, 0.55f, 0.92f, 0.99f);

        // jemná žiara podľa rarity za zbraňou
        Rarity(w, out _, out Color rc);
        var glow = new GameObject("Glow"); glow.transform.SetParent(area.transform, false);
        var gi = glow.AddComponent<Image>(); gi.sprite = UITheme.Rounded(44); gi.type = Image.Type.Sliced;
        gi.color = new Color(rc.r, rc.g, rc.b, 0.16f);
        var grt = glow.GetComponent<RectTransform>();
        grt.anchorMin = new Vector2(0.14f, 0.08f); grt.anchorMax = new Vector2(0.86f, 0.92f);
        grt.offsetMin = grt.offsetMax = Vector2.zero;

        // 3D render modelu zbrane (pekná ikona). Pri zlyhaní → 2D silueta.
        var sprite = WeaponIcon.Get(w);
        if (sprite != null)
        {
            var img = new GameObject("Icon3D"); img.transform.SetParent(area.transform, false);
            var im = img.AddComponent<Image>(); im.sprite = sprite; im.preserveAspect = true;
            var rt = img.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        else BuildIcon2D(area, w);
    }

    // Záložná 2D silueta (ak by sa 3D render nepodaril)
    void BuildIcon2D(GameObject area, WeaponData w)
    {
        var handle = new GameObject("Handle"); handle.transform.SetParent(area.transform, false);
        var hi = handle.AddComponent<Image>(); hi.color = w.handleColor;
        var hr = handle.GetComponent<RectTransform>();
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0f); hr.pivot = new Vector2(0.5f, 0f);
        hr.anchoredPosition = new Vector2(0, 6); hr.sizeDelta = new Vector2(16, 62);

        var head = new GameObject("Head"); head.transform.SetParent(area.transform, false);
        var hdi = head.AddComponent<Image>(); hdi.color = w.handColor;
        var hdr = head.GetComponent<RectTransform>();
        hdr.anchorMin = hdr.anchorMax = new Vector2(0.5f, 0f); hdr.pivot = new Vector2(0.5f, 0f);
        hdr.anchoredPosition = new Vector2(0, 50); hdr.sizeDelta = HeadSize(w.id);
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
        else if (id == "flamethrower")
        {
            // PREMIUM (reálne peniaze) — tu by sa volalo Unity IAP. Po úspešnej
            // platbe sa zbraň udelí. Zatiaľ placeholder: udelí ju priamo.
            inv.GiveWeapon(id); inv.Equip(id);
            if (SfxManager.Instance != null) SfxManager.Coin();
        }
        else if (XPManager.SavedLevel >= WeaponData.UnlockLevel(id)) inv.TryBuy(id);
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

    /// Návrat do hlavného menu.
    public void GoBack() { SceneManager.LoadScene("MainMenu"); }
}
