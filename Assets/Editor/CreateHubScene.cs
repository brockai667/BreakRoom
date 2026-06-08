using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;

/// Postaví Hub (lobby) scénu v štýle One Armed Robber: 3D izba ako pozadie,
/// horné taby Play/Loadout/Shop, peniaze vpravo hore, výber mapy, START,
/// loadout zbraní a panel s animáciou počítania peňazí.
public class CreateHubScene
{
    [MenuItem("BreakRoom/Create Hub Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---------- SVETLO ----------
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.15f;
        light.color = new Color(1f, 0.96f, 0.9f);
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // ---------- KAMERA ----------
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.05f, 0.08f);
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 2.6f, -6.6f);
        camGO.transform.rotation = Quaternion.Euler(10, 0, 0);
        camGO.tag = "MainCamera";

        // ---------- 3D POZADIE (izba) ----------
        MkBox("Podlaha",     new Vector3(0, -0.5f, 0),  new Vector3(20, 1, 20),    new Color(0.30f, 0.32f, 0.38f));
        MkBox("Stena_Zadna", new Vector3(0, 3, 6),      new Vector3(20, 8, 0.4f),  new Color(0.40f, 0.42f, 0.48f));
        MkBox("Stena_Lava",  new Vector3(-8, 3, 0),     new Vector3(0.4f, 8, 20),  new Color(0.36f, 0.38f, 0.44f));
        MkBox("Stena_Prava", new Vector3(8, 3, 0),      new Vector3(0.4f, 8, 20),  new Color(0.36f, 0.38f, 0.44f));
        MkBox("Stage",       new Vector3(0, 0.1f, 0),   new Vector3(3.4f, 0.2f, 3.4f), new Color(0.5f, 0.2f, 0.08f));
        MkBox("Stol",        new Vector3(0, 0.85f, 0.2f), new Vector3(2.4f, 0.16f, 1.1f), new Color(0.5f, 0.33f, 0.15f));
        MkBox("Monitor",     new Vector3(0, 1.35f, 0.55f), new Vector3(0.9f, 0.55f, 0.08f), new Color(0.07f, 0.07f, 0.09f));
        MkBox("PC",          new Vector3(0.85f, 1.1f, 0.4f), new Vector3(0.3f, 0.6f, 0.5f), new Color(0.12f, 0.12f, 0.14f));

        // Pódium pre 3D náhľad zbrane (vpravo v izbe, viditeľné popri shop mriežke)
        MkBox("PodiumBase",  new Vector3(2.7f, 0.15f, 0.6f), new Vector3(1.3f, 0.3f, 1.3f), new Color(0.45f, 0.18f, 0.07f));
        var previewGO = new GameObject("WeaponPreview");
        previewGO.transform.position = new Vector3(2.7f, 1.45f, 0.6f);
        previewGO.transform.localScale = Vector3.one * 1.3f;
        previewGO.AddComponent<WeaponPreview>();

        // ---------- CANVAS ----------
        var cvGO = new GameObject("UICanvas");
        var cv = cvGO.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cvGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cvGO.AddComponent<GraphicRaycaster>();

        // Horná lišta
        var topBar = MkImg(cvGO, "TopBar", new Color(0.1f, 0.06f, 0.03f, 0.92f));
        var tbr = topBar.GetComponent<RectTransform>();
        tbr.anchorMin = new Vector2(0, 1); tbr.anchorMax = new Vector2(1, 1); tbr.pivot = new Vector2(0.5f, 1);
        tbr.sizeDelta = new Vector2(0, 90); tbr.anchoredPosition = Vector2.zero;

        // Peniaze vpravo hore
        var moneyGO = MkTxt(cvGO, "MoneyText", "$0", 40, FontStyle.Bold,
            new Color(0.2f, 0.9f, 0.3f), TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(1, 1));
        var mrt = moneyGO.GetComponent<RectTransform>();
        mrt.pivot = new Vector2(1, 1); mrt.anchoredPosition = new Vector2(-40, -20); mrt.sizeDelta = new Vector2(320, 56);

        // Späť do hlavného menu
        var backBtn = MkBtn(cvGO, "BackBtn", "← MENU", 18);
        var brt = backBtn.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0, 1); brt.anchorMax = new Vector2(0, 1); brt.pivot = new Vector2(0, 1);
        brt.anchoredPosition = new Vector2(20, -20); brt.sizeDelta = new Vector2(150, 50);

        // Taby
        var playTab    = MkBtn(cvGO, "PlayTab",    "PLAY", 22);    PlaceTab(playTab,    195);
        var loadoutTab = MkBtn(cvGO, "LoadoutTab", "LOADOUT", 22); PlaceTab(loadoutTab, 395);
        var shopTab    = MkBtn(cvGO, "ShopTab",    "SHOP", 22);    PlaceTab(shopTab,    595);

        // ---------- PLAY PANEL ----------
        var playPanel = MkPanel(cvGO, "PlayPanel");
        var title = MkTxt(playPanel, "Title", "PRIPRAV SA NA NIČENIE", 54, FontStyle.Bold,
            new Color(1f, 0.85f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var titleRt = title.GetComponent<RectTransform>();
        titleRt.pivot = new Vector2(0.5f, 1); titleRt.anchoredPosition = new Vector2(0, -40); titleRt.sizeDelta = new Vector2(1200, 80);

        // Výber mapy (vľavo dole)
        var mapBtn = MkBtn(playPanel, "MapBtn", "VYBRAŤ MAPU", 24);
        var mapRt = mapBtn.GetComponent<RectTransform>();
        mapRt.anchorMin = new Vector2(0, 0); mapRt.anchorMax = new Vector2(0, 0); mapRt.pivot = new Vector2(0, 0);
        mapRt.anchoredPosition = new Vector2(40, 40); mapRt.sizeDelta = new Vector2(280, 64);

        var mapLabel = MkTxt(playPanel, "MapLabel", "Vybraná mapa: Office", 22, FontStyle.Bold,
            Color.white, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(0, 0));
        var mlRt = mapLabel.GetComponent<RectTransform>();
        mlRt.pivot = new Vector2(0, 0); mlRt.anchoredPosition = new Vector2(40, 116); mlRt.sizeDelta = new Vector2(420, 32);

        // START (vpravo dole)
        var startBtn = MkBtn(playPanel, "StartBtn", "START", 34, new Color(0.1f, 0.45f, 0.12f));
        var stRt = startBtn.GetComponent<RectTransform>();
        stRt.anchorMin = new Vector2(1, 0); stRt.anchorMax = new Vector2(1, 0); stRt.pivot = new Vector2(1, 0);
        stRt.anchoredPosition = new Vector2(-40, 40); stRt.sizeDelta = new Vector2(320, 90);

        // ---------- LOADOUT PANEL ----------
        var loadoutPanel = MkPanel(cvGO, "LoadoutPanel");
        var loTitle = MkTxt(loadoutPanel, "LoTitle", "LOADOUT — vyber si zbraň", 40, FontStyle.Bold,
            new Color(1f, 0.85f, 0.1f), TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var loTrt = loTitle.GetComponent<RectTransform>();
        loTrt.pivot = new Vector2(0.5f, 1); loTrt.anchoredPosition = new Vector2(0, -30); loTrt.sizeDelta = new Vector2(900, 56);

        // ScrollView vertikálny
        var loScroll = new GameObject("LoadoutScroll"); loScroll.transform.SetParent(loadoutPanel.transform, false);
        var loSR = loScroll.AddComponent<ScrollRect>();
        var loSRt = loScroll.GetComponent<RectTransform>();
        loSRt.anchorMin = new Vector2(0.5f, 0); loSRt.anchorMax = new Vector2(0.5f, 1); loSRt.pivot = new Vector2(0.5f, 1);
        loSRt.sizeDelta = new Vector2(560, -120); loSRt.anchoredPosition = new Vector2(0, -100);
        loSR.horizontal = false; loSR.vertical = true;

        var loVp = new GameObject("Viewport"); loVp.transform.SetParent(loScroll.transform, false);
        loVp.AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);
        var loMask = loVp.AddComponent<Mask>(); loMask.showMaskGraphic = true;
        var loVpRt = loVp.GetComponent<RectTransform>();
        loVpRt.anchorMin = Vector2.zero; loVpRt.anchorMax = Vector2.one; loVpRt.offsetMin = loVpRt.offsetMax = Vector2.zero;
        loSR.viewport = loVpRt;

        var loContent = new GameObject("LoadoutContainer"); loContent.transform.SetParent(loVp.transform, false);
        var loCRt = loContent.AddComponent<RectTransform>();
        loCRt.anchorMin = new Vector2(0, 1); loCRt.anchorMax = new Vector2(1, 1); loCRt.pivot = new Vector2(0.5f, 1);
        loCRt.anchoredPosition = Vector2.zero; loCRt.sizeDelta = new Vector2(0, 0);
        var vlg = loContent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12; vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        var csf = loContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        loSR.content = loCRt;

        // ---------- SHOP PANEL (reuse ShopManager) ----------
        var shopPanel = MkPanel(cvGO, "ShopPanel");
        var shTitle = MkTxt(shopPanel, "ShTitle", "SHOP — kúp si zbrane", 40, FontStyle.Bold,
            new Color(1f, 0.85f, 0.1f), TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        var shTrt = shTitle.GetComponent<RectTransform>();
        shTrt.pivot = new Vector2(0.5f, 1); shTrt.anchoredPosition = new Vector2(0, -30); shTrt.sizeDelta = new Vector2(900, 56);

        // Vertikálny scroll s mriežkou kariet — vľavo (vpravo ostáva pódium s náhľadom)
        var shScroll = new GameObject("ShopScroll"); shScroll.transform.SetParent(shopPanel.transform, false);
        var shSR = shScroll.AddComponent<ScrollRect>();
        var shSRt = shScroll.GetComponent<RectTransform>();
        shSRt.anchorMin = new Vector2(0.02f, 0.05f); shSRt.anchorMax = new Vector2(0.60f, 0.86f);
        shSRt.offsetMin = shSRt.offsetMax = Vector2.zero;
        shSR.horizontal = false; shSR.vertical = true; shSR.scrollSensitivity = 25;

        var shVp = new GameObject("Viewport"); shVp.transform.SetParent(shScroll.transform, false);
        var shVpRt = shVp.AddComponent<RectTransform>();
        shVp.AddComponent<RectMask2D>();
        shVpRt.anchorMin = Vector2.zero; shVpRt.anchorMax = Vector2.one; shVpRt.offsetMin = shVpRt.offsetMax = Vector2.zero;
        shSR.viewport = shVpRt;

        var shContent = new GameObject("CardContainer"); shContent.transform.SetParent(shVp.transform, false);
        var shCRt = shContent.AddComponent<RectTransform>();
        shCRt.anchorMin = new Vector2(0, 1); shCRt.anchorMax = new Vector2(1, 1); shCRt.pivot = new Vector2(0.5f, 1);
        shCRt.anchoredPosition = Vector2.zero; shCRt.sizeDelta = Vector2.zero;
        var grid = shContent.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(205, 275);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperLeft;
        var shCsf = shContent.AddComponent<ContentSizeFitter>();
        shCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        shSR.content = shCRt;

        var smGO = new GameObject("ShopManager");
        var sm = smGO.AddComponent<ShopManager>();
        sm.cardContainer = shCRt;
        sm.moneyText = moneyGO.GetComponent<Text>();

        // ---------- RESULTS PANEL (animácia peňazí) ----------
        var resultsPanel = MkImg(cvGO, "ResultsPanel", new Color(0.06f, 0.04f, 0.02f, 0.9f));
        var rpRt = resultsPanel.GetComponent<RectTransform>();
        rpRt.anchorMin = new Vector2(0.5f, 0.5f); rpRt.anchorMax = new Vector2(0.5f, 0.5f); rpRt.pivot = new Vector2(0.5f, 0.5f);
        rpRt.sizeDelta = new Vector2(640, 320); rpRt.anchoredPosition = new Vector2(0, 40);

        var rTitle = MkTxt(resultsPanel, "RTitle", "KOLO UKONČENÉ", 40, FontStyle.Bold,
            new Color(1f, 0.85f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        rTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
        rTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 60);

        var resultsText = MkTxt(resultsPanel, "ResultsText", "Rozbité: 0   Čas: 00:00", 26, FontStyle.Normal,
            Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        resultsText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
        resultsText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);

        var earnedFly = MkTxt(resultsPanel, "EarnedFly", "", 56, FontStyle.Bold,
            new Color(0.2f, 0.95f, 0.35f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        earnedFly.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
        earnedFly.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
        earnedFly.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);

        // ---------- HUB MANAGER ----------
        var hubGO = new GameObject("HubManager");
        var hub = hubGO.AddComponent<HubManager>();
        hub.moneyText        = moneyGO.GetComponent<Text>();
        hub.playTab          = playTab.GetComponent<Button>();
        hub.loadoutTab       = loadoutTab.GetComponent<Button>();
        hub.shopTab          = shopTab.GetComponent<Button>();
        hub.playPanel        = playPanel;
        hub.loadoutPanel     = loadoutPanel;
        hub.shopPanel        = shopPanel;
        hub.mapLabel         = mapLabel.GetComponent<Text>();
        hub.loadoutContainer = loContent.transform;
        hub.resultsPanel     = resultsPanel;
        hub.resultsText      = resultsText.GetComponent<Text>();
        hub.earnedFlyText    = earnedFly.GetComponent<Text>();

        // Wire tlačidlá (perzistentné listenery)
        UnityEventTools.AddPersistentListener(playTab.GetComponent<Button>().onClick,    hub.ShowPlay);
        UnityEventTools.AddPersistentListener(loadoutTab.GetComponent<Button>().onClick, hub.ShowLoadout);
        UnityEventTools.AddPersistentListener(shopTab.GetComponent<Button>().onClick,    hub.ShowShop);
        UnityEventTools.AddPersistentListener(backBtn.GetComponent<Button>().onClick,    hub.BackToMainMenu);
        UnityEventTools.AddPersistentListener(mapBtn.GetComponent<Button>().onClick,     hub.SelectOffice);
        UnityEventTools.AddPersistentListener(startBtn.GetComponent<Button>().onClick,   hub.StartGame);

        // PlayerInventory
        new GameObject("PlayerInventory").AddComponent<PlayerInventory>();

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ---------- ULOŽ ----------
        string path = "Assets/Scenes/Hub.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("✅ Hub scéna vytvorená: " + path);

        var bsc = EditorBuildSettings.scenes;
        if (!System.Array.Exists(bsc, s => s.path == path))
        {
            var ns = new EditorBuildSettingsScene[bsc.Length + 1];
            System.Array.Copy(bsc, ns, bsc.Length);
            ns[bsc.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = ns;
        }
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(path);
    }

    // ---------- HELPERY ----------
    static void PlaceTab(GameObject btn, float x)
    {
        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -20); rt.sizeDelta = new Vector2(185, 50);
    }

    static GameObject MkBox(string name, Vector3 pos, Vector3 scale, Color col)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.position = pos; go.transform.localScale = scale;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Standard"));
        mat.color = col;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject MkImg(GameObject parent, string name, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        go.AddComponent<Image>().color = col;
        return go;
    }

    // Priehľadný panel, ktorý vyplní obrazovku pod hornou lištou (90px)
    static GameObject MkPanel(GameObject parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = new Vector2(0, -90);
        return go;
    }

    static GameObject MkTxt(GameObject parent, string name, string txt, int size,
        FontStyle style, Color col, TextAnchor anchor, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>(); r.anchorMin = aMin; r.anchorMax = aMax;
        var t = go.AddComponent<Text>(); t.text = txt; t.fontSize = size;
        t.fontStyle = style; t.color = col; t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static GameObject MkBtn(GameObject parent, string name, string label, int fontSize)
        => MkBtn(parent, name, label, fontSize, new Color(0.18f, 0.10f, 0.05f));

    static GameObject MkBtn(GameObject parent, string name, string label, int fontSize, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = col;
        go.AddComponent<RectTransform>();
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var lbl = new GameObject("Label"); lbl.transform.SetParent(go.transform, false);
        var lr = lbl.AddComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
        var lt = lbl.AddComponent<Text>(); lt.text = label; lt.fontSize = fontSize;
        lt.fontStyle = FontStyle.Bold; lt.color = new Color(1f, 0.85f, 0.1f); lt.alignment = TextAnchor.MiddleCenter;
        lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }
}
