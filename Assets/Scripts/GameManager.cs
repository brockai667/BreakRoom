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
    public int   roundMoney     = 0;   // peniaze nazbierane v tomto kole (pridaju sa v hube)
    public bool  roundActive    = true;

    // --- Sledovanie "vycistenia miestnosti" (auto-koniec) ---
    int   origTotal      = 0;   // pocet povodnych (scenovych) objektov v miestnosti
    int   origRemaining  = 0;   // kolko z povodnych este ostava
    bool  warmupDone     = false;
    float clearDelay     = -1f;

    // --- Combo ---
    [Header("Combo")]
    public  int   comboCount  = 0;
    float   comboTimer = 0f;
    const   float COMBO_WINDOW = 2.2f;
    int     lastMilestoneAnnounced = 0;

    [Header("Round timer")]
    public float    roundDuration = 300f;
    public TMP_Text timerText;

    [Header("End Round UI (legacy - nepouzite v novom hub flow)")]
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
        Time.timeScale = 1f;
        if (endPanel != null) endPanel.SetActive(false);
        roundActive    = true;
        elapsedTime    = 0f;
        destroyedCount = 0;
        roundMoney     = 0;
        comboCount     = 0;

        if (PlayerInventory.Instance != null)
        {
            var wh = FindFirstObjectByType<WeaponHit>();
            if (wh != null) wh.ApplyWeapon(PlayerInventory.Instance.GetEquipped());
            var hd = FindFirstObjectByType<HandDisplay>();
            if (hd != null) hd.SetWeapon(PlayerInventory.Instance.GetEquipped());
        }
    }

    void Update()
    {
        if (!roundActive) return;

        var pm = FindFirstObjectByType<PauseMenu>();
        if (pm != null && pm.IsPaused) return;

        elapsedTime += Time.deltaTime;

        // Combo doznieva v realnom case
        if (comboCount > 0)
        {
            comboTimer -= Time.unscaledDeltaTime;
            if (comboTimer <= 0f) { comboCount = 0; lastMilestoneAnnounced = 0; }
        }

        // Odpocitavaci casovac
        float left = Mathf.Max(0f, roundDuration - elapsedTime);
        if (timerText != null)
        {
            int m = (int)left / 60;
            int s = (int)left % 60;
            timerText.text = $"{m:00}:{s:00}";
            timerText.color = left <= 30f ? new Color(1f, 0.3f, 0.2f) : Color.white;
        }
        if (left <= 0f) { EndAndGoHub(false); return; }

        // --- Auto-koniec: ked su znicene vsetky POVODNE objekty miestnosti ---
        if (!warmupDone && elapsedTime > 1.5f) warmupDone = true;
        if (warmupDone && origTotal > 0)
        {
            if (clearDelay < 0f)
            {
                if (origRemaining <= 0)
                {
                    clearDelay = 1.2f;
                    Time.timeScale = 0.4f;                 // slow-motion oslava
                    if (Announcer.Instance != null) Announcer.Show("ALL SMASHED!", true);
                }
            }
            else
            {
                clearDelay -= Time.unscaledDeltaTime;
                if (clearDelay <= 0f) { Time.timeScale = 1f; EndAndGoHub(true); return; }
            }
        }
    }

    // ---------- COMBO / ODMENY ----------
    public float ComboMultiplier()
    {
        int c = comboCount;
        return c < 3  ? 1f
             : c < 6  ? 1.25f
             : c < 10 ? 1.5f
             : c < 16 ? 2f
             : c < 25 ? 2.5f
                      : 3f;
    }

    public float DestructionPct()
        => origTotal > 0 ? Mathf.Clamp01(1f - (float)origRemaining / origTotal) : 0f;

    /// Hlavne pripisanie odmeny za rozbitie. Vracia skutocne pripisane $.
    public int AwardBreak(int baseReward, int baseXp, bool golden, Vector3 pos)
    {
        if (!roundActive) return 0;

        comboCount++;
        comboTimer = COMBO_WINDOW + Perks.ComboWindowBonus();

        float mult = ComboMultiplier();
        int pay = Mathf.Max(1, Mathf.RoundToInt(baseReward * mult * Perks.MoneyMult()));
        if (golden) pay *= 3;

        roundMoney += pay;
        destroyedCount++;

        if (XPManager.Instance != null) XPManager.Instance.AddXP(baseXp);

        AnnounceComboMilestone();
        return pay;
    }

    void AnnounceComboMilestone()
    {
        int[] miles = { 30, 20, 10, 5 };
        foreach (int m in miles)
        {
            if (comboCount >= m && lastMilestoneAnnounced < m)
            {
                lastMilestoneAnnounced = m;
                if (Announcer.Instance != null)
                    Announcer.Show("COMBO x" + comboCount + "!", true);
                break;
            }
        }
    }

    public void RegisterDestroy() => destroyedCount++;
    public void AddRoundMoney(int amount) => roundMoney += amount;

    public void RegisterBreakable(bool isChunk = false)
    {
        if (!isChunk) { origTotal++; origRemaining++; }
    }
    public void UnregisterBreakable(bool isChunk = false)
    {
        if (!isChunk) origRemaining--;
    }

    public int CalculateBonus()
    {
        return destroyedCount >= 120 ? 80
             : destroyedCount >=  80 ? 50
             : destroyedCount >=  45 ? 25
             : destroyedCount >=  20 ? 10 : 0;
    }

    public void QuitToHub() => EndAndGoHub(false);

    void EndAndGoHub(bool cleared)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (roundActive)
        {
            roundActive = false;
            int bonus = CalculateBonus();
            if (cleared) bonus += 50;
            int earned = roundMoney + bonus;
            string grade = ComputeGrade(cleared);
            GameSession.SetResult(earned, destroyedCount, elapsedTime, grade, cleared);
        }

        SceneManager.LoadScene("Hub");
    }

    string ComputeGrade(bool cleared)
    {
        if (cleared)
            return elapsedTime <= 90f ? "S" : elapsedTime <= 160f ? "A" : "B";
        int d = destroyedCount;
        return d >= 150 ? "S" : d >= 90 ? "A" : d >= 50 ? "B" : d >= 20 ? "C" : "D";
    }

    public void EndRound() => QuitToHub();

    public void GoToMenu()  { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
    public void GoToShop()  { Time.timeScale = 1f; GameSession.InitialHubTab = "Shop"; SceneManager.LoadScene("Hub"); }
    public void Replay()    { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}
