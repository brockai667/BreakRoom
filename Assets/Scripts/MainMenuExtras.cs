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

        // COLLECTION button (pravý stĺpec, medzi PLAY a QUIT) — jednotný prémiový štýl
        var go = new GameObject("CollectionBtn"); go.transform.SetParent(canvasGO.transform, false);
        var btnCol = new Color(0.13f, 0.14f, 0.18f, 0.97f);
        var img = UITheme.PanelImage(go, btnCol, 18);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(540f, 10f); rt.sizeDelta = new Vector2(460f, 90f);
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img; UITheme.Hover(btn, btnCol, btnCol);
        UITheme.Shadow(go, new Vector2(0, -4), 0.5f);
        btn.onClick.AddListener(() => SceneManager.LoadScene("Collection"));
        Label(go.transform, "COLLECTION", 32, new Color(1f, 0.85f, 0.1f), Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);

        // oranžový spodný akcent (ako PLAY/QUIT)
        var acc = new GameObject("Accent"); acc.transform.SetParent(go.transform, false);
        var ai = acc.AddComponent<Image>(); ai.sprite = UITheme.Rounded(4); ai.type = Image.Type.Sliced;
        ai.color = UITheme.Accent; ai.raycastTarget = false;
        var ar = acc.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.08f, 0f); ar.anchorMax = new Vector2(0.92f, 0f); ar.pivot = new Vector2(0.5f, 0f);
        ar.sizeDelta = new Vector2(0, 6); ar.anchoredPosition = new Vector2(0, 9);

        // Best stats (bottom-left)
        int level     = PlayerPrefs.GetInt("Level", 1);
        int smashed   = PlayerPrefs.GetInt("Stat_smashed", 0);
        int bestCombo = PlayerPrefs.GetInt("Stat_bestCombo", 0);
        int cleared   = PlayerPrefs.GetInt("Stat_cleared", 0);
        int bestRound = Mathf.Max(PlayerPrefs.GetInt("Best_Obyvacka", 0),
                        Mathf.Max(PlayerPrefs.GetInt("Best_Garage", 0),
                                  PlayerPrefs.GetInt("Best_Kitchen", 0)));
        var statsGO = new GameObject("Stats"); statsGO.transform.SetParent(canvasGO.transform, false);
        var srt = statsGO.AddComponent<RectTransform>();
        srt.anchorMin = srt.anchorMax = new Vector2(0f, 0f); srt.pivot = new Vector2(0f, 0f);
        srt.anchoredPosition = new Vector2(28, 24); srt.sizeDelta = new Vector2(640, 160);
        var t = statsGO.AddComponent<Text>(); t.font = F; t.fontSize = 22; t.fontStyle = FontStyle.Bold;
        t.color = new Color(0.85f, 0.88f, 0.95f); t.alignment = TextAnchor.LowerLeft;
        t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = $"Level {level}\nSmashed: {smashed}\nBest combo: {bestCombo}\nRooms cleared: {cleared}\nBest round: ${bestRound}";
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
