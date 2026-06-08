using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;

/// Postaví druhú mapu "Obyvacka" (obývačka) — miestnosť s rozbíjateľným nábytkom
/// a kompletnými hernými systémami (hráč, časovač, pauza, crosshair, ruka, peniaze).
public class CreateLivingRoomScene
{
    const string BANGERS_GUID = "f62de6debf194a140b0eab5be7f29d66";

    [MenuItem("BreakRoom/Create Living Room Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bangers = LoadBangers();

        // ---------- SVETLO ----------
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.05f; light.color = new Color(1f, 0.95f, 0.85f);
        lightGO.transform.rotation = Quaternion.Euler(50, 40, 0);
        RenderSettings.ambientLight = new Color(0.45f, 0.43f, 0.4f);

        // ---------- MIESTNOSŤ ----------
        Color floorCol = new Color(0.45f, 0.30f, 0.16f);   // drevo
        Color wallCol  = new Color(0.80f, 0.74f, 0.62f);   // teplá béžová
        Color ceilCol  = new Color(0.88f, 0.86f, 0.82f);
        Prop("Podlaha",     PrimitiveType.Cube, new Vector3(0, -0.25f, 0), new Vector3(12, 0.5f, 12), floorCol, 0, false);
        Prop("Stena_Zadna", PrimitiveType.Cube, new Vector3(0, 2.5f, 6),   new Vector3(12, 5, 0.3f), wallCol, 0, false);
        Prop("Stena_Predna",PrimitiveType.Cube, new Vector3(0, 2.5f, -6),  new Vector3(12, 5, 0.3f), wallCol, 0, false);
        Prop("Stena_Lava",  PrimitiveType.Cube, new Vector3(-6, 2.5f, 0),  new Vector3(0.3f, 5, 12), wallCol, 0, false);
        Prop("Stena_Prava", PrimitiveType.Cube, new Vector3(6, 2.5f, 0),   new Vector3(0.3f, 5, 12), wallCol, 0, false);
        Prop("Strop",       PrimitiveType.Cube, new Vector3(0, 5, 0),      new Vector3(12, 0.3f, 12), ceilCol, 0, false);
        // koberec (nerozbitný)
        Prop("Koberec", PrimitiveType.Cube, new Vector3(0, 0.02f, 1.2f), new Vector3(4.2f, 0.04f, 3f), new Color(0.5f, 0.18f, 0.18f), 0, false);

        // ---------- NÁBYTOK (rozbíjateľný) ----------
        Color sofa = new Color(0.30f, 0.42f, 0.46f);
        // Gauč pri zadnej stene (čelom do miestnosti, -z)
        Prop("Gauc_base",  PrimitiveType.Cube, new Vector3(0, 0.45f, 4.6f), new Vector3(3.4f, 0.5f, 1.2f), sofa, 5);
        Prop("Gauc_chrbat",PrimitiveType.Cube, new Vector3(0, 0.95f, 5.15f),new Vector3(3.4f, 0.9f, 0.3f), sofa, 5);
        Prop("Gauc_armL",  PrimitiveType.Cube, new Vector3(-1.85f, 0.7f, 4.6f), new Vector3(0.3f, 0.8f, 1.2f), sofa, 4);
        Prop("Gauc_armR",  PrimitiveType.Cube, new Vector3( 1.85f, 0.7f, 4.6f), new Vector3(0.3f, 0.8f, 1.2f), sofa, 4);
        Prop("Vankus1", PrimitiveType.Cube, new Vector3(-1f, 0.78f, 4.6f), new Vector3(0.9f, 0.25f, 1f), new Color(0.7f,0.3f,0.3f), 2);
        Prop("Vankus2", PrimitiveType.Cube, new Vector3( 1f, 0.78f, 4.6f), new Vector3(0.9f, 0.25f, 1f), new Color(0.7f,0.55f,0.25f), 2);

        // Konferenčný stolík v strede
        Color wood = new Color(0.5f, 0.33f, 0.16f);
        Prop("Stolik_top", PrimitiveType.Cube, new Vector3(0, 0.5f, 1.3f), new Vector3(1.8f, 0.12f, 0.9f), wood, 4);
        Prop("Stolik_n1", PrimitiveType.Cube, new Vector3(-0.8f,0.25f,0.9f), new Vector3(0.12f,0.5f,0.12f), wood, 3);
        Prop("Stolik_n2", PrimitiveType.Cube, new Vector3( 0.8f,0.25f,0.9f), new Vector3(0.12f,0.5f,0.12f), wood, 3);
        Prop("Stolik_n3", PrimitiveType.Cube, new Vector3(-0.8f,0.25f,1.7f), new Vector3(0.12f,0.5f,0.12f), wood, 3);
        Prop("Stolik_n4", PrimitiveType.Cube, new Vector3( 0.8f,0.25f,1.7f), new Vector3(0.12f,0.5f,0.12f), wood, 3);
        // veci na stolíku
        Prop("Salka1", PrimitiveType.Cylinder, new Vector3(-0.4f,0.66f,1.2f), new Vector3(0.18f,0.08f,0.18f), new Color(0.9f,0.9f,0.95f), 1);
        Prop("Salka2", PrimitiveType.Cylinder, new Vector3( 0.4f,0.66f,1.5f), new Vector3(0.18f,0.08f,0.18f), new Color(0.85f,0.3f,0.25f), 1);
        Prop("Kniha_s", PrimitiveType.Cube, new Vector3(0.1f,0.62f,1.0f), new Vector3(0.4f,0.06f,0.28f), new Color(0.2f,0.4f,0.7f), 1);

        // TV zostava pri prednej stene (čelom +z na gauč)
        Prop("TV_skrinka", PrimitiveType.Cube, new Vector3(0, 0.4f, -5.4f), new Vector3(2.6f, 0.8f, 0.5f), new Color(0.28f,0.2f,0.12f), 5);
        Prop("TV_obrazovka", PrimitiveType.Cube, new Vector3(0, 1.55f, -5.5f), new Vector3(2.2f, 1.2f, 0.12f), new Color(0.05f,0.05f,0.07f), 4);
        Prop("TV_noha", PrimitiveType.Cube, new Vector3(0, 0.95f, -5.5f), new Vector3(0.2f, 0.3f, 0.15f), new Color(0.1f,0.1f,0.12f), 2);
        Prop("Konzola", PrimitiveType.Cube, new Vector3(0.7f,0.85f,-5.4f), new Vector3(0.5f,0.1f,0.35f), new Color(0.12f,0.12f,0.14f), 2);

        // Knižnica pri ľavej stene
        Color shelfCol = new Color(0.45f, 0.3f, 0.15f);
        Prop("Kniznica_ramec", PrimitiveType.Cube, new Vector3(-5.4f, 1.4f, -1.5f), new Vector3(0.4f, 2.8f, 2.4f), shelfCol, 6);
        for (int i = 0; i < 3; i++)
            Prop("Kniznica_polica" + i, PrimitiveType.Cube, new Vector3(-5.4f, 0.6f + i * 0.85f, -1.5f), new Vector3(0.42f, 0.08f, 2.3f), shelfCol, 3);
        Color[] bookCols = { new Color(0.7f,0.2f,0.2f), new Color(0.2f,0.6f,0.3f), new Color(0.25f,0.4f,0.8f), new Color(0.8f,0.7f,0.2f), new Color(0.6f,0.3f,0.7f) };
        for (int row = 0; row < 3; row++)
            for (int b = 0; b < 5; b++)
                Prop($"Kniha_{row}_{b}", PrimitiveType.Cube,
                    new Vector3(-5.35f, 0.85f + row * 0.85f, -2.4f + b * 0.45f),
                    new Vector3(0.28f, 0.5f, 0.32f), bookCols[(row + b) % bookCols.Length], 1);

        // Stojaca lampa v rohu
        Prop("Lampa_pat", PrimitiveType.Cylinder, new Vector3(4.8f, 0.1f, 4.8f), new Vector3(0.5f, 0.08f, 0.5f), new Color(0.2f,0.2f,0.22f), 2);
        Prop("Lampa_tyc", PrimitiveType.Cylinder, new Vector3(4.8f, 1.1f, 4.8f), new Vector3(0.08f, 1f, 0.08f), new Color(0.25f,0.25f,0.27f), 2);
        Prop("Lampa_tienidlo", PrimitiveType.Cylinder, new Vector3(4.8f, 2.05f, 4.8f), new Vector3(0.6f, 0.3f, 0.6f), new Color(0.9f,0.85f,0.6f), 2);

        // Bočný stolík + váza
        Prop("BocnyStolik", PrimitiveType.Cube, new Vector3(2.6f, 0.4f, 4.4f), new Vector3(0.7f, 0.8f, 0.7f), wood, 4);
        Prop("Vaza", PrimitiveType.Cylinder, new Vector3(2.6f, 1.0f, 4.4f), new Vector3(0.3f, 0.25f, 0.3f), new Color(0.3f,0.55f,0.6f), 1);

        // Rastlina v rohu
        Prop("Kvetinac", PrimitiveType.Cylinder, new Vector3(-4.8f, 0.35f, 4.8f), new Vector3(0.5f, 0.35f, 0.5f), new Color(0.5f,0.3f,0.2f), 3);
        Prop("Rastlina", PrimitiveType.Sphere, new Vector3(-4.8f, 1.1f, 4.8f), new Vector3(0.9f, 1.1f, 0.9f), new Color(0.2f,0.5f,0.2f), 2);

        // Obrazy na stene
        Prop("Obraz1", PrimitiveType.Cube, new Vector3(-2f, 3f, 5.85f), new Vector3(1.2f, 0.9f, 0.08f), new Color(0.3f,0.35f,0.5f), 2);
        Prop("Obraz2", PrimitiveType.Cube, new Vector3( 2f, 3f, 5.85f), new Vector3(1.2f, 0.9f, 0.08f), new Color(0.5f,0.35f,0.3f), 2);

        // ---------- HRÁČ ----------
        int playerLayer = LayerMask.NameToLayer("Player"); if (playerLayer < 0) playerLayer = 3;
        var player = new GameObject("Player");
        player.layer = playerLayer;
        player.transform.position = new Vector3(0, 1, -3);
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.35f; cc.center = Vector3.zero; cc.slopeLimit = 45; cc.stepOffset = 0.45f; cc.skinWidth = 0.08f;
        player.AddComponent<PlayerController>();

        var camGO = new GameObject("Main Camera");
        camGO.transform.SetParent(player.transform, false);
        camGO.transform.localPosition = new Vector3(0, 0.7f, 0);
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera";

        var wh = player.AddComponent<WeaponHit>();
        wh.playerCamera = cam; wh.hitDistance = 4.5f;

        // ---------- PLAYER INVENTORY + XP ----------
        new GameObject("PlayerInventory").AddComponent<PlayerInventory>();
        new GameObject("XPManager").AddComponent<XPManager>();   // ničenie -> XP -> level (odomykanie máp)

        // ---------- CROSSHAIR ----------
        var cross = new GameObject("Crosshair");
        cross.AddComponent<CrosshairUI>();

        // ---------- HUD CANVAS (ruka) ----------
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var csc = canvasGO.AddComponent<CanvasScaler>(); csc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; csc.referenceResolution = new Vector2(1920,1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        BuildHandDisplay(canvas);

        // ---------- GAME MANAGER + ČASOVAČ ----------
        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        gm.roundDuration = 300f;

        var timerCv = new GameObject("TimerCanvas");
        var tcv = timerCv.AddComponent<Canvas>(); tcv.renderMode = RenderMode.ScreenSpaceOverlay; tcv.sortingOrder = 50;
        var tsc = timerCv.AddComponent<CanvasScaler>(); tsc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; tsc.referenceResolution = new Vector2(1920,1080);
        timerCv.AddComponent<GraphicRaycaster>();
        var tbg = new GameObject("TimerBG"); tbg.transform.SetParent(timerCv.transform, false);
        var tbgImg = tbg.AddComponent<Image>(); tbgImg.color = new Color(0,0,0,0.45f);
        var tbgRt = tbg.GetComponent<RectTransform>(); tbgRt.anchorMin = new Vector2(0.5f,1); tbgRt.anchorMax = new Vector2(0.5f,1); tbgRt.pivot = new Vector2(0.5f,1); tbgRt.anchoredPosition = new Vector2(0,-14); tbgRt.sizeDelta = new Vector2(230,88);
        var timerT = MkTMP(timerCv, "TimerText", "05:00", 64, bangers, Color.white, new Vector2(0,-10), new Vector2(320,90));
        var ttRt = timerT.GetComponent<RectTransform>(); ttRt.anchorMin = new Vector2(0.5f,1); ttRt.anchorMax = new Vector2(0.5f,1); ttRt.pivot = new Vector2(0.5f,1);
        gm.timerText = timerT.GetComponent<TMP_Text>();

        // ---------- PAUSE MENU ----------
        var pm = new GameObject("PauseMenuController").AddComponent<PauseMenu>();
        var pcv = new GameObject("PauseCanvas");
        var pcanvas = pcv.AddComponent<Canvas>(); pcanvas.renderMode = RenderMode.ScreenSpaceOverlay; pcanvas.sortingOrder = 200;
        var psc = pcv.AddComponent<CanvasScaler>(); psc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; psc.referenceResolution = new Vector2(1920,1080);
        pcv.AddComponent<GraphicRaycaster>();
        var panel = new GameObject("PausePanel"); panel.transform.SetParent(pcv.transform, false);
        var pImg = panel.AddComponent<Image>(); pImg.color = new Color(0,0,0,0.78f);
        var pRt = panel.GetComponent<RectTransform>(); pRt.anchorMin = Vector2.zero; pRt.anchorMax = Vector2.one; pRt.offsetMin = pRt.offsetMax = Vector2.zero;
        MkTMP(panel, "PauseTitle", "PAUSED", 96, bangers, new Color(1f,0.85f,0.1f), new Vector2(0,200), new Vector2(700,130));
        var resume = MkButton(panel, "ResumeButton", "RESUME", bangers, new Color(0.17f,0.1f,0.05f), new Vector2(0,30), new Vector2(380,90));
        UnityEventTools.AddPersistentListener(resume.GetComponent<Button>().onClick, pm.Resume);
        var quit = MkButton(panel, "QuitButton", "QUIT", bangers, new Color(0.45f,0.1f,0.05f), new Vector2(0,-90), new Vector2(380,90));
        UnityEventTools.AddPersistentListener(quit.GetComponent<Button>().onClick, pm.GoToMainMenu);
        pm.pausePanel = panel;

        // ---------- EVENT SYSTEM ----------
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ---------- ULOŽ + BUILD SETTINGS ----------
        string path = "Assets/Scenes/Obyvacka.unity";
        EditorSceneManager.SaveScene(scene, path);
        var bsc = EditorBuildSettings.scenes;
        if (!System.Array.Exists(bsc, s => s.path == path))
        {
            var ns = new EditorBuildSettingsScene[bsc.Length + 1];
            System.Array.Copy(bsc, ns, bsc.Length);
            ns[bsc.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = ns;
        }
        AssetDatabase.Refresh();
        Debug.Log("✅ Obývačka vytvorená: " + path + " (pridaná do Build Settings, mapa 'Obyvacka').");
    }

    // ---------- HELPERY ----------
    static GameObject Prop(string name, PrimitiveType t, Vector3 pos, Vector3 scale, Color col, int hp, bool breakable = true)
    {
        var g = GameObject.CreatePrimitive(t);
        g.name = name; g.transform.position = pos; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mat(col);
        if (breakable)
        {
            var b = g.AddComponent<Breakable>();
            b.hp = Mathf.Max(1, hp); b.damage = 1; b.xpValue = 10; b.fragmentCount = 7;
        }
        return g;
    }

    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") m = new Material(Shader.Find("Standard"));
        m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }

    static void BuildHandDisplay(Canvas canvas)
    {
        var root = new GameObject("HandDisplayRoot"); root.transform.SetParent(canvas.transform, false);
        var rR = root.AddComponent<RectTransform>();
        rR.anchorMin = new Vector2(1,0); rR.anchorMax = new Vector2(1,0); rR.pivot = new Vector2(1,0);
        rR.anchoredPosition = new Vector2(-30,110); rR.sizeDelta = new Vector2(160,200);

        var nGO = new GameObject("WeaponName"); nGO.transform.SetParent(root.transform, false);
        var nR = nGO.AddComponent<RectTransform>(); nR.anchorMin = new Vector2(0,1); nR.anchorMax = new Vector2(1,1); nR.pivot = new Vector2(0.5f,1); nR.anchoredPosition = Vector2.zero; nR.sizeDelta = new Vector2(0,28);
        var nT = nGO.AddComponent<Text>(); nT.text = "Holé ruky"; nT.fontSize = 15; nT.fontStyle = FontStyle.Bold; nT.color = new Color(1f,0.85f,0.1f); nT.alignment = TextAnchor.MiddleCenter; nT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var hGO = new GameObject("Handle"); hGO.transform.SetParent(root.transform, false);
        var hI = hGO.AddComponent<Image>(); hI.color = new Color(0.7f,0.5f,0.35f);
        var hR = hGO.GetComponent<RectTransform>(); hR.anchoredPosition = new Vector2(20,-110); hR.sizeDelta = new Vector2(45,120);

        var bGO = new GameObject("Blade"); bGO.transform.SetParent(hGO.transform, false);
        var bI = bGO.AddComponent<Image>(); bI.color = new Color(0.85f,0.65f,0.5f);
        var bR = bGO.GetComponent<RectTransform>(); bR.anchoredPosition = new Vector2(0,80); bR.sizeDelta = new Vector2(55,55);

        var hdGO = new GameObject("HandDisplay_Ctrl"); hdGO.transform.SetParent(root.transform, false);
        hdGO.AddComponent<RectTransform>();
        var hd = hdGO.AddComponent<HandDisplay>();
        hd.handleRect = hR; hd.bladeRect = bR; hd.handleImg = hI; hd.bladeImg = bI; hd.weaponNameText = nT;
    }

    static TMP_FontAsset LoadBangers()
    {
        string p = AssetDatabase.GUIDToAssetPath(BANGERS_GUID);
        return string.IsNullOrEmpty(p) ? null : AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
    }

    static GameObject MkTMP(GameObject parent, string name, string txt, int size, TMP_FontAsset font, Color col, Vector2 pos, Vector2 sizeD)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = sizeD;
        var t = go.AddComponent<TextMeshProUGUI>(); t.text = txt; t.fontSize = size; t.color = col; t.alignment = TextAlignmentOptions.Center;
        if (font != null) t.font = font;
        return go;
    }

    static GameObject MkButton(GameObject parent, string name, string label, TMP_FontAsset font, Color col, Vector2 pos, Vector2 sizeD)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = col;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = pos; rt.sizeDelta = sizeD;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var lbl = MkTMP(go, "Label", label, 40, font, new Color(1f,0.85f,0.1f), Vector2.zero, sizeD);
        var lr = lbl.GetComponent<RectTransform>(); lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
        return go;
    }
}
