using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Settings: mouse sensitivity, volume, FOV, graphics quality.
/// Modern look via UITheme (rounded panel, accent, hover, full dim overlay).
public class SettingsMenu : MonoBehaviour
{
    static readonly string[] SHOW_BTN = { "MainMenu", "Hub" };
    static SettingsMenu inst;

    GameObject canvasGO, overlay, box;
    float sens, sfx, music, fov;
    int   qual;
    Text  sensT, sfxT, musT, fovT, qualT;

    static Font F => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("SettingsMenu");
        DontDestroyOnLoad(go);
        inst = go.AddComponent<SettingsMenu>();
        ApplyGlobal();
        SceneManager.sceneLoaded += (s, m) => { ApplyGlobal(); if (inst != null) inst.OnScene(s.name); };
        inst.OnScene(SceneManager.GetActiveScene().name);
    }

    public static void ApplyGlobal()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("SfxVolume", 0.8f);
        int q = PlayerPrefs.GetInt("Quality", -1);
        if (q >= 0 && q < QualitySettings.names.Length) QualitySettings.SetQualityLevel(q, true);
    }

    void OnScene(string scene)
    {
        if (canvasGO != null) { Destroy(canvasGO); canvasGO = null; overlay = null; box = null; }
        if (System.Array.IndexOf(SHOW_BTN, scene) < 0) return;
        Build();
    }

    void Build()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        }

        canvasGO = new GameObject("SettingsCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 160;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Open button top-right, under the money line
        var open = Btn(canvasGO.transform, "SETTINGS", new Vector2(1, 1), new Vector2(-24, -96),
                       new Vector2(196, 56), UITheme.PanelLight, 22, OpenPanel, 14);
        UITheme.Shadow(open.gameObject, new Vector2(0, -3));

        BuildPanel();
        overlay.SetActive(false);
    }

    void BuildPanel()
    {
        sens  = PlayerPrefs.GetFloat("Sensitivity", 2f);
        sfx   = PlayerPrefs.GetFloat("SfxVolume", 0.8f);
        music = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        fov  = PlayerPrefs.GetFloat("FOV", 70f);
        qual = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());

        // Fullscreen dim overlay (click outside box = close)
        overlay = new GameObject("SettingsOverlay");
        overlay.transform.SetParent(canvasGO.transform, false);
        var dim = overlay.AddComponent<Image>(); dim.color = UITheme.Overlay;
        var ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one; ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
        var dimBtn = overlay.AddComponent<Button>(); dimBtn.targetGraphic = dim;
        dimBtn.transition = Selectable.Transition.None;
        dimBtn.onClick.AddListener(ClosePanel);

        // Centered rounded box
        box = new GameObject("Box"); box.transform.SetParent(overlay.transform, false);
        UITheme.PanelImage(box, UITheme.Panel, 24);
        UITheme.Shadow(box, new Vector2(0, -8), 0.55f);
        var brt = box.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero; brt.sizeDelta = new Vector2(840, 790);

        // Title + accent underline
        Label(box.transform, "SETTINGS", 52, UITheme.Accent,
              new Vector2(0.5f, 0.5f), new Vector2(0, 322), new Vector2(800, 70), TextAnchor.MiddleCenter);
        var bar = new GameObject("Bar"); bar.transform.SetParent(box.transform, false);
        UITheme.PanelImage(bar, UITheme.Accent, 4);
        var barRt = bar.GetComponent<RectTransform>();
        barRt.anchorMin = barRt.anchorMax = new Vector2(0.5f, 0.5f); barRt.pivot = new Vector2(0.5f, 0.5f);
        barRt.anchoredPosition = new Vector2(0, 282); barRt.sizeDelta = new Vector2(220, 6);

        sensT = Row("Mouse Sensitivity", 192, () => Adjust(ref sens, -0.25f, 0.5f, 4f, "Sensitivity"),
                                              () => Adjust(ref sens, 0.25f, 0.5f, 4f, "Sensitivity"));
        sfxT  = Row("SFX Volume", 112, () => Adjust(ref sfx, -0.1f, 0f, 1f, "SfxVolume"),
                                       () => Adjust(ref sfx, 0.1f, 0f, 1f, "SfxVolume"));
        musT  = Row("Music Volume", 32, () => Adjust(ref music, -0.1f, 0f, 1f, "MusicVolume"),
                                        () => Adjust(ref music, 0.1f, 0f, 1f, "MusicVolume"));
        fovT  = Row("Field of View", -48, () => Adjust(ref fov, -5f, 60f, 100f, "FOV"),
                                          () => Adjust(ref fov, 5f, 60f, 100f, "FOV"));
        qualT = Row("Graphics Quality", -128, () => CycleQuality(-1), () => CycleQuality(1));

        var close = Btn(box.transform, "CLOSE", new Vector2(0.5f, 0.5f), new Vector2(0, -312),
                        new Vector2(340, 68), UITheme.Danger, 26, ClosePanel, 16);
        UITheme.Shadow(close.gameObject, new Vector2(0, -3));

        RefreshValues();
    }

    Text Row(string label, float y, UnityEngine.Events.UnityAction minus, UnityEngine.Events.UnityAction plus)
    {
        Label(box.transform, label, 28, UITheme.Text, new Vector2(0.5f, 0.5f),
              new Vector2(-225, y), new Vector2(420, 50), TextAnchor.MiddleLeft);
        Btn(box.transform, "-", new Vector2(0.5f, 0.5f), new Vector2(120, y),
            new Vector2(58, 54), UITheme.BtnNormal, 32, minus, 12);
        var val = Label(box.transform, "", 26, new Color(0.85f, 0.9f, 1f), new Vector2(0.5f, 0.5f),
                        new Vector2(262, y), new Vector2(170, 50), TextAnchor.MiddleCenter);
        Btn(box.transform, "+", new Vector2(0.5f, 0.5f), new Vector2(328, y),
            new Vector2(58, 54), UITheme.BtnNormal, 32, plus, 12);
        return val;
    }

    void Adjust(ref float v, float delta, float min, float max, string key)
    {
        v = Mathf.Clamp(Mathf.Round((v + delta) * 100f) / 100f, min, max);
        PlayerPrefs.SetFloat(key, v); PlayerPrefs.Save();
        if (key == "SfxVolume") AudioListener.volume = v;
        if (key == "FOV") { var c = Camera.main; if (c != null) c.fieldOfView = v; }
        RefreshValues();
    }

    void CycleQuality(int dir)
    {
        int n = QualitySettings.names.Length;
        qual = (qual + dir + n) % n;
        QualitySettings.SetQualityLevel(qual, true);
        PlayerPrefs.SetInt("Quality", qual); PlayerPrefs.Save();
        RefreshValues();
    }

    void RefreshValues()
    {
        if (sensT != null) sensT.text = sens.ToString("0.00");
        if (sfxT  != null) sfxT.text  = Mathf.RoundToInt(sfx * 100f) + "%";
        if (musT  != null) musT.text  = Mathf.RoundToInt(music * 100f) + "%";
        if (fovT  != null) fovT.text  = Mathf.RoundToInt(fov).ToString();
        if (qualT != null) qualT.text = QualitySettings.names[Mathf.Clamp(qual, 0, QualitySettings.names.Length - 1)];
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
