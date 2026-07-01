using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// XP/level systém hráča. Self-bootstrapping (rovnaký princíp ako RoundHUD) —
/// jedna trvalá inštancia (DontDestroyOnLoad) si sama postaví XP HUD (level,
/// XP text, bar, level-up flash) a zobrazuje ho len v herných scénach, nie
/// v Hube/MainMenu/Shop/Collection. Používame len Legacy UI Text - bez TMP závislosti.
public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    static readonly string[] SKIP = { "Hub", "MainMenu", "Shop", "Collection" };

    [Header("UI References (Legacy Text)")]
    public Image xpBarFill;
    // TMP polia odstraňujeme - LegacyXPUI sa stará o text

    [Header("State")]
    public int currentLevel = 1;
    public int currentXP = 0;

    GameObject hudGO;

    // Trvalý level (na odomykanie máp) - čítateľný aj bez inštancie
    public static int SavedLevel => PlayerPrefs.GetInt("Level", 1);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("XPManager");
        DontDestroyOnLoad(go);
        var xp = go.AddComponent<XPManager>();
        SceneManager.sceneLoaded += (s, m) => xp.OnScene(s.name);
        xp.OnScene(SceneManager.GetActiveScene().name);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Načítaj trvalý postup
        currentLevel = PlayerPrefs.GetInt("Level", 1);
        currentXP    = PlayerPrefs.GetInt("XP", 0);
    }

    // Zapne/vypne HUD podľa toho, či sme v hernej scéne (nie v menu/hube/shope).
    void OnScene(string scene)
    {
        bool gameplay = System.Array.IndexOf(SKIP, scene) < 0;
        if (!gameplay) { if (hudGO != null) hudGO.SetActive(false); return; }
        if (hudGO == null) BuildUI();
        hudGO.SetActive(true);
        UpdateUI();
    }

    // Postaví XP HUD (level text, XP text, bar, level-up flash) dole vpravo.
    void BuildUI()
    {
        hudGO = new GameObject("XP_HUD_Canvas");
        hudGO.transform.SetParent(transform, false);
        var cv = hudGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 50;
        hudGO.AddComponent<CanvasScaler>();
        hudGO.AddComponent<GraphicRaycaster>();

        var panelGO = new GameObject("XP_HUD"); panelGO.transform.SetParent(hudGO.transform, false);
        var panelR = panelGO.AddComponent<RectTransform>();
        panelR.anchorMin = new Vector2(1, 0); panelR.anchorMax = new Vector2(1, 0); panelR.pivot = new Vector2(1, 0);
        panelR.anchoredPosition = new Vector2(-20, 20); panelR.sizeDelta = new Vector2(340, 95);
        panelGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.65f);

        var lvlGO = new GameObject("LevelText"); lvlGO.transform.SetParent(panelGO.transform, false);
        var lvlR = lvlGO.AddComponent<RectTransform>(); lvlR.anchorMin = Vector2.zero; lvlR.anchorMax = Vector2.one;
        lvlR.offsetMin = new Vector2(12, 55); lvlR.offsetMax = new Vector2(-12, -5);
        var lvlT = lvlGO.AddComponent<Text>(); lvlT.text = "LVL 1"; lvlT.fontSize = 28;
        lvlT.fontStyle = FontStyle.Bold; lvlT.color = new Color(1f, 0.85f, 0.1f); lvlT.alignment = TextAnchor.MiddleLeft;
        lvlT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var xpGO = new GameObject("XPText"); xpGO.transform.SetParent(panelGO.transform, false);
        var xpR = xpGO.AddComponent<RectTransform>(); xpR.anchorMin = Vector2.zero; xpR.anchorMax = Vector2.one;
        xpR.offsetMin = new Vector2(12, 33); xpR.offsetMax = new Vector2(-12, -33);
        var xpT = xpGO.AddComponent<Text>(); xpT.text = "0 / 80 XP"; xpT.fontSize = 16;
        xpT.color = Color.white; xpT.alignment = TextAnchor.MiddleLeft;
        xpT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var barBgGO = new GameObject("XP_BarBG"); barBgGO.transform.SetParent(panelGO.transform, false);
        var barBgR = barBgGO.AddComponent<RectTransform>();
        barBgR.anchorMin = new Vector2(0, 0); barBgR.anchorMax = new Vector2(1, 0); barBgR.pivot = new Vector2(0.5f, 0);
        barBgR.offsetMin = new Vector2(12, 8); barBgR.offsetMax = new Vector2(-12, 26);
        barBgGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

        var barFillGO = new GameObject("XP_BarFill"); barFillGO.transform.SetParent(barBgGO.transform, false);
        var barFillR = barFillGO.AddComponent<RectTransform>();
        barFillR.anchorMin = Vector2.zero; barFillR.anchorMax = Vector2.one; barFillR.offsetMin = barFillR.offsetMax = Vector2.zero;
        var barFillI = barFillGO.AddComponent<Image>();
        barFillI.color = new Color(0.2f, 0.8f, 1f); barFillI.type = Image.Type.Filled;
        barFillI.fillMethod = Image.FillMethod.Horizontal; barFillI.fillAmount = 0f;

        var luGO = new GameObject("LevelUpText"); luGO.transform.SetParent(hudGO.transform, false);
        var luR = luGO.AddComponent<RectTransform>();
        luR.anchorMin = new Vector2(0.5f, 0.5f); luR.anchorMax = new Vector2(0.5f, 0.5f);
        luR.anchoredPosition = new Vector2(0, 140); luR.sizeDelta = new Vector2(520, 85);
        var luT = luGO.AddComponent<Text>(); luT.text = "LEVEL UP!"; luT.fontSize = 50;
        luT.fontStyle = FontStyle.Bold; luT.color = new Color(1f, 0.85f, 0.1f); luT.alignment = TextAnchor.MiddleCenter;
        luT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        luGO.SetActive(false);

        xpBarFill = barFillI;
        var leg = panelGO.AddComponent<LegacyXPUI>();
        leg.levelText = lvlT; leg.xpText = xpT; leg.levelUpText = luT; leg.levelUpGO = luGO;
    }

    /// Pridá XP, prípadne posunie level (max 100), a uloží postup do PlayerPrefs (Level, XP).
    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentLevel < 100 && currentXP >= XPForLevel(currentLevel + 1))
            currentLevel++;

        // Ulož postup (prežije scénu aj reštart)
        PlayerPrefs.SetInt("Level", currentLevel);
        PlayerPrefs.SetInt("XP", currentXP);
        PlayerPrefs.Save();

        UpdateUI();
    }

    /// Celkové XP potrebné na dosiahnutie daného levelu: súčet i*i*20 pre i od 2 po level.
    public int XPForLevel(int level)
    {
        if (level <= 1) return 0;
        int total = 0;
        for (int i = 2; i <= level; i++)
            total += i * i * 20;
        return total;
    }

    /// Postup v rámci aktuálneho levelu (0-1) na vykreslenie XP baru.
    public float XPProgress()
    {
        if (currentLevel >= 100) return 1f;
        int prev = XPForLevel(currentLevel);
        int next = XPForLevel(currentLevel + 1);
        if (next <= prev) return 1f;
        return Mathf.Clamp01((float)(currentXP - prev) / (next - prev));
    }

    void UpdateUI()
    {
        if (xpBarFill != null) xpBarFill.fillAmount = XPProgress();
        // Text je aktualizovaný cez LegacyXPUI.Update()
    }
}
