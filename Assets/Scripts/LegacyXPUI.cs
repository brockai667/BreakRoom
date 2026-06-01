using UnityEngine;
using UnityEngine.UI;

// Bridges XPManager to legacy Unity UI Text (no TMP required)
public class LegacyXPUI : MonoBehaviour
{
    public Text levelText;
    public Text xpText;
    public Text levelUpText;
    public GameObject levelUpGO;

    private XPManager xpm;
    private int lastLevel = -1;

    void Awake()
    {
        xpm = GetComponent<XPManager>();
        if (xpm == null) xpm = FindObjectOfType<XPManager>();
    }

    void Update()
    {
        if (xpm == null) return;

        // Sync bar fill directly
        // (already handled by XPManager.UpdateUI via Image.fillAmount)

        // Level text
        if (levelText != null)
            levelText.text = "LVL " + xpm.currentLevel;

        // XP text
        if (xpText != null)
        {
            if (xpm.currentLevel >= 100)
                xpText.text = "MAX LEVEL";
            else
                xpText.text = xpm.currentXP + " / " + xpm.XPForLevel(xpm.currentLevel + 1) + " XP";
        }

        // Level-up flash
        if (xpm.currentLevel != lastLevel && lastLevel != -1)
        {
            lastLevel = xpm.currentLevel;
            if (levelUpGO != null)
                StartCoroutine(ShowLevelUp());
        }
        else if (lastLevel == -1)
            lastLevel = xpm.currentLevel;
    }

    System.Collections.IEnumerator ShowLevelUp()
    {
        if (levelUpText != null) levelUpText.text = "LEVEL UP!  LVL " + xpm.currentLevel;
        if (levelUpGO != null)   levelUpGO.SetActive(true);

        // Animate scale
        float t = 0f;
        while (t < 2.5f)
        {
            t += Time.deltaTime;
            if (levelUpGO != null)
            {
                float scale = 1f + Mathf.Sin(t * 8f) * 0.06f;
                levelUpGO.transform.localScale = Vector3.one * scale;
            }
            yield return null;
        }
        if (levelUpGO != null) levelUpGO.SetActive(false);
    }
}
