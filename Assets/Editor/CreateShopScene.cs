using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateShopScene
{
    [MenuItem("BreakRoom/Create Shop Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera s tmavým pozadím
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.backgroundColor = new Color(0.12f, 0.08f, 0.12f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.AddComponent<AudioListener>();

        // ---- HLAVNÝ CANVAS ----
        var cvGO = new GameObject("Canvas");
        var cv = cvGO.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cvGO.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
            UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ((UnityEngine.UI.CanvasScaler)cvGO.GetComponent<UnityEngine.UI.CanvasScaler>()).referenceResolution =
            new Vector2(1920, 1080);
        cvGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Tmavé pozadie
        var bg = MkImg(cvGO, "BG", new Color(0.1f, 0.06f, 0.03f, 1f));
        Stretch(bg.GetComponent<RectTransform>());

        // Tehlový overlay (semi-transparent)
        var brickOverlay = MkImg(cvGO, "BrickOverlay", new Color(0.18f, 0.10f, 0.05f, 0.7f));
        Stretch(brickOverlay.GetComponent<RectTransform>());

        // Nadpis SHOP
        var title = MkTxt(cvGO, "TitleText", "SHOP", 96, FontStyle.Bold,
            new Color(1f, 0.85f, 0.1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
        title.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 110);

        // Money display
        var moneyGO = MkTxt(cvGO, "MoneyText", "$0", 38, FontStyle.Bold,
            new Color(0.2f, 0.9f, 0.3f), TextAnchor.MiddleRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f));
        moneyGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, -60);
        moneyGO.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 60);

        // ---- SCROLL VIEW pre karty ----
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(cvGO.transform, false);
        var scrollRect = scrollGO.AddComponent<UnityEngine.UI.ScrollRect>();
        var scrollRectT = scrollGO.GetComponent<RectTransform>();
        scrollRectT.anchorMin = new Vector2(0.02f, 0.08f);
        scrollRectT.anchorMax = new Vector2(0.98f, 0.88f);
        scrollRectT.offsetMin = scrollRectT.offsetMax = Vector2.zero;
        scrollRect.horizontal = true; scrollRect.vertical = false;

        // Viewport
        var vpGO = new GameObject("Viewport"); vpGO.transform.SetParent(scrollGO.transform, false);
        vpGO.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        var vpMask = vpGO.AddComponent<UnityEngine.UI.Mask>(); vpMask.showMaskGraphic = false;
        var vpRect = vpGO.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero; vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = vpRect.offsetMax = Vector2.zero;
        scrollRect.viewport = vpRect;

        // Content (container pre karty)
        var contentGO = new GameObject("CardContainer"); contentGO.transform.SetParent(vpGO.transform, false);
        var contentRect = contentGO.GetComponent<RectTransform>();
        if (contentRect == null) contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0); contentRect.anchorMax = new Vector2(0, 1);
        contentRect.pivot = new Vector2(0, 0.5f);
        contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;
        contentRect.sizeDelta = new Vector2(WeaponData.All.Length * 230f, 0);
        var hlg = contentGO.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hlg.spacing = 30; hlg.padding = new RectOffset(30,30,20,20);
        hlg.childForceExpandHeight = true;
        scrollRect.content = contentRect;

        // BACK tlačidlo
        var backBtn = MkButton(cvGO, "BackBtn", "← MENU", new Color(0.18f, 0.09f, 0.04f));
        var backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0,0); backRect.anchorMax = new Vector2(0,0);
        backRect.pivot = new Vector2(0,0);
        backRect.anchoredPosition = new Vector2(30, 25);
        backRect.sizeDelta = new Vector2(200, 55);

        // ShopManager
        var smGO = new GameObject("ShopManager");
        var sm = smGO.AddComponent<ShopManager>();
        sm.cardContainer = contentRect;
        sm.moneyText = moneyGO.GetComponent<UnityEngine.UI.Text>();
        backBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(sm.GoBack);

        // PlayerInventory (singleton)
        new GameObject("PlayerInventory").AddComponent<PlayerInventory>();

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        string path = "Assets/Scenes/Shop.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("✅ Shop scéna vytvorená: " + path);

        var bsc = EditorBuildSettings.scenes;
        bool ex = System.Array.Exists(bsc, s => s.path == path);
        if (!ex) {
            var ns = new EditorBuildSettingsScene[bsc.Length + 1];
            System.Array.Copy(bsc, ns, bsc.Length);
            ns[bsc.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = ns;
        }
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(path);
    }

    static GameObject MkImg(GameObject parent, string name, Color col) {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        go.AddComponent<UnityEngine.UI.Image>().color = col;
        go.AddComponent<RectTransform>();
        return go;
    }

    static void Stretch(RectTransform r) {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    static GameObject MkTxt(GameObject parent, string name, string txt, int size,
        FontStyle style, Color col, TextAnchor anchor, Vector2 aMin, Vector2 aMax) {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>(); r.anchorMin = aMin; r.anchorMax = aMax;
        var t = go.AddComponent<UnityEngine.UI.Text>(); t.text = txt; t.fontSize = size;
        t.fontStyle = style; t.color = col; t.alignment = anchor;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static GameObject MkButton(GameObject parent, string name, string label, Color col) {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<UnityEngine.UI.Image>(); img.color = col;
        go.AddComponent<RectTransform>();
        var btn = go.AddComponent<UnityEngine.UI.Button>(); btn.targetGraphic = img;
        var lbl = new GameObject("Label"); lbl.transform.SetParent(go.transform, false);
        var lr = lbl.AddComponent<RectTransform>(); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
        var lt = lbl.AddComponent<UnityEngine.UI.Text>(); lt.text = label; lt.fontSize = 22;
        lt.fontStyle = FontStyle.Bold; lt.color = new Color(1f,0.85f,0.1f); lt.alignment = TextAnchor.MiddleCenter;
        lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }
}
