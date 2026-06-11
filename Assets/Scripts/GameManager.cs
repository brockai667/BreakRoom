using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    public float elapsedTime    = 0f;
    public int   destroyedCount = 0;
    public int   roundMoney     = 0;   // peniaze nazbierané v tomto kole (pridajú sa v hube)
    public bool  roundActive    = true;

    // --- Sledovanie "vyčistenia miestnosti" (auto-koniec) ---
    int   liveBreakables = 0;   // koľko rozbitných vecí (vrátane kúskov) je práve v scéne
    bool  anyRegistered  = false;
    bool  warmupDone     = false;   // počkaj kým sa všetky veci zaregistrujú
    float clearDelay     = -1f;     // po vyčistení krátka oslava, potom vyhodnotenie

    [Header("Round timer")]
    public float    roundDuration = 300f;   // dĺžka kola v sekundách (5:00)
    public TMP_Text timerText;

    [Header("End Round UI (legacy - nepoužité v novom hub flow)")]
    public GameObject endPanel;
    public Text timeText;
    public Text destroyedText;
    public Text moneyEarnedText;
    public Text totalMoneyText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (endPanel != null) endPanel.SetActive(false);
        roundActive    = true;
        elapsedTime    = 0f;
        destroyedCount = 0;
        roundMoney     = 0;

        if (PlayerInventory.Instance != null)
        {
            var wh = FindObjectOfType<WeaponHit>();
            if (wh != null) wh.ApplyWeapon(PlayerInventory.Instance.GetEquipped());
            var hd = FindObjectOfType<HandDisplay>();
            if (hd != null) hd.SetWeapon(PlayerInventory.Instance.GetEquipped());
        }
    }

    void Update()
    {
        if (!roundActive) return;

        // Ak je hra pauzovaná, nerátaj čas (TAB aj ESC pauzuje cez PauseMenu)
        var pm = FindObjectOfType<PauseMenu>();
        if (pm != null && pm.IsPaused) return;

        elapsedTime += Time.deltaTime;

        // Odpočítavací časovač
        float left = Mathf.Max(0f, roundDuration - elapsedTime);
        if (timerText != null)
        {
            int m = (int)left / 60;
            int s = (int)left % 60;
            timerText.text = $"{m:00}:{s:00}";
            timerText.color = left <= 30f ? new Color(1f, 0.3f, 0.2f) : Color.white;
        }
        if (left <= 0f) { EndAndGoHub(false); return; }   // čas vypršal -> vyhodnotenie

        // --- Auto-koniec: keď je celá miestnosť zničená ---
        if (!warmupDone && elapsedTime > 1.5f) warmupDone = true;  // počkaj na registráciu
        if (warmupDone && anyRegistered)
        {
            if (clearDelay < 0f)
            {
                if (liveBreakables <= 0)
                {
                    clearDelay = 1.0f;                     // krátka oslava
                    if (Announcer.Instance != null) Announcer.Show("VŠETKO ZNIČENÉ!", true);
                }
            }
            else
            {
                if (liveBreakables > 0) clearDelay = -1f;  // objavili sa nové kúsky -> zruš
                else
                {
                    clearDelay -= Time.deltaTime;
                    if (clearDelay <= 0f) { EndAndGoHub(true); return; }
                }
            }
        }
    }

    public void RegisterDestroy() => destroyedCount++;
    public void AddRoundMoney(int amount) => roundMoney += amount;

    // Rozbitné veci sa hlásia sem, aby sme vedeli, kedy je miestnosť čistá
    public void RegisterBreakable()   { liveBreakables++; anyRegistered = true; }
    public void UnregisterBreakable() { liveBreakables--; }

    /// Bonus na konci kola = odmena za množstvo rozbitia (combo).
    /// Hlavné peniaze sú v roundMoney (súčet odmien za jednotlivé veci),
    /// takže platí pravidlo "viac rozbiješ = viac zarobíš". Bonus je len
    /// malá nadstavba za usilovnosť, nie hlavný zdroj peňazí.
    public int CalculateBonus()
    {
        return destroyedCount >= 120 ? 250
             : destroyedCount >=  80 ? 150
             : destroyedCount >=  45 ?  80
             : destroyedCount >=  20 ?  30 : 0;
    }

    /// Tlačidlo QUIT v pauze: ukonči kolo a ukáž vyhodnotenie v hube.
    public void QuitToHub() => EndAndGoHub(false);

    /// Ukonči kolo a choď na vyhodnotenie do hubu. Peniaze sa NEpridajú tu -
    /// hub ich animovane pripočíta. cleared = hráč zničil celú miestnosť.
    void EndAndGoHub(bool cleared)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (roundActive)
        {
            roundActive = false;
            int bonus = CalculateBonus();
            if (cleared) bonus += 150;                  // bonus za vyčistenie miestnosti
            int earned = roundMoney + bonus;
            string grade = ComputeGrade(cleared);
            GameSession.SetResult(earned, destroyedCount, elapsedTime, grade, cleared);
        }

        SceneManager.LoadScene("Hub");
    }

    // Hodnotenie kola: S / A / B / C / D
    string ComputeGrade(bool cleared)
    {
        if (cleared)
            return elapsedTime <= 90f ? "S" : elapsedTime <= 160f ? "A" : "B";
        int d = destroyedCount;
        return d >= 150 ? "S" : d >= 90 ? "A" : d >= 50 ? "B" : d >= 20 ? "C" : "D";
    }

    // Legacy: ak by niečo ešte volalo EndRound, presmeruj na nový flow
    public void EndRound() => QuitToHub();

    public void GoToMenu()  { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
    public void GoToShop()  { Time.timeScale = 1f; GameSession.InitialHubTab = "Shop"; SceneManager.LoadScene("Hub"); }
    public void Replay()    { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}
