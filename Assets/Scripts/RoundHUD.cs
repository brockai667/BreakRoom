using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// HUD počas kola: zarobené $ za kolo, % zničenia miestnosti a combo
/// s multiplikátorom. Postaví sa sám z kódu len v herných scénach
/// (nie v Hube/MainMenu/Shop/Collection), takže netreba upravovať scény.
public class RoundHUD : MonoBehaviour
{
    static readonly string[] SKIP = { "Hub", "MainMenu", "Shop", "Collection" };

    GameObject canvasGO;
    Text moneyText, pctText, comboText, objectiveText;
    int   lastCombo = 0;
    float comboPop  = 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("RoundHUD");
        DontDestroyOnLoad(go);
        var hud = go.AddComponent<RoundHUD>();
        SceneManager.sceneLoaded += (s, m) => hud.OnScene(s.name);
        hud.OnScene(SceneManager.GetActiveScene().name);
    }

    void OnScene(string scene)
    {
        bool gameplay = System.Array.IndexOf(SKIP, scene) < 0;
        if (!gameplay) { if (canvasGO != null) Destroy(canvasGO); canvasGO = null; return; }
        if (canvasGO == null) Build();
    }

    void Build()
    {
        canvasGO = new GameObject("RoundHUDCanvas");
        canvasGO.transform.SetParent(transform, false);
        var cv = canvasGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 90;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        // bez GraphicRaycaster -> neblokuje hru

        moneyText     = Make("Money",  44, FontStyle.Bold, new Color(1f, 0.86f, 0.3f),
                             new Vector2(0f, 1f), new Vector2(30, -26), new Vector2(560, 60), TextAnchor.UpperLeft);
        pctText       = Make("Pct",    26, FontStyle.Bold, new Color(0.8f, 0.85f, 1f),
                             new Vector2(0f, 1f), new Vector2(32, -86), new Vector2(560, 40), TextAnchor.UpperLeft);
        objectiveText = Make("Objective", 24, FontStyle.Bold, new Color(0.7f, 1f, 0.75f),
                             new Vector2(0f, 1f), new Vector2(32, -124), new Vector2(720, 40), TextAnchor.UpperLeft);
        comboText     = Make("Combo",  72, FontStyle.Bold, new Color(1f, 0.55f, 0.1f),
                             new Vector2(0f, 0.5f), new Vector2(40, 40), new Vector2(560, 110), TextAnchor.MiddleLeft);
    }

    Text Make(string name, int size, FontStyle style, Color col, Vector2 anchor, Vector2 pos, Vector2 sizeD, TextAnchor align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvasGO.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeD;
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.fontStyle = style; t.color = col; t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var sh = go.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.7f);
        sh.effectDistance = new Vector2(2, -2);
        return t;
    }

    void Update()
    {
        if (canvasGO == null) return;
        var gm = GameManager.Instance;
        if (gm == null) { canvasGO.SetActive(false); return; }
        if (!canvasGO.activeSelf) canvasGO.SetActive(true);

        moneyText.text = "$ " + gm.roundMoney;
        pctText.text   = "Zničené: " + Mathf.RoundToInt(gm.DestructionPct() * 100f) + "%";

        // Cieľ kola (ak existuje)
        if (objectiveText != null)
            objectiveText.text = Objectives.Instance != null ? Objectives.Instance.HudText() : "";

        // Combo
        int combo = gm.comboCount;
        if (combo != lastCombo)
        {
            if (combo > lastCombo) comboPop = 1f;   // pulz pri náraste
            lastCombo = combo;
        }
        comboPop = Mathf.MoveTowards(comboPop, 0f, Time.unscaledDeltaTime * 3.5f);

        if (combo >= 2)
        {
            float mult = gm.ComboMultiplier();
            comboText.text = $"COMBO ×{combo}";
            comboText.color = mult >= 2f ? new Color(1f, 0.35f, 0.1f) : new Color(1f, 0.7f, 0.15f);
            comboText.rectTransform.localScale = Vector3.one * (1f + comboPop * 0.35f);
            if (!comboText.gameObject.activeSelf) comboText.gameObject.SetActive(true);
        }
        else if (comboText.gameObject.activeSelf)
        {
            comboText.gameObject.SetActive(false);
        }
    }
}
