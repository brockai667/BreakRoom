
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
 
/// Riadi hub (lobby) obrazovku: taby Play/Loadout/Shop, peniaze vpravo hore,
/// výber mapy, štart hry, loadout zbraní a animáciu počítania peňazí po kole.
public class HubManager : MonoBehaviour
{
    [Header("Top bar")]
    public Text   moneyText;
    public Button playTab;
    public Button loadoutTab;
    public Button shopTab;
 
    [Header("Panels")]
    public GameObject playPanel;
    public GameObject loadoutPanel;
    public GameObject shopPanel;
 
    [Header("Play panel")]
    public Text mapLabel;
 
    [Header("Loadout panel")]
    public Transform loadoutContainer;
 
    [Header("Results / earnings animation")]
    public GameObject resultsPanel;
    public Text       resultsText;
    public Text       earnedFlyText;
 
    static readonly Color TAB_ON  = new Color(0.55f, 0.15f, 0.05f, 1f);
    static readonly Color TAB_OFF = new Color(0.18f, 0.10f, 0.05f, 1f);
 
    void Start()
    {
        // Zaisti PlayerInventory
        if (PlayerInventory.Instance == null)
            new GameObject("PlayerInventory").AddComponent<PlayerInventory>();
 
        if (resultsPanel != null) resultsPanel.SetActive(false);
 
        // Postavička robotníka v pozadí lobby (low-poly, OAR/Synty štýl)
        LobbyCharacter.EnsureInScene();

        UpdateMoney();
        SyncMapIndex();
        UpdateMapLabel();
        ShowTab(string.IsNullOrEmpty(GameSession.InitialHubTab) ? "Play" : GameSession.InitialHubTab);
 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
 
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnChanged += UpdateMoney;
 
        // Po návrate z kola: animuj pripočítanie peňazí
        if (GameSession.HasPendingResult)
            StartCoroutine(AnimateEarnings());
    }
 
    void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnChanged -= UpdateMoney;
    }
 
    // ---------- TABY ----------
    public void ShowPlay()    => ShowTab("Play");
    public void ShowLoadout() => ShowTab("Loadout");
    public void ShowShop()    => ShowTab("Shop");
 
    void ShowTab(string tab)
    {
        if (playPanel    != null) playPanel.SetActive(tab == "Play");
        if (loadoutPanel != null) loadoutPanel.SetActive(tab == "Loadout");
        if (shopPanel    != null) shopPanel.SetActive(tab == "Shop");
 
        SetTabColor(playTab,    tab == "Play");
        SetTabColor(loadoutTab, tab == "Loadout");
        SetTabColor(shopTab,    tab == "Shop");
 
        // 3D náhľad zbrane na pódiu — viditeľný na Shop/Loadout, skrytý na Play
        if (WeaponPreview.Instance != null)
        {
            bool show = tab != "Play";
            if (show && PlayerInventory.Instance != null)
                WeaponPreview.Instance.Show(PlayerInventory.Instance.GetEquipped());
            WeaponPreview.Instance.SetVisible(show);
        }
 
        if (tab == "Loadout") BuildLoadout();
    }
 
    void SetTabColor(Button b, bool on)
    {
        if (b == null) return;
        var img = b.GetComponent<Image>();
        if (img != null) img.color = on ? TAB_ON : TAB_OFF;
    }
 
    // ---------- MAPA ----------
    // Mapy v poradí + level potrebný na odomknutie (Obývačka = prvý level)
    static readonly (string scene, string label, int unlock)[] MAPS =
    {
        ("Obyvacka", "Obývačka",   1),
        ("Office",   "Kancelária", 3),
        ("Factory",  "Továreň",    5),
    };
    int mapIndex = 0;
 
    // Tlačidlo mapy: prepína medzi mapami (vidíš aj zamknuté + ich požiadavku)
    public void SelectOffice() => CycleMap();
 
    public void CycleMap()
    {
        mapIndex = (mapIndex + 1) % MAPS.Length;
        GameSession.SelectedMap = MAPS[mapIndex].scene;
        UpdateMapLabel();
    }
 
    bool IsUnlocked(int unlockLevel) => XPManager.SavedLevel >= unlockLevel;
 
    void SyncMapIndex()
    {
        mapIndex = 0;
        for (int i = 0; i < MAPS.Length; i++)
            if (MAPS[i].scene == GameSession.SelectedMap) { mapIndex = i; break; }
        if (!IsUnlocked(MAPS[mapIndex].unlock)) mapIndex = 0; // Obývačka je vždy dostupná
        GameSession.SelectedMap = MAPS[mapIndex].scene;
    }
 
    void UpdateMapLabel()
    {
        if (mapLabel == null) return;
        var m = MAPS[mapIndex];
        int lvl = XPManager.SavedLevel;
        bool ok = IsUnlocked(m.unlock);
        mapLabel.color = ok ? Color.white : new Color(1f, 0.45f, 0.3f);
        mapLabel.text = ok
            ? $"Mapa: {m.label}    •    Level {lvl}"
            : $"🔒 {m.label} — odomkne sa na leveli {m.unlock}  (máš level {lvl})";
    }
 
    public void StartGame()
    {
        var m = MAPS[mapIndex];
        if (!IsUnlocked(m.unlock))
        {
            UpdateMapLabel();   // zvýrazní zámok na červeno
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(m.scene);
    }
 
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
 
    // ---------- PENIAZE ----------
    void UpdateMoney()
    {
        if (moneyText != null && PlayerInventory.Instance != null)
            moneyText.text = "$" + PlayerInventory.Instance.Money;
    }
 
    // ---------- LOADOUT ----------
    void BuildLoadout()
    {
        if (loadoutContainer == null) return;
        foreach (Transform c in loadoutContainer) Destroy(c.gameObject);
 
        var inv = PlayerInventory.Instance;
        foreach (var w in WeaponData.All)
        {
            bool owned    = inv != null && inv.Owns(w.id);
            bool equipped = inv != null && inv.EquippedId == w.id;
 
            string label = equipped ? "✓ " + w.displayName
                         : owned    ? w.displayName
                                    : "🔒 " + w.displayName + "  (v shope)";
            Color col = equipped ? new Color(0.10f, 0.45f, 0.12f)
                      : owned    ? new Color(0.15f, 0.20f, 0.35f)
                                 : new Color(0.22f, 0.12f, 0.10f);
 
            var row = MakeRowButton(loadoutContainer, label, col, w.handColor);
            var btn = row.GetComponent<Button>();
            string id = w.id;
            if (owned && !equipped)
                btn.onClick.AddListener(() => {
                    inv.Equip(id);
                    if (WeaponPreview.Instance != null) WeaponPreview.Instance.Show(WeaponData.Get(id));
                    BuildLoadout();
                });
            else
                btn.interactable = false;
        }
    }
 
    // ---------- ANIMÁCIA PEŇAZÍ ----------
    IEnumerator AnimateEarnings()
    {
        int   earned    = GameSession.PendingEarned;
        int   destroyed = GameSession.PendingDestroyed;
        float time      = GameSession.PendingTime;
        int   startTotal = PlayerInventory.Instance != null ? PlayerInventory.Instance.Money : 0;
 
        if (resultsPanel != null) resultsPanel.SetActive(true);
        if (resultsText != null)
        {
            int m = (int)time / 60;
            int s = (int)time % 60;
            resultsText.text = $"Rozbité: {destroyed}    Čas: {m:00}:{s:00}";
        }
        if (earnedFlyText != null) earnedFlyText.text = "";
 
        // krátke oneskorenie
        yield return WaitUnscaled(0.5f);
 
        // postupné odpočítavanie zarobeného a pripočítavanie k celku
        if (earnedFlyText != null) earnedFlyText.text = "+$" + earned;
        float dur = 1.3f, t = 0f, coinTick = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            int shownTotal   = startTotal + Mathf.RoundToInt(earned * k);
            int shownEarned  = Mathf.RoundToInt(earned * (1f - k));
            if (moneyText     != null) moneyText.text     = "$" + shownTotal;
            if (earnedFlyText != null) earnedFlyText.text = shownEarned > 0 ? "+$" + shownEarned : "";
            coinTick -= Time.unscaledDeltaTime;
            if (earned > 0 && coinTick <= 0f) { coinTick = 0.12f; SfxManager.Coin(); }
            yield return null;
        }
 
        // commit (uloží do PlayerPrefs)
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.AddMoney(earned);
        UpdateMoney();
        if (earnedFlyText != null) earnedFlyText.text = "";
        GameSession.ClearResult();
 
        yield return WaitUnscaled(1.4f);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }
 
    IEnumerator WaitUnscaled(float secs)
    {
        float t = 0f;
        while (t < secs) { t += Time.unscaledDeltaTime; yield return null; }
    }
 
    // ---------- HELPERY UI ----------
    GameObject MakeRowButton(Transform parent, string label, Color bg, Color iconCol)
    {
        var go = new GameObject("Row");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = bg;
        var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(520, 56);
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var le = go.AddComponent<LayoutElement>(); le.minHeight = 56; le.preferredHeight = 56;
 
        // ikona (farebný štvorec)
        var icon = new GameObject("Icon"); icon.transform.SetParent(go.transform, false);
        var iimg = icon.AddComponent<Image>(); iimg.color = iconCol;
        var irt = icon.GetComponent<RectTransform>();
        irt.anchorMin = new Vector2(0, 0.5f); irt.anchorMax = new Vector2(0, 0.5f);
        irt.pivot = new Vector2(0, 0.5f);
        irt.anchoredPosition = new Vector2(14, 0);
        irt.sizeDelta = new Vector2(34, 34);
 
        // text
        MakeText(go, label, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                 new Vector2(64, 0), new Vector2(440, 50));
        return go;
    }
 
    GameObject MakeText(GameObject parent, string txt, int size, FontStyle style,
        Color col, TextAnchor anchor, Vector2 pos, Vector2 sizeD)
    {
        var go = new GameObject("Txt"); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 0.5f); r.anchorMax = new Vector2(0, 0.5f);
        r.pivot = new Vector2(0, 0.5f);
        r.anchoredPosition = pos; r.sizeDelta = sizeD;
        var t = go.AddComponent<Text>(); t.text = txt; t.fontSize = size;
        t.fontStyle = style; t.color = col; t.alignment = anchor;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }
}
 