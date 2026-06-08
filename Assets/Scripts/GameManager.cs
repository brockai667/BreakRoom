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
        if (left <= 0f) QuitToHub();   // čas vypršal -> koniec kola, do hubu
    }

    public void RegisterDestroy() => destroyedCount++;
    public void AddRoundMoney(int amount) => roundMoney += amount;

    /// Bonus na konci kola (rýchlosť + combo). Priebežné peniaze sú v roundMoney.
    public int CalculateBonus()
    {
        float speedMul   = Mathf.Max(0f, 300f - elapsedTime);
        int   timeBonus  = (int)(speedMul * 0.5f);
        int   comboBonus = destroyedCount >= 30 ? 200
                         : destroyedCount >= 20 ? 100
                         : destroyedCount >= 10 ? 40 : 0;
        return timeBonus + comboBonus;
    }

    /// Ukonči kolo a choď do hubu. Peniaze sa NEpridajú tu - hub ich
    /// animovane pripočíta k celkovej sume. Volá to tlačidlo QUIT v pauze.
    public void QuitToHub()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (roundActive)
        {
            roundActive = false;
            int earned = roundMoney + CalculateBonus();
            GameSession.SetResult(earned, destroyedCount, elapsedTime);
        }

        SceneManager.LoadScene("Hub");
    }

    // Legacy: ak by niečo ešte volalo EndRound, presmeruj na nový flow
    public void EndRound() => QuitToHub();

    public void GoToMenu()  { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
    public void GoToShop()  { Time.timeScale = 1f; GameSession.InitialHubTab = "Shop"; SceneManager.LoadScene("Hub"); }
    public void Replay()    { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}
