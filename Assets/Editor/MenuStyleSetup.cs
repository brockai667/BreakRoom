// Assets/Editor/MenuStyleSetup.cs
// Unity: Break Room -> Style Redesign

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class MenuStyleSetup : Editor
{
    [MenuItem("Break Room/Style Redesign")]
    static void ApplyStyle()
    {
        // ── TITLE ────────────────────────────────────────────────────
        GameObject titleGO = GameObject.Find("TitleText");
        if (titleGO != null)
        {
            RectTransform titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchoredPosition = new Vector2(0f, 230f);
            titleRT.sizeDelta        = new Vector2(1400f, 220f);

            TextMeshProUGUI title = titleGO.GetComponent<TextMeshProUGUI>();
            if (title != null)
            {
                title.fontSize    = 175f;
                title.color       = new Color(1f, 0.85f, 0.05f, 1f);
                title.alignment   = TextAlignmentOptions.Center;
                title.fontStyle   = FontStyles.Bold;
                // Outline vypnuty — pridaj ho manualne cez Inspector ak chces
            }
        }
        else
        {
            Debug.LogWarning("TitleText nenajdeny!");
        }

        // ── BUTTONY ───────────────────────────────────────────────────
        StyleButton("PlayButton",       new Vector2(0f,  40f));
        StyleButton("ShopButton",       new Vector2(0f, -70f));
        StyleButton("CollectionButton", new Vector2(0f, -180f));

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Style redesign hotovy! Ctrl+S uloz.");
    }

    static void StyleButton(string name, Vector2 position)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) { Debug.LogWarning("Nenajdeny: " + name); return; }

        // Pozicia a velkost
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta        = new Vector2(500f, 80f);

        // Farba pozadia
        Image img = go.GetComponent<Image>();
        if (img != null)
            img.color = new Color(0.18f, 0.02f, 0.02f, 0.92f);

        // Button hover — explicitne nastavime Transition na ColorTint
        Button btn = go.GetComponent<Button>();
        if (btn != null)
        {
            btn.transition = Selectable.Transition.ColorTint;

            ColorBlock cb       = btn.colors;
            cb.normalColor      = new Color(0.18f, 0.02f, 0.02f, 0.92f);
            cb.highlightedColor = new Color(0.50f, 0.10f, 0.02f, 1.00f);
            cb.pressedColor     = new Color(0.08f, 0.01f, 0.01f, 1.00f);
            cb.selectedColor    = new Color(0.18f, 0.02f, 0.02f, 0.92f);
            cb.fadeDuration     = 0.1f;
            btn.colors          = cb;
        }

        // Text
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.fontSize    = 52f;
            txt.color       = new Color(1f, 0.92f, 0.6f, 1f);
            txt.alignment   = TextAlignmentOptions.Center;
            txt.fontStyle   = FontStyles.Bold;
            txt.outlineColor = new Color32(80, 0, 0, 255);
            txt.outlineWidth = 0.08f;
        }
    }
}
#endif
