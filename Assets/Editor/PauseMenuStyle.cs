// Assets/Editor/PauseMenuStyle.cs
// Unity: Break Room -> Style Pause Menu

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class PauseMenuStyle : Editor
{
    [MenuItem("Break Room/Style Pause Menu")]
    static void ApplyPauseStyle()
    {
        // PausePanel — tmave polopriehladne pozadie
        GameObject panel = GameObject.Find("PausePanel");
        if (panel != null)
        {
            Image panelImg = panel.GetComponent<Image>();
            if (panelImg != null)
                panelImg.color = new Color(0.05f, 0.0f, 0.0f, 0.85f);

            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(550f, 380f);
        }

        // PAUSED title
        GameObject titleGO = GameObject.Find("PauseTitle");
        if (titleGO != null)
        {
            RectTransform rt = titleGO.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 120f);
            rt.sizeDelta        = new Vector2(500f, 130f);

            TextMeshProUGUI txt = titleGO.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text      = "PAUSED";
                txt.fontSize  = 110f;
                txt.color     = new Color(1f, 0.85f, 0.05f, 1f);
                txt.alignment = TextAlignmentOptions.Center;
                txt.fontStyle = FontStyles.Bold;
            }
        }

        StylePauseButton("ResumeButton", new Vector2(0f, -20f),  "RESUME");
        StylePauseButton("MenuButton",   new Vector2(0f, -120f), "MAIN MENU");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Pause menu style hotovy! Ctrl+S uloz.");
    }

    static void StylePauseButton(string name, Vector2 pos, string label)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) return;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(400f, 75f);

        Image img = go.GetComponent<Image>();
        if (img != null)
            img.color = new Color(0.18f, 0.02f, 0.02f, 0.92f);

        Button btn = go.GetComponent<Button>();
        if (btn != null)
        {
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb       = btn.colors;
            cb.normalColor      = new Color(0.18f, 0.02f, 0.02f, 0.92f);
            cb.highlightedColor = new Color(0.50f, 0.10f, 0.02f, 1.00f);
            cb.pressedColor     = new Color(0.08f, 0.01f, 0.01f, 1.00f);
            cb.fadeDuration     = 0.1f;
            btn.colors          = cb;
        }

        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text      = label;
            txt.fontSize  = 48f;
            txt.color     = new Color(1f, 0.92f, 0.6f, 1f);
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontStyle = FontStyles.Bold;
        }
    }
}
#endif
