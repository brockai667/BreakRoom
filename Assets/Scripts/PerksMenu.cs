using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Panel globalnych perkov v Hube. Self-bootstrapping, moderny vzhlad (UITheme).
public class PerksMenu : MonoBehaviour
{
    static PerksMenu inst;
    GameObject canvasGO, overlay, box;
    Text moneyText;
    Text[] lvlT;
    Text[] costT;
    Button[] buyBtn;
    RectTransform[] lvlFill;

    static Font F => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("PerksMenu");
        DontDestroyOnLoad(go);
        inst = go.AddComponent<PerksMenu>();
        SceneManager.sceneLoaded += (s, m) => { if (inst != null) inst.OnScene(s.name); };
        inst.OnScene(SceneManager.GetActiveScene().name);
    }

    void OnScene(string scene)
    {
        if (canvasGO != null) { Destroy(canvasGO); canvasGO = null; overlay = null; box = null; }
        if (scene != "Hub") return;
        Build();
    }

    void Build()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        }

        canvasGO = new GameObject("PerksCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 158;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Open button (under SETTINGS)
        var open = Btn(canvasGO.transform, "PERKS", new Vector2(1, 1), new Vector2(-24, -160),
                       new Vector2(196, 56), UITheme.Accent, 22, OpenPanel, 14);
        UITheme.Shadow(open.gameObject, new Vector2(0, -3));

        BuildPanel();
        overlay.SetActive(false);
    }

    void BuildPanel()
    {
        overlay = new GameObject("PerksOverlay");
        overlay.transform.SetParent(canvasGO.transform, false);
        var dim = overlay.AddComponent<Image>(); dim.color = UITheme.Overlay;
        var ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one; ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
        var dimBtn = overlay.AddComponent<Button>(); dimBtn.targetGraphic = dim;
        dimBtn.transition = Selectable.Transition.None;
        dimBtn.onClick.AddListener(ClosePanel);

        box = new GameObject("Box"); box.transform.SetParent(overlay.transform, false);
        UITheme.PanelImage(box, UITheme.Panel, 26);
        UITheme.Shadow(box, new Vector2(0, -10), 0.6f);
        var brt = box.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero; brt.sizeDelta = new Vector2(940, 700);

        Label(box.transform, "PERKS", 50, UITheme.Accent, new Vector2(0.5f, 0.5f),
              new Vector2(0, 282), new Vector2(800, 64), TextAnchor.MiddleCenter);
        var bar = new GameObject("Bar"); bar.transform.SetParent(box.transform, false);
        UITheme.PanelImage(bar, UITheme.Accent, 4);
        var barRt = bar.GetComponent<RectTransform>();
        barRt.anchorMin = barRt.anchorMax = new Vector2(0.5f, 0.5f); barRt.pivot = new Vector2(0.5f, 0.5f);
        barRt.anchoredPosition = new Vector2(0, 244); barRt.sizeDelta = new Vector2(220, 6);

        moneyText = Label(box.transform, "", 26, new Color(1f, 0.86f, 0.3f), new Vector2(0.5f, 0.5f),
                          new Vector2(0, 202), new Vector2(700, 40), TextAnchor.MiddleCenter);

        int n = Perks.LIST.Length;
        lvlT = new Text[n]; costT = new Text[n]; buyBtn = new Button[n]; lvlFill = new RectTransform[n];
        float y = 120f;
        const float rowH = 92f;
        for (int i = 0; i < n; i++)
        {
            var p = Perks.LIST[i];

            // pozadie riadku
            var rowBg = new GameObject("RowBg"); rowBg.transform.SetParent(box.transform, false);
            UITheme.PanelImage(rowBg, UITheme.PanelLight, 14);
            var rbg = rowBg.GetComponent<RectTransform>();
            rbg.anchorMin = rbg.anchorMax = new Vector2(0.5f, 0.5f); rbg.pivot = new Vector2(0.5f, 0.5f);
            rbg.anchoredPosition = new Vector2(0, y); rbg.sizeDelta = new Vector2(852, rowH - 10);

            // názov + popis (vľavo, v rámci boxu)
            Label(box.transform, p.name, 27, UITheme.Text, new Vector2(0.5f, 0.5f),
                  new Vector2(-235, y + 16), new Vector2(370, 34), TextAnchor.MiddleLeft);
            Label(box.transform, p.desc, 17, UITheme.SubText, new Vector2(0.5f, 0.5f),
                  new Vector2(-225, y - 16), new Vector2(400, 28), TextAnchor.MiddleLeft);

            // úroveň: text + progress bar
            lvlT[i] = Label(box.transform, "", 20, new Color(0.9f, 0.93f, 1f), new Vector2(0.5f, 0.5f),
                            new Vector2(150, y + 15), new Vector2(200, 28), TextAnchor.MiddleCenter);
            var barBg = new GameObject("LvlBarBg"); barBg.transform.SetParent(box.transform, false);
            UITheme.PanelImage(barBg, new Color(0.07f, 0.08f, 0.10f, 1f), 6);
            var bbg = barBg.GetComponent<RectTransform>();
            bbg.anchorMin = bbg.anchorMax = new Vector2(0.5f, 0.5f); bbg.pivot = new Vector2(0.5f, 0.5f);
            bbg.anchoredPosition = new Vector2(150, y - 14); bbg.sizeDelta = new Vector2(200, 12);
            var fillGO = new GameObject("LvlFill"); fillGO.transform.SetParent(barBg.transform, false);
            UITheme.PanelImage(fillGO, UITheme.Accent, 6);
            lvlFill[i] = fillGO.GetComponent<RectTransform>();
            lvlFill[i].anchorMin = lvlFill[i].anchorMax = new Vector2(0f, 0.5f); lvlFill[i].pivot = new Vector2(0f, 0.5f);
            lvlFill[i].anchoredPosition = new Vector2(-100, 0); lvlFill[i].sizeDelta = new Vector2(0, 12);

            string pid = p.id;
            buyBtn[i] = Btn(box.transform, "", new Vector2(0.5f, 0.5f), new Vector2(345, y),
                            new Vector2(180, 60), UITheme.Good, 22, () => OnBuy(pid), 12);
            costT[i] = buyBtn[i].GetComponentInChildren<Text>();
            y -= rowH;
        }

        var close = Btn(box.transform, "CLOSE", new Vector2(0.5f, 0.5f), new Vector2(0, -292),
                        new Vector2(340, 66), UITheme.Danger, 26, ClosePanel, 16);
        UITheme.Shadow(close.gameObject, new Vector2(0, -4));

        Refresh();
    }

    void OnBuy(string id)
    {
        if (Perks.TryBuy(id)) { if (SfxManager.Instance != null) SfxManager.Coin(); }
        Refresh();
    }

    void Refresh()
    {
        int money = PlayerInventory.Instance != null ? PlayerInventory.Instance.Money : PlayerPrefs.GetInt("Money", 0);
        if (moneyText != null) moneyText.text = "Money: $" + money;
        for (int i = 0; i < Perks.LIST.Length; i++)
        {
            var p = Perks.LIST[i];
            int lvl = Perks.Level(p.id);
            if (lvlT[i] != null) lvlT[i].text = "Lv " + lvl + " / " + Perks.MAX;
            if (lvlFill[i] != null) lvlFill[i].sizeDelta = new Vector2(200f * Mathf.Clamp01((float)lvl / Mathf.Max(1, Perks.MAX)), 12f);
            bool maxed = !Perks.CanBuy(p.id);
            if (costT[i] != null) costT[i].text = maxed ? "MAX" : "$" + Perks.Cost(p.id);
            if (buyBtn[i] != null)
            {
                var img = buyBtn[i].targetGraphic as Image;
                if (img != null) img.color = maxed ? new Color(0.2f, 0.2f, 0.22f) : UITheme.Good;
                buyBtn[i].interactable = !maxed && money >= Perks.Cost(p.id);
            }
        }
    }

    void OpenPanel()  { if (overlay != null) { overlay.SetActive(true); Refresh(); } }
    void ClosePanel() { if (overlay != null) overlay.SetActive(false); }

    // ---------- HELPERS ----------
    Button Btn(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, Color col, int fs, UnityEngine.Events.UnityAction onClick, int radius)
    {
        var go = new GameObject("Btn"); go.transform.SetParent(parent, false);
        var img = UITheme.PanelImage(go, col, radius);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        UITheme.Hover(btn, col, col);
        var t = new GameObject("T"); t.transform.SetParent(go.transform, false);
        var trt = t.AddComponent<RectTransform>(); trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var txt = t.AddComponent<Text>(); txt.text = text; txt.font = F; txt.fontSize = fs; txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white; txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        if (onClick != null) btn.onClick.AddListener(onClick);
        return btn;
    }

    Text Label(Transform parent, string text, int fs, Color col, Vector2 anchor, Vector2 pos, Vector2 size, TextAnchor align)
    {
        var go = new GameObject("Lbl"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
        var t = go.AddComponent<Text>(); t.text = text; t.font = F; t.fontSize = fs; t.fontStyle = FontStyle.Bold;
        t.color = col; t.alignment = align; t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
