using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Panel achievementov v Hube: zoznam vsetkych + progres / odomknute.
/// Self-bootstrapping, moderny vzhlad (UITheme).
public class AchievementsMenu : MonoBehaviour
{
    static AchievementsMenu inst;
    GameObject canvasGO, overlay, box;

    static Font F => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("AchievementsMenu");
        DontDestroyOnLoad(go);
        inst = go.AddComponent<AchievementsMenu>();
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

        canvasGO = new GameObject("AchCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 156;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var open = Btn(canvasGO.transform, "ACHIEVEMENTS", new Vector2(1, 1), new Vector2(-24, -224),
                       new Vector2(196, 56), UITheme.PanelLight, 18, OpenPanel, 14);
        UITheme.Shadow(open.gameObject, new Vector2(0, -3));

        BuildPanel();
        overlay.SetActive(false);
    }

    void BuildPanel()
    {
        overlay = new GameObject("AchOverlay");
        overlay.transform.SetParent(canvasGO.transform, false);
        var dim = overlay.AddComponent<Image>(); dim.color = UITheme.Overlay;
        var ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one; ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
        var dimBtn = overlay.AddComponent<Button>(); dimBtn.targetGraphic = dim;
        dimBtn.transition = Selectable.Transition.None; dimBtn.onClick.AddListener(ClosePanel);

        box = new GameObject("Box"); box.transform.SetParent(overlay.transform, false);
        UITheme.PanelImage(box, UITheme.Panel, 24);
        UITheme.Shadow(box, new Vector2(0, -8), 0.55f);
        var brt = box.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero; brt.sizeDelta = new Vector2(960, 760);

        Label(box.transform, "ACHIEVEMENTS", 48, UITheme.Accent, new Vector2(0.5f, 0.5f),
              new Vector2(0, 318), new Vector2(900, 64), TextAnchor.MiddleCenter);

        int smashed   = PlayerPrefs.GetInt("Stat_smashed", 0);
        int bestCombo = PlayerPrefs.GetInt("Stat_bestCombo", 0);
        int cleared   = PlayerPrefs.GetInt("Stat_cleared", 0);

        float y = 240f;
        foreach (var d in Achievements.DEFS)
        {
            bool got = PlayerPrefs.GetInt("Ach_" + d.id, 0) == 1;
            int have = d.type == "smashed" ? smashed : d.type == "combo" ? bestCombo : cleared;

            Label(box.transform, (got ? "[X] " : "[ ] ") + d.name, 26, got ? UITheme.Good : UITheme.SubText,
                  new Vector2(0.5f, 0.5f), new Vector2(-410, y), new Vector2(560, 44), TextAnchor.MiddleLeft);
            string status = got ? ("UNLOCKED   +$" + d.reward) : (Mathf.Min(have, d.need) + " / " + d.need);
            Label(box.transform, status, 24, got ? UITheme.Good : new Color(0.85f, 0.9f, 1f),
                  new Vector2(0.5f, 0.5f), new Vector2(180, y), new Vector2(300, 44), TextAnchor.MiddleRight);
            y -= 60f;
        }

        var close = Btn(box.transform, "CLOSE", new Vector2(0.5f, 0.5f), new Vector2(0, -322),
                        new Vector2(320, 64), UITheme.Danger, 26, ClosePanel, 16);
        UITheme.Shadow(close.gameObject, new Vector2(0, -3));
    }

    void OpenPanel()  { if (overlay != null) overlay.SetActive(true); }
    void ClosePanel() { if (overlay != null) overlay.SetActive(false); }

    // ---------- HELPERS ----------
    Button Btn(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, Color col, int fs, UnityEngine.Events.UnityAction onClick, int radius)
    {
        var go = new GameObject("Btn"); go.transform.SetParent(parent, false);
        var img = UITheme.PanelImage(go, col, radius);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img; UITheme.Hover(btn, col, col);
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
