using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Settings: mouse sensitivity, volume, FOV, graphics quality.
/// Self-bootstrapping. Adds a SETTINGS button in MainMenu and Hub that
/// opens a centered panel. Values are saved to PlayerPrefs and applied.
public class SettingsMenu : MonoBehaviour
{
    static readonly string[] SHOW_BTN = { "MainMenu", "Hub" };
    static SettingsMenu inst;

    GameObject canvasGO, overlay, box;
    float sens, vol, fov;
    int   qual;
    Text  sensT, volT, fovT, qualT;

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
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 0.8f);
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

        // Open button: top-right, just under the money line
        MakeButton(canvasGO.transform, "SETTINGS", new Vector2(1, 1), new Vector2(-24, -96),
                   new Vector2(190, 52), new Color(0.22f, 0.24f, 0.3f, 0.95f), 22, OpenPanel);

        BuildPanel();
        overlay.SetActive(false);
    }

    void BuildPanel()
    {
        sens = PlayerPrefs.GetFloat("Sensitivity", 2f);
        vol  = PlayerPrefs.GetFloat("Volume", 0.8f);
        fov  = PlayerPrefs.GetFloat("FOV", 70f);
        qual = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());

        // Fullscreen dim overlay (click outside box = close)
        overlay = new GameObject("SettingsOverlay");
        overlay.transform.SetParent(canvasGO.transform, false);
        var dim = overlay.AddComponent<Image>(); dim.color = new Color(0f, 0f, 0f, 0.65f);
        var ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one; ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
        var dimBtn = overlay.AddComponent<Button>(); dimBtn.targetGraphic = dim;
        dimBtn.onClick.AddListener(ClosePanel);

        // Centered box
        box = new GameObject("Box"); box.transform.SetParent(overlay.transform, false);
        var bImg = box.AddComponent<Image>(); bImg.color = new Color(0.10f, 0.10f, 0.13f, 1f);
        var brt = box.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero; brt.sizeDelta = new Vector2(820, 660);

        Label(box.transform, "SETTINGS", 50, new Color(1f, 0.9f, 0.45f),
              new Vector2(0.5f, 0.5f), new Vector2(0, 260), new Vector2(800, 70), TextAnchor.MiddleCenter);

        sensT = Row("Mouse Sensitivity", 150, () => Adjust(ref sens, -0.25f, 0.5f, 4f, "Sensitivity"),
                                              () => Adjust(ref sens, 0.25f, 0.5f, 4f, "Sensitivity"));
        volT  = Row("Volume", 60, () => Adjust(ref vol, -0.1f, 0f, 1f, "Volume"),
                                  () => Adjust(ref vol, 0.1f, 0f, 1f, "Volume"));
        fovT  = Row("Field of View", -30, () => Adjust(ref fov, -5f, 60f, 100f, "FOV"),
                                          () => Adjust(ref fov, 5f, 60f, 100f, "FOV"));
        qualT = Row("Graphics Quality", -120, () => CycleQuality(-1), () => CycleQuality(1));

        MakeButton(box.transform, "CLOSE", new Vector2(0.5f, 0.5f), new Vector2(0, -250),
                   new Vector2(320, 66), new Color(0.5f, 0.16f, 0.06f), 26, ClosePanel);

        RefreshValues();
    }

    // Row inside the box (label left, [-] value [+] right), y relative to box center
    Text Row(string label, float y, UnityEngine.Events.UnityAction minus, UnityEngine.Events.UnityAction plus)
    {
        Label(box.transform, label, 28, Color.white, new Vector2(0.5f, 0.5f),
              new Vector2(-220, y), new Vector2(420, 50), TextAnchor.MiddleLeft);
        MakeButton(box.transform, "-", new Vector2(0.5f, 0.5f), new Vector2(120, y),
                   new Vector2(58, 52), new Color(0.25f, 0.28f, 0.34f), 30, minus);
        var val = Label(box.transform, "", 26, new Color(0.85f, 0.9f, 1f), new Vector2(0.5f, 0.5f),
                        new Vector2(265, y), new Vector2(170, 50), TextAnchor.MiddleCenter);
        MakeButton(box.transform, "+", new Vector2(0.5f, 0.5f), new Vector2(330, y),
                   new Vector2(58, 52), new Color(0.25f, 0.28f, 0.34f), 30, plus);
        return val;
    }

    void Adjust(ref float v, float delta, float min, float max, string key)
    {
        v = Mathf.Clamp(Mathf.Round((v + delta) * 100f) / 100f, min, max);
        PlayerPrefs.SetFloat(key, v); PlayerPrefs.Save();
        if (key == "Volume") AudioListener.volume = v;
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
        if (volT  != null) volT.text  = Mathf.RoundToInt(vol * 100f) + "%";
        if (fovT  != null) fovT.text  = Mathf.RoundToInt(fov).ToString();
        if (qualT != null) qualT.text = QualitySettings.names[Mathf.Clamp(qual, 0, QualitySettings.names.Length - 1)];
    }

    void OpenPanel()  { if (overlay != null) overlay.SetActive(true); }
    void ClosePanel() { if (overlay != null) overlay.SetActive(false); }

    // ---------- HELPERS ----------
    Button MakeButton(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, Color col, int fs, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn"); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = col;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
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
