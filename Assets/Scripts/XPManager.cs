using UnityEngine;
using UnityEngine.UI;

// Používame len Legacy UI Text - bez TMP závislosti
public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Header("UI References (Legacy Text)")]
    public Image xpBarFill;
    // TMP polia odstraňujeme - LegacyXPUI sa stará o text

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
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentLevel < 100 && currentXP >= XPForLevel(currentLevel + 1))
            currentLevel++;

        UpdateUI();
    }

    public int XPForLevel(int level)
    {
        if (level <= 1) return 0;
        int total = 0;
        for (int i = 2; i <= level; i++)
            total += i * i * 20;
        return total;
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
        // Text je aktualizovaný cez LegacyXPUI.Update()
    }
}
