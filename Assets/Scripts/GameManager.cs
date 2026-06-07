using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    public float elapsedTime = 0f;
    public int destroyedCount = 0;
    public bool roundActive = true;

    [Header("End Round UI")]
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
        roundActive = true;
        elapsedTime = 0f;
        destroyedCount = 0;

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

        elapsedTime += Time.deltaTime;

        // Skontroluj pauzu - ak je hra pauzovaná, nevolaj EndRound
        var pm = FindObjectOfType<PauseMenu>();
        if (pm != null && pm.IsPaused) return;

        if (UnityEngine.InputSystem.Keyboard.current == null) return;

        // TAB alebo END = koniec kola
        if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame ||
            UnityEngine.InputSystem.Keyboard.current.endKey.wasPressedThisFrame)
        {
            EndRound();
        }
    }

    public void RegisterDestroy() => destroyedCount++;

    public void EndRound()
    {
        if (!roundActive) return;
        roundActive = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int earned = CalculateMoney();
        // Peniaze za kolo (nad priebežné peniaze za každý objekt)
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.AddMoney(earned);

        int total = PlayerInventory.Instance != null ? PlayerInventory.Instance.Money : earned;

        if (endPanel != null) endPanel.SetActive(true);

        int min = (int)elapsedTime / 60;
        float sec = elapsedTime % 60f;
        if (timeText        != null) timeText.text        = $"Čas: {min:00}:{sec:00.0}";
        if (destroyedText   != null) destroyedText.text   = $"Rozbité: {destroyedCount} objektov";
        if (moneyEarnedText != null) moneyEarnedText.text = $"+${earned}";
        if (totalMoneyText  != null) totalMoneyText.text  = $"Celkom: ${total}";
    }

    int CalculateMoney()
    {
        int baseM      = destroyedCount * 15;
        float speedMul = Mathf.Max(0f, 300f - elapsedTime);
        int timeBonus  = (int)(speedMul * 0.5f);
        int comboBonus = destroyedCount >= 30 ? 200
                       : destroyedCount >= 20 ? 100
                       : destroyedCount >= 10 ? 40 : 0;
        return baseM + timeBonus + comboBonus;
    }

    public void GoToMenu()  { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
    public void GoToShop()  { Time.timeScale = 1f; SceneManager.LoadScene("Shop"); }
    public void Replay()    { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}
