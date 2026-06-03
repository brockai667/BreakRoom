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
        // Vždy skryj end panel na začiatku
        if (endPanel != null) endPanel.SetActive(false);
        roundActive = true;
        elapsedTime = 0f;
        destroyedCount = 0;

        // Aplikuj vybranú zbraň
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

        if (UnityEngine.InputSystem.Keyboard.current != null &&
            (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.endKey.wasPressedThisFrame))
        {
            EndRound();
        }
    }

    public void RegisterDestroy()
    {
        destroyedCount++;
    }

    public void EndRound()
    {
        roundActive = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int earned = CalculateMoney();
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.AddMoney(earned);

        int total = PlayerInventory.Instance != null ? PlayerInventory.Instance.Money : earned;

        if (endPanel != null) endPanel.SetActive(true);

        int min = (int)elapsedTime / 60;
        float sec = elapsedTime % 60f;
        if (timeText       != null) timeText.text       = $"Čas: {min:00}:{sec:00.0}";
        if (destroyedText  != null) destroyedText.text  = $"Rozbité objekty: {destroyedCount}";
        if (moneyEarnedText!= null) moneyEarnedText.text= $"+${earned}";
        if (totalMoneyText != null) totalMoneyText.text = $"Celkom peňazí: ${total}";
    }

    int CalculateMoney()
    {
        int base_ = destroyedCount * 15;
        float speedBonus = Mathf.Max(0f, 300f - elapsedTime);
        int timeBonus  = (int)(speedBonus * 0.5f);
        int countBonus = destroyedCount >= 20 ? 100 : destroyedCount >= 10 ? 40 : 0;
        return base_ + timeBonus + countBonus;
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToShop()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Shop");
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
