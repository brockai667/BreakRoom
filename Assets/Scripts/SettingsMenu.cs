using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Nastavenia: citlivost mysi, hlasitost, FOV, kvalita grafiky.
/// Self-bootstrapping. V MainMenu a Hube prida tlacidlo NASTAVENIA,
/// ktore otvori panel. Hodnoty sa ukladaju do PlayerPrefs a aplikuju.
public class SettingsMenu : MonoBehaviour
{
    static readonly string[] SHOW_GEAR = { "MainMenu", "Hub" };
    static SettingsMenu inst;

    GameObject canvasGO, panel;
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

    /// Aplikuj globalne nastavenia (hlasitost, kvalita) - vola sa pri kazdej scene.
    public static void ApplyGlobal()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 0.8f);
        int q = PlayerPrefs.GetInt("Quality", -1);
        if (q >= 0 && q < QualitySettings.names.Length) QualitySettings.SetQualityLevel(q, true);
    }

    void OnScene(string scene)
    {
        if (canvasGO != null) { Destroy(canvasGO); canvasGO = null; panel = null; }
        if (System.Array.IndexOf(SHOW_GEAR, scene) < 0) return;
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
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 150;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Tlacidlo NASTAVENIA vpravo hore
        MakeButton(canvasGO.transform, "NASTAVENIA", new Vector2(1, 0), new Vector2(-20, 20),
                              new Vector2(200, 54), new Color(0.2f, 0.2f, 0.24f, 0.92f), 20, OpenPanel);

        BuildPanel();
        panel.SetActive(false);
    }

    void BuildPanel()
    {
        sens = PlayerPrefs.GetFloat("Sensitivity", 2f);
        vol  = PlayerPrefs.GetFloat("Volume", 0.8f);
        fov  = PlayerPrefs.GetFloat("FOV", 70f);
        qual = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());

        panel = new GameObject("SettingsPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var bg = panel.AddComponent<Image>(); bg.color = new Color(0.05f, 0.05f, 0.07f, 0.95f);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

        Label(panel.transform, "NASTAVENIA", 46, new Color(1f, 0.9f, 0.45f),
              new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(800, 70), TextAnchor.MiddleCenter);

        float y = -180f;
        sensT = Row("Citlivost mysi", y, () => Adjust(ref sens, -0.25f, 0.5f, 4f, "Sensitivity"),
                                          () => Adjust(ref sens, 0.25f, 0.5f, 4f, "Sensitivity")); y -= 95f;
        volT  = Row("Hlasitost", y, () => Adjust(ref vol, -0.1f, 0f, 1f, "Volume"),
                                     () => Adjust(ref vol, 0.1f, 0f, 1f, "Volume")); y -= 95f;
        fovT  = Row("Zorne pole (FOV)", y, () => Adjust(ref fov, -5f, 60f, 100f, "FOV"),
                                           () => Adjust(ref fov, 5f, 60f, 100f, "FOV")); y -= 95f;
        qualT = Row("Kvalita grafiky", y, () => CycleQuality(-1), () => CycleQuality(1)); y -= 120f;

        MakeButton(panel.transform, "ZAVRIET", new Vector2(0.5f, 0.5f), new Vector2(0, y),
                   new Vector2(300, 64), new Color(0.45f, 0.14f, 0.05f), 24, ClosePanel);

        RefreshValues();
    }

    // Riadok: label vlavo, [-] hodnota [+] vpravo
    Text Row(string label, float y, UnityEngine.Events.UnityAction minus, UnityEngine.Events.UnityAction plus)
    {
        Label(panel.transform, label, 26, Color.white, new Vector2(0.5f, 0.5f),
              new Vector2(-280, y), new Vector2(420, 50), TextAnchor.MiddleLeft);
        MakeButton(panel.transform, "-", new Vector2(0.5f, 0.5f), new Vector2(60, y),
                   new Vector2(56, 50), new Color(0.25f, 0.28f, 0.32f), 28, minus);
        var val = Label(panel.transform, "", 24, new Color(0.85f, 0.9f, 1f), new Vector2(0.5f, 0.5f),
                        new Vector2(210, y), new Vector2(180, 50), TextAnchor.MiddleCenter);
        MakeButton(panel.transform, "+", new Vector2(0.5f, 0.5f), new Vector2(360, y),
                   new Vector2(56, 50), new Color(0.25f, 0.28f, 0.32f), 28, plus);
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

    void OpenPanel()  { if (panel != null) panel.SetActive(true); }
    void ClosePanel() { if (panel != null) panel.SetActive(false); }

    // ---------- HELPERY ----------
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
