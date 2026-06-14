using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Kratke hinty pri uplne prvom herskom kole. Zobrazi sa raz (PlayerPrefs flag),
/// pozastavi cas, hrac klikne GOT IT a hra pokracuje. Self-bootstrapping.
public class Tutorial : MonoBehaviour
{
    static readonly string[] SKIP = { "Hub", "MainMenu", "Shop", "Collection" };
    GameObject canvasGO;
    static Font F => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("Tutorial");
        DontDestroyOnLoad(go);
        var t = go.AddComponent<Tutorial>();
        SceneManager.sceneLoaded += (s, m) => t.OnScene(s.name);
        t.OnScene(SceneManager.GetActiveScene().name);
    }

    void OnScene(string scene)
    {
        if (System.Array.IndexOf(SKIP, scene) >= 0) return;
        if (PlayerPrefs.GetInt("TutorialDone", 0) == 1) return;
        Show();
    }

    void Show()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        }

        canvasGO = new GameObject("TutorialCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 180;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var dim = new GameObject("Dim"); dim.transform.SetParent(canvasGO.transform, false);
        var dimg = dim.AddComponent<Image>(); dimg.color = UITheme.Overlay;
        var drt = dim.GetComponent<RectTransform>();
        drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one; drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;

        var box = new GameObject("Box"); box.transform.SetParent(canvasGO.transform, false);
        UITheme.PanelImage(box, UITheme.Panel, 24);
        UITheme.Shadow(box, new Vector2(0, -8), 0.55f);
        var brt = box.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = Vector2.zero; brt.sizeDelta = new Vector2(820, 540);

        Label(box.transform, "HOW TO PLAY", 46, UITheme.Accent, new Vector2(0, 210), new Vector2(760, 64), TextAnchor.MiddleCenter);

        string tips =
            "Left click  -  smash furniture\n\n" +
            "Smash fast in a row  -  COMBO (more money)\n\n" +
            "Do the objective (top-left) for a bonus\n\n" +
            "Grab glowing power-ups that drop\n\n" +
            "ESC / TAB  -  pause";
        var tg = new GameObject("Tips"); tg.transform.SetParent(box.transform, false);
        var trt = tg.AddComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f); trt.pivot = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, 0); trt.sizeDelta = new Vector2(720, 320);
        var tt = tg.AddComponent<Text>(); tt.text = tips; tt.font = F; tt.fontSize = 26; tt.fontStyle = FontStyle.Bold;
        tt.color = new Color(0.9f, 0.92f, 0.98f); tt.alignment = TextAnchor.MiddleCenter;
        tt.horizontalOverflow = HorizontalWrapMode.Overflow; tt.verticalOverflow = VerticalWrapMode.Overflow;

        var go = new GameObject("Got"); go.transform.SetParent(box.transform, false);
        var img = UITheme.PanelImage(go, UITheme.Good, 14);
        var grt = go.GetComponent<RectTransform>();
        grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.5f); grt.pivot = new Vector2(0.5f, 0.5f);
        grt.anchoredPosition = new Vector2(0, -215); grt.sizeDelta = new Vector2(320, 66);
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img; UITheme.Hover(btn, UITheme.Good, UITheme.Good);
        btn.onClick.AddListener(Dismiss);
        Label(go.transform, "GOT IT!", 26, Color.white, Vector2.zero, Vector2.zero);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Dismiss()
    {
        PlayerPrefs.SetInt("TutorialDone", 1); PlayerPrefs.Save();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (canvasGO != null) Destroy(canvasGO);
        canvasGO = null;
    }

    void Label(Transform parent, string text, int fs, Color col, Vector2 pos, Vector2 size, TextAnchor align)
    {
        var go = new GameObject("L"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var t = go.AddComponent<Text>(); t.text = text; t.font = F; t.fontSize = fs; t.fontStyle = FontStyle.Bold;
        t.color = col; t.alignment = align; t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
    }

    // overload pre tlacidlo (stretch)
    void Label(Transform parent, string text, int fs, Color col, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject("L"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>(); t.text = text; t.font = F; t.fontSize = fs; t.fontStyle = FontStyle.Bold;
        t.color = col; t.alignment = TextAnchor.MiddleCenter; t.horizontalOverflow = HorizontalWrapMode.Overflow;
    }
}
