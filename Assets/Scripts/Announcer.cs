using UnityEngine;
using UnityEngine.UI;

/// Hlásič: veľké štylizované hlášky cez obrazovku (SMASH!, BOOM!, míľniky).
/// Vlastný overlay canvas sa vytvorí za behu, takže netreba upravovať scény.
/// Neblokuje UI (žiadny raycaster).
public class Announcer : MonoBehaviour
{
    public static Announcer Instance;

    Text label;
    RectTransform rt;
    float t;            // zostávajúci čas zobrazenia
    float age;          // čas od zobrazenia (na "pop" animáciu)
    bool  showing;

    static readonly string[] SMASH = { "SMASH!", "TRESK!", "BUM!", "RACHOT!", "NA ŠROT!" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("Announcer");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<Announcer>();
        Instance.Build();
    }

    void Build()
    {
        var cvGO = new GameObject("AnnouncerCanvas");
        cvGO.transform.SetParent(transform, false);
        var cv = cvGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 200;                       // nad hrou aj UI
        var scaler = cvGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        // bez GraphicRaycaster -> neblokuje klikanie

        var tGO = new GameObject("Callout");
        tGO.transform.SetParent(cvGO.transform, false);
        label = tGO.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 90;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        label.color = new Color(1f, 0.85f, 0.1f);
        var sh = tGO.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.7f);
        sh.effectDistance = new Vector2(3, -3);

        rt = label.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.72f);
        rt.anchorMax = new Vector2(0.5f, 0.72f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1600, 200);

        label.text = "";
    }

    /// Náhodná "smash" hláška (volá sa občas pri rozbití).
    public static void Smash()
    {
        if (Instance != null) Instance.Display(SMASH[Random.Range(0, SMASH.Length)], false);
    }

    public static void Show(string text, bool strong = false)
    {
        if (Instance != null) Instance.Display(text, strong);
    }

    void Display(string txt, bool strong)
    {
        if (label == null) return;
        label.text = txt;
        label.color = strong ? new Color(1f, 0.55f, 0.1f) : new Color(1f, 0.88f, 0.2f);
        label.fontSize = strong ? 110 : 80;
        t = strong ? 1.3f : 0.85f;
        age = 0f;
        showing = true;
        SfxManager.Sting();
    }

    void Update()
    {
        if (!showing) return;
        t   -= Time.unscaledDeltaTime;
        age += Time.unscaledDeltaTime;

        // "pop" pri nábehu + jemné doznenie
        float pop = 1f + Mathf.Exp(-age * 9f) * 0.5f;
        rt.localScale = Vector3.one * pop;

        var c = label.color;
        c.a = Mathf.Clamp01(t / 0.35f);              // posledných 0.35s fade out
        label.color = c;

        if (t <= 0f) { showing = false; label.text = ""; }
    }
}
