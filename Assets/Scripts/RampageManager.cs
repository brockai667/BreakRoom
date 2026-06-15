using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// RAMPAGE: rozbíjaním plníš RAGE meter. Keď je plný, stlač [R] a spustíš
/// niekoľko sekúnd šialenstva — každý úder vyšle rázovú vlnu (reťazové ničenie
/// okolia), 3× damage, 2× peniaze, červený nádych obrazovky a otrasy.
/// Self-bootstrapping, funguje vo všetkých herných mapách.
public class RampageManager : MonoBehaviour
{
    public static RampageManager Instance;
    static readonly string[] SKIP = { "Hub", "MainMenu", "Shop", "Collection" };

    const float MAX = 100f;
    const float DURATION = 8f;
    const float GAIN_PER_BREAK = 4.0f;
    const float SHOCK_RADIUS = 2.4f;

    float rage;
    bool  active;
    float activeTimer;
    bool  inShock;
    float shakeTick;
    float shownFrac;   // plynulo dobiehajúca výplň baru

    GameObject canvasGO;
    RectTransform fillRect;
    Image fillImg, tintImg;
    Text  label;
    const float BAR_W = 540f;

    static Font F => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("RampageManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<RampageManager>();
        SceneManager.sceneLoaded += (s, m) => { if (Instance != null) Instance.OnScene(s.name); };
        Instance.OnScene(SceneManager.GetActiveScene().name);
    }

    void OnScene(string scene)
    {
        active = false; rage = 0f; inShock = false;
        if (canvasGO != null) { Destroy(canvasGO); canvasGO = null; }
        if (System.Array.IndexOf(SKIP, scene) >= 0) return;
        Build();
    }

    // ---------- VEREJNÉ API ----------
    public static float DamageMult() => (Instance != null && Instance.active) ? 3f : 1f;
    public static float MoneyMult()  => (Instance != null && Instance.active) ? 2f : 1f;

    /// Volá GameManager.AwardBreak pri každom rozbití.
    public void RegisterBreak(Vector3 pos)
    {
        if (active)
        {
            if (!inShock) Shockwave(pos);
        }
        else
        {
            rage = Mathf.Min(MAX, rage + GAIN_PER_BREAK);
        }
    }

    void Shockwave(Vector3 center)
    {
        inShock = true;
        Fx.Dust(center, new Color(1f, 0.5f, 0.15f));
        CameraShaker.Shake(0.12f, 0.18f);

        foreach (var col in Physics.OverlapSphere(center, SHOCK_RADIUS))
        {
            if (col == null) continue;
            var b = col.GetComponent<Breakable>();
            if (b == null) continue;
            int orig = b.damage;
            b.damage = 999;
            b.Hit(b.transform.position, (b.transform.position - center).normalized);
            if (b != null) b.damage = orig;
        }
        inShock = false;
    }

    // ---------- BEH ----------
    void Update()
    {
        bool roundOk = GameManager.Instance == null || GameManager.Instance.roundActive;

        if (active)
        {
            activeTimer -= Time.deltaTime;

            shakeTick -= Time.deltaTime;
            if (shakeTick <= 0f) { CameraShaker.Shake(0.15f, 0.16f); shakeTick = 0.45f; }

            if (activeTimer <= 0f) End();
        }
        else if (roundOk && rage >= MAX)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame) Activate();
        }

        UpdateUI();
    }

    void Activate()
    {
        active = true; activeTimer = DURATION; rage = 0f; shakeTick = 0f;
        if (Announcer.Instance != null) Announcer.Show("R A M P A G E !", true);
        if (SfxManager.Instance != null) SfxManager.Sting();
        CameraShaker.Shake(0.4f, 0.6f);
    }

    void End()
    {
        active = false; rage = 0f;
    }

    // ---------- UI ----------
    void Build()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        }

        canvasGO = new GameObject("RampageCanvas");
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 140;
        var sc = canvasGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // fullscreen červený nádych (počas RAMPAGE)
        var tint = new GameObject("Tint"); tint.transform.SetParent(canvasGO.transform, false);
        tintImg = tint.AddComponent<Image>(); tintImg.color = new Color(1f, 0.15f, 0.05f, 0f);
        tintImg.raycastTarget = false;
        var trt = tint.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        // pozadie baru (dole v strede)
        var bg = new GameObject("RageBg"); bg.transform.SetParent(canvasGO.transform, false);
        var bgImg = UITheme.PanelImage(bg, new Color(0.08f, 0.08f, 0.10f, 0.85f), 8);
        bgImg.raycastTarget = false;
        var brt = bg.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f); brt.pivot = new Vector2(0.5f, 0f);
        brt.anchoredPosition = new Vector2(0, 64); brt.sizeDelta = new Vector2(BAR_W + 8, 30);

        // výplň (Filled — plynulé plnenie bez artefaktov)
        var fill = new GameObject("RageFill"); fill.transform.SetParent(bg.transform, false);
        fillImg = UITheme.PanelImage(fill, new Color(1f, 0.5f, 0.12f), 8);
        fillImg.raycastTarget = false;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 0f;
        fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.5f); fillRect.anchorMax = new Vector2(0f, 0.5f); fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = new Vector2(-BAR_W / 2f, 0f); fillRect.sizeDelta = new Vector2(BAR_W, 22f);

        // popis nad barom
        var lg = new GameObject("RageLabel"); lg.transform.SetParent(canvasGO.transform, false);
        var lrt = lg.AddComponent<RectTransform>();
        lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0f); lrt.pivot = new Vector2(0.5f, 0f);
        lrt.anchoredPosition = new Vector2(0, 96); lrt.sizeDelta = new Vector2(640, 36);
        label = lg.AddComponent<Text>(); label.font = F; label.fontSize = 22; label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter; label.color = new Color(1f, 0.8f, 0.4f);
        label.horizontalOverflow = HorizontalWrapMode.Overflow; label.verticalOverflow = VerticalWrapMode.Overflow;
        var sh = lg.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.7f); sh.effectDistance = new Vector2(2, -2);
    }

    void UpdateUI()
    {
        if (canvasGO == null) return;

        // skry počas konca kola
        bool show = GameManager.Instance == null || GameManager.Instance.roundActive;
        canvasGO.SetActive(show);
        if (!show) return;

        float frac = active ? Mathf.Clamp01(activeTimer / DURATION) : Mathf.Clamp01(rage / MAX);
        shownFrac = Mathf.MoveTowards(shownFrac, frac, Time.unscaledDeltaTime * (active ? 0.6f : 2.0f));
        if (fillImg != null) fillImg.fillAmount = shownFrac;

        if (active)
        {
            float p = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 12f);
            if (fillImg != null) fillImg.color = Color.Lerp(new Color(1f, 0.85f, 0.1f), new Color(1f, 0.2f, 0.1f), p);
            if (tintImg != null) tintImg.color = new Color(1f, 0.15f, 0.05f, 0.10f + 0.10f * p);
            if (label != null) { label.text = "R A M P A G E !"; label.color = Color.Lerp(new Color(1f,0.9f,0.4f), new Color(1f,0.3f,0.2f), p); }
        }
        else
        {
            if (tintImg != null) tintImg.color = new Color(1f, 0.15f, 0.05f, 0f);
            if (rage >= MAX)
            {
                float p = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f);
                if (fillImg != null) fillImg.color = Color.Lerp(new Color(1f, 0.4f, 0.1f), new Color(1f, 0.85f, 0.2f), p);
                if (label != null) { label.text = "PRESS  [R]  —  RAMPAGE!"; label.color = Color.Lerp(new Color(1f,0.6f,0.2f), Color.white, p); }
            }
            else
            {
                if (fillImg != null) fillImg.color = new Color(1f, 0.5f, 0.12f);
                if (label != null) { label.text = "RAGE"; label.color = new Color(0.85f, 0.7f, 0.5f); }
            }
        }
    }
}
