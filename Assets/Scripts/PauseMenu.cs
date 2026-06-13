using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// Pauza s modernym code-overlayom: RESUME / RESTART / QUIT TO HUB.
/// Stara scenova PausePanel sa skryje a nahradi sa tymto.
public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;          // stara scenova (skryjeme)
    private GameObject crosshairDot;
    private GameObject overlay;            // novy code-built overlay
    private bool isPaused = false;

    public bool IsPaused => isPaused;

    static Font F => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    void Start()
    {
        if (pausePanel == null) pausePanel = GameObject.Find("PausePanel");
        if (pausePanel != null) pausePanel.SetActive(false);   // stara nech sa neukazuje
        crosshairDot = GameObject.Find("Crosshair");

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) return;
        if (overlay == null) BuildOverlay();
        overlay.SetActive(true);
        if (crosshairDot != null) crosshairDot.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    public void Resume()
    {
        if (overlay != null) overlay.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (crosshairDot != null) crosshairDot.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// QUIT v pauze: ukonci kolo, spocitaj peniaze a chod do hubu.
    public void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.QuitToHub();
        else SceneManager.LoadScene("Hub");
    }

    public void QuitToHub() => GoToMainMenu();

    // ---------- OVERLAY ----------
    void BuildOverlay()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        }

        overlay = new GameObject("PauseOverlay");
        var cv = overlay.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 170;
        var sc = overlay.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        overlay.AddComponent<GraphicRaycaster>();

        var dim = new GameObject("Dim"); dim.transform.SetParent(overlay.transform, false);
        var dimg = dim.AddComponent<Image>(); dimg.color = UITheme.Overlay;
        var drt = dim.GetComponent<RectTransform>();
        drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one; drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;

        var box = new GameObject("Box"); box.transform.SetParent(overlay.transform, false);
        UITheme.PanelImage(box, UITheme.Panel, 24);
        UITheme.Shadow(box, new Vector2(0, -8), 0.55f);
        var brt = box.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero; brt.sizeDelta = new Vector2(560, 460);

        Label(box.transform, "PAUSED", 56, UITheme.Accent, new Vector2(0, 165), new Vector2(500, 70));
        var bar = new GameObject("Bar"); bar.transform.SetParent(box.transform, false);
        UITheme.PanelImage(bar, UITheme.Accent, 4);
        var barRt = bar.GetComponent<RectTransform>();
        barRt.anchorMin = barRt.anchorMax = new Vector2(0.5f, 0.5f); barRt.pivot = new Vector2(0.5f, 0.5f);
        barRt.anchoredPosition = new Vector2(0, 128); barRt.sizeDelta = new Vector2(150, 6);

        Btn(box.transform, "RESUME",      new Vector2(0, 60),   new Vector2(380, 72), UITheme.Good,   26, Resume);
        Btn(box.transform, "RESTART",     new Vector2(0, -25),  new Vector2(380, 72), UITheme.BtnHover, 26, Restart);
        Btn(box.transform, "QUIT TO HUB", new Vector2(0, -110), new Vector2(380, 72), UITheme.Danger, 26, GoToMainMenu);

        overlay.SetActive(false);
    }

    void Btn(Transform parent, string text, Vector2 pos, Vector2 size, Color col, int fs, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn"); go.transform.SetParent(parent, false);
        var img = UITheme.PanelImage(go, col, 14);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        UITheme.Hover(btn, col, col);
        UITheme.Shadow(go, new Vector2(0, -2));
        var t = new GameObject("T"); t.transform.SetParent(go.transform, false);
        var trt = t.AddComponent<RectTransform>(); trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var txt = t.AddComponent<Text>(); txt.text = text; txt.font = F; txt.fontSize = fs; txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
        if (onClick != null) btn.onClick.AddListener(onClick);
    }

    void Label(Transform parent, string text, int fs, Color col, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Lbl"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var t = go.AddComponent<Text>(); t.text = text; t.font = F; t.fontSize = fs; t.fontStyle = FontStyle.Bold;
        t.color = col; t.alignment = TextAnchor.MiddleCenter; t.horizontalOverflow = HorizontalWrapMode.Overflow;
    }
}
