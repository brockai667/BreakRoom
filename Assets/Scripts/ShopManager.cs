using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI Root")]
    public Transform cardContainer;   // auto-nájdený
    public Text moneyText;

    void Start()
    {
        if (cardContainer == null)
            cardContainer = GameObject.Find("CardContainer")?.transform;
        if (moneyText == null)
            moneyText = GameObject.Find("MoneyText")?.GetComponent<Text>();

        // Vytvor PlayerInventory ak neexistuje
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

    void CreateCard(WeaponData w)
    {
        // Card background
        var cardGO = new GameObject("Card_" + w.id);
        cardGO.transform.SetParent(cardContainer, false);
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.15f, 0.08f, 0.04f, 0.95f);
        var cardRect = cardGO.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot     = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(200, 290);
        // Pevná veľkosť karty, aby ju HorizontalLayoutGroup nezrazil na nulu
        var cardLE = cardGO.AddComponent<LayoutElement>();
        cardLE.preferredWidth = 200; cardLE.minWidth = 200;
        cardLE.preferredHeight = 290; cardLE.minHeight = 290;

        // Weapon icon (colored square)
        var iconGO = new GameObject("Icon"); iconGO.transform.SetParent(cardGO.transform, false);
        var iconImg = iconGO.AddComponent<Image>(); iconImg.color = w.handColor;
        var iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchoredPosition = new Vector2(0, 70);
        iconRect.sizeDelta = new Vector2(80, 80);

        // Handle indicator
        var hndGO = new GameObject("Handle"); hndGO.transform.SetParent(cardGO.transform, false);
        var hndImg = hndGO.AddComponent<Image>(); hndImg.color = w.handleColor;
        var hndRect = hndGO.GetComponent<RectTransform>();
        hndRect.anchoredPosition = new Vector2(0, 20);
        hndRect.sizeDelta = new Vector2(14, 50);

        // Name text
        var nameGO = MakeText(cardGO, w.displayName, 18, FontStyle.Bold,
            new Color(1f, 0.85f, 0.1f), new Vector2(0, -45), new Vector2(180, 28));

        // Description
        MakeText(cardGO, w.description, 13, FontStyle.Normal,
            Color.white, new Vector2(0, -95), new Vector2(180, 55));

        // Splash info
        string splashTxt = w.splashRadius > 0 ? $"Splash: {w.splashRadius:0.0}m" : "Bez splash";
        MakeText(cardGO, $"DMG: {w.damage}  {splashTxt}", 12, FontStyle.Normal,
            new Color(0.7f, 0.9f, 1f), new Vector2(0, -135), new Vector2(180, 22));

        // Price text
        string priceTxt = w.price == 0 ? "ZADARMO" : "$" + w.price;
        MakeText(cardGO, priceTxt, 16, FontStyle.Bold,
            w.price == 0 ? Color.green : new Color(1f, 0.85f, 0.1f),
            new Vector2(0, -110), new Vector2(180, 24));

        // Button
        bool owned    = PlayerInventory.Instance.Owns(w.id);
        bool equipped = PlayerInventory.Instance.EquippedId == w.id;
        string btnLabel = equipped ? "✓ EQUIPPED" : owned ? "EQUIP" : "$" + w.price + " KÚPIŤ";
        Color  btnColor = equipped ? new Color(0.1f,0.5f,0.1f) :
                          owned    ? new Color(0.1f,0.3f,0.6f) : new Color(0.5f,0.15f,0.05f);

        var btnGO = new GameObject("Btn"); btnGO.transform.SetParent(cardGO.transform, false);
        var btnImg2 = btnGO.AddComponent<Image>(); btnImg2.color = btnColor;
        var btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -105);
        btnRect.sizeDelta = new Vector2(175, 38);
        var btn = btnGO.AddComponent<Button>(); btn.targetGraphic = btnImg2;
        MakeText(btnGO, btnLabel, 14, FontStyle.Bold, Color.white, Vector2.zero, new Vector2(170,34));

        string capturedId = w.id;
        btn.onClick.AddListener(() => OnCardClick(capturedId));
        if (equipped) btn.interactable = false;
    }

    void OnCardClick(string id)
    {
        var inv = PlayerInventory.Instance;
        if (inv.Owns(id))
            inv.Equip(id);
        else
            inv.TryBuy(id);
        RefreshCards();
    }

    void RefreshCards() { UpdateMoney(); BuildCards(); }

    GameObject MakeText(GameObject parent, string txt, int size, FontStyle style,
        Color col, Vector2 anchorPos, Vector2 sizeD)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchoredPosition = anchorPos; r.sizeDelta = sizeD;
        var t = go.AddComponent<Text>(); t.text = txt; t.fontSize = size;
        t.fontStyle = style; t.color = col; t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    public void GoBack() { SceneManager.LoadScene("MainMenu"); }
}
