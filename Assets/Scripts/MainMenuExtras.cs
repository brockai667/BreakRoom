using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Do MainMenu prida tlacidlo COLLECTION + najlepsie lifetime statistiky.
/// (SETTINGS tlacidlo prida SettingsMenu.) Self-bootstrapping.
public class MainMenuExtras : MonoBehaviour
{
    GameObject canvasGO;
    static Font F => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("MainMenuExtras");
        DontDestroyOnLoad(go);
        var m = go.AddComponent<MainMenuExtras>();
        SceneManager.sceneLoaded += (s, mode) => m.OnScene(s.name);
        m.OnScene(SceneManager.GetActiveScene().name);
    }

    void OnScene(string scene)
    {
        if (canvasGO != null) { Destroy(canvasGO); canvasGO = null; }
        if (scene != "MainMenu") return;
        Build();
    }

    void Build()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        }

        canvasGO = new GameObject("MainMenuExtrasCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 120;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // COLLECTION button (bottom-center)
        var go = new GameObject("CollectionBtn"); go.transform.SetParent(canvasGO.transform, false);
        var img = UITheme.PanelImage(go, UITheme.PanelLight, 14);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 90); rt.sizeDelta = new Vector2(300, 64);
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img; UITheme.Hover(btn, UITheme.PanelLight, UITheme.PanelLight);
        UITheme.Shadow(go, new Vector2(0, -3));
        btn.onClick.AddListener(() => SceneManager.LoadScene("Collection"));
        Label(go.transform, "COLLECTION", 24, Color.white, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);

        // Best stats (bottom-left)
        int level     = PlayerPrefs.GetInt("Level", 1);
        int smashed   = PlayerPrefs.GetInt("Stat_smashed", 0);
        int bestCombo = PlayerPrefs.GetInt("Stat_bestCombo", 0);
        int cleared   = PlayerPrefs.GetInt("Stat_cleared", 0);
        var statsGO = new GameObject("Stats"); statsGO.transform.SetParent(canvasGO.transform, false);
        var srt = statsGO.AddComponent<RectTransform>();
        srt.anchorMin = srt.anchorMax = new Vector2(0f, 0f); srt.pivot = new Vector2(0f, 0f);
        srt.anchoredPosition = new Vector2(28, 24); srt.sizeDelta = new Vector2(640, 130);
        var t = statsGO.AddComponent<Text>(); t.font = F; t.fontSize = 22; t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.85f, 0.88f, 0.95f); t.alignment = TextAnchor.LowerLeft;
        t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = $"Level {level}\nSmashed: {smashed}\nBest combo: {bestCombo}\nRooms cleared: {cleared}";
        var sh = statsGO.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.7f); sh.effectDistance = new Vector2(2, -2);
    }

    void Label(Transform parent, string text, int fs, Color col, Vector2 aMin, Vector2 aMax, TextAnchor align)
    {
        var go = new GameObject("T"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>(); t.text = text; t.font = F; t.fontSize = fs; t.fontStyle = FontStyle.Bold;
        t.color = col; t.alignment = align; t.horizontalOverflow = HorizontalWrapMode.Overflow;
    }
}
