using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;

/// Opravy UI: hlavné menu (len PLAY + QUIT GAME) a pause menu v Office.
public class FixUI
{
    const string BANGERS_GUID = "f62de6debf194a140b0eab5be7f29d66";

    static TMP_FontAsset LoadBangers()
    {
        string p = AssetDatabase.GUIDToAssetPath(BANGERS_GUID);
        return string.IsNullOrEmpty(p) ? null : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
    }

    // ===================== MAIN MENU =====================
    [MenuItem("BreakRoom/Fix Main Menu")]
    public static void FixMainMenu()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        var play = GameObject.Find("PlayButton");
        var shop = GameObject.Find("ShopButton");
        var coll = GameObject.Find("CollectionButton");
        var mm   = Object.FindObjectOfType<MainMenu>();

        if (play != null)
        {
            var pr = play.GetComponent<RectTransform>();
            pr.anchoredPosition = new Vector2(0, -10);

            // QUIT GAME = klon PLAY (zachová štýl + Bangers font)
            var quit = Object.Instantiate(play, play.transform.parent);
            quit.name = "QuitButton";
            quit.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);

            var tmp = quit.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = "QUIT GAME";

            var btn = quit.GetComponent<Button>();
            for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(btn.onClick, i);
            if (mm != null) UnityEventTools.AddPersistentListener(btn.onClick, mm.KoniecHry);
        }

        if (shop != null) Object.DestroyImmediate(shop);
        if (coll != null) Object.DestroyImmediate(coll);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✅ Main Menu opravené: len PLAY + QUIT GAME");
    }

    // ===================== OFFICE PAUSE =====================
    [MenuItem("BreakRoom/Fix Office Pause")]
    public static void FixOfficePause()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Office.unity");
        var bangers = LoadBangers();

        // PauseMenu controller
        var pm = Object.FindObjectOfType<PauseMenu>();
        if (pm == null)
            pm = new GameObject("PauseMenuController").AddComponent<PauseMenu>();

        // Zruš starý PauseCanvas/PausePanel
        var oldCanvas = GameObject.Find("PauseCanvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
        var oldPanel = GameObject.Find("PausePanel");
        if (oldPanel != null) Object.DestroyImmediate(oldPanel);

        // Nový PauseCanvas (navrchu)
        var cvGO = new GameObject("PauseCanvas");
        var cv = cvGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 200;
        var sc = cvGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        cvGO.AddComponent<GraphicRaycaster>();

        // Dim panel cez celú obrazovku
        var panel = new GameObject("PausePanel");
        panel.transform.SetParent(cvGO.transform, false);
        var pImg = panel.AddComponent<Image>(); pImg.color = new Color(0f, 0f, 0f, 0.78f);
        var pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = Vector2.zero; pRt.anchorMax = Vector2.one; pRt.offsetMin = pRt.offsetMax = Vector2.zero;

        // PAUSED
        MkTMP(panel, "PauseTitle", "PAUSED", 96, bangers, new Color(1f, 0.85f, 0.1f),
              new Vector2(0, 200), new Vector2(700, 130));

        // RESUME
        var resume = MkButton(panel, "ResumeButton", "RESUME", bangers,
            new Color(0.17f, 0.10f, 0.05f), new Vector2(0, 30), new Vector2(380, 90));
        UnityEventTools.AddPersistentListener(resume.GetComponent<Button>().onClick, pm.Resume);

        // QUIT (do hubu so spočítaním peňazí)
        var quit = MkButton(panel, "QuitButton", "QUIT", bangers,
            new Color(0.45f, 0.10f, 0.05f), new Vector2(0, -90), new Vector2(380, 90));
        UnityEventTools.AddPersistentListener(quit.GetComponent<Button>().onClick, pm.GoToMainMenu);

        pm.pausePanel = panel;

        // EventSystem s novým input modulom
        EnsureEventSystem();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✅ Office pause menu postavené: PAUSED + RESUME + QUIT");
    }

    // ===================== OFFICE TIMER =====================
    [MenuItem("BreakRoom/Add Office Timer")]
    public static void AddOfficeTimer()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Office.unity");
        var bangers = LoadBangers();
        var gm = Object.FindObjectOfType<GameManager>();
        if (gm == null) { Debug.LogError("GameManager nenájdený v Office scéne."); return; }

        var old = GameObject.Find("TimerCanvas");
        if (old != null) Object.DestroyImmediate(old);

        var cvGO = new GameObject("TimerCanvas");
        var cv = cvGO.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 50;
        var sc = cvGO.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080);
        cvGO.AddComponent<GraphicRaycaster>();

        // tmavé pozadie pre čitateľnosť
        var bg = new GameObject("TimerBG"); bg.transform.SetParent(cvGO.transform, false);
        var bgImg = bg.AddComponent<Image>(); bgImg.color = new Color(0f, 0f, 0f, 0.45f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 1f); bgRt.anchorMax = new Vector2(0.5f, 1f); bgRt.pivot = new Vector2(0.5f, 1f);
        bgRt.anchoredPosition = new Vector2(0, -14); bgRt.sizeDelta = new Vector2(230, 88);

        var t = MkTMP(cvGO, "TimerText", "05:00", 64, bangers, Color.white, Vector2.zero, new Vector2(320, 90));
        var trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 1f); trt.anchorMax = new Vector2(0.5f, 1f); trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0, -10);

        gm.timerText = t.GetComponent<TMP_Text>();
        gm.roundDuration = 300f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✅ Časovač pridaný do Office (05:00, odpočítava hore v strede).");
    }

    // ===================== KNIŽNICE ROZBÍJATEĽNÉ =====================
    [MenuItem("BreakRoom/Make Bookshelves Breakable")]
    public static void MakeBookshelvesBreakable()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Office.unity");
        int added = 0, cols = 0;
        var all = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        foreach (var tr in all)
        {
            if (!tr.name.StartsWith("Shelf_")) continue;
            var go = tr.gameObject;
            if (go.GetComponent<MeshRenderer>() == null) continue;
            if (go.GetComponent<Breakable>() != null) continue;
            if (go.GetComponent<Collider>() == null) { go.AddComponent<BoxCollider>(); cols++; }
            var b = go.AddComponent<Breakable>();
            b.hp = 4; b.damage = 1; b.xpValue = 8; b.fragmentCount = 6;
            added++;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"✅ Knižnice: pridané Breakable na {added} dielov (+{cols} colliderov).");
    }

    // ===================== HELPERY =====================
    static void EnsureEventSystem()
    {
        var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            return;
        }
        // odstráň starý StandaloneInputModule (nefunguje s novým Input Systemom)
        var old = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (old != null) Object.DestroyImmediate(old);
        if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    static GameObject MkTMP(GameObject parent, string name, string txt, int size,
        TMP_FontAsset font, Color col, Vector2 pos, Vector2 sizeD)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = sizeD;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = txt; t.fontSize = size; t.color = col; t.alignment = TextAlignmentOptions.Center;
        if (font != null) t.font = font;
        return go;
    }

    static GameObject MkButton(GameObject parent, string name, string label, TMP_FontAsset font,
        Color col, Vector2 pos, Vector2 sizeD)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = col;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = sizeD;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var lbl = MkTMP(go, "Label", label, 40, font, new Color(1f, 0.85f, 0.1f), Vector2.zero, sizeD);
        var lr = lbl.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
        return go;
    }
}
