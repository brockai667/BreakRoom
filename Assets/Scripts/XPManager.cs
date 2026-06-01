using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Header("UI References")]
    public Image xpBarFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelUpText;

    [Header("State")]
    public int currentLevel = 1;
    public int currentXP = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
        if (levelUpText != null) levelUpText.gameObject.SetActive(false);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        // Show floating text (optional, simple popup)
        ShowXPPopup(amount);

        // Check level ups
        while (currentLevel < 100 && currentXP >= XPForLevel(currentLevel + 1))
            LevelUp();

        UpdateUI();
    }

    void LevelUp()
    {
        currentLevel++;
        if (levelUpText != null)
            StartCoroutine(ShowLevelUp());
    }

    // XP needed to reach level n (cumulative)
    // Level 2 = 200, Level 3 = 500, Level 10 = 5500, Level 50 = 127500
    public int XPForLevel(int level)
    {
        if (level <= 1) return 0;
        int total = 0;
        for (int i = 2; i <= level; i++)
            total += i * i * 20;
        return total;
    }

    public int XPNeededForNext()
    {
        if (currentLevel >= 100) return 0;
        return XPForLevel(currentLevel + 1) - currentXP;
    }

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
        if (levelText  != null) levelText.text = "LVL " + currentLevel;
        if (xpText     != null)
        {
            if (currentLevel >= 100) xpText.text = "MAX";
            else xpText.text = currentXP + " / " + XPForLevel(currentLevel + 1) + " XP";
        }
    }

    System.Collections.IEnumerator ShowLevelUp()
    {
        levelUpText.text = "LEVEL UP!  LVL " + currentLevel;
        levelUpText.gameObject.SetActive(true);
        float t = 0f;
        while (t < 2.5f) { t += Time.deltaTime; yield return null; }
        levelUpText.gameObject.SetActive(false);
    }

    void ShowXPPopup(int amount)
    {
        // Quick flash on xpText
        if (xpText != null)
            StartCoroutine(FlashXP(amount));
    }

    System.Collections.IEnumerator FlashXP(int amount)
    {
        Color orig = xpText.color;
        xpText.color = new Color(1f, 0.9f, 0.1f);
        yield return new WaitForSeconds(0.25f);
        xpText.color = orig;
    }
}
