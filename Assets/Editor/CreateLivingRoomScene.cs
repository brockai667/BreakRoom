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
        // ---------- NÁBYTOK (Kenney Furniture Kit - low-poly cartoon, OAR štýl) ----------
        float S = 0.4f; // mierka Kenney nábytku
        // --- Sedacia zóna: gauč + 2 kreslá + stolík na koberci, čelom k TV (+z) ---
        PlacePrefab("rugRectangle",     new Vector3(0f, 0.02f, 3.0f), Vector3.zero,         0, false, S * 1.7f); // koberec
        PlacePrefab("loungeSofa",       new Vector3(0f, 0f, 1.8f),    new Vector3(0,0,0),   6, true,  S);  // gauč čelom k TV
        PlacePrefab("pillow",           new Vector3(-0.7f,0.34f,1.8f),new Vector3(0,0,0),   1, true,  S);  // vankúš
        PlacePrefab("pillowBlue",       new Vector3( 0.7f,0.34f,1.8f),new Vector3(0,0,0),   1, true,  S);  // vankúš
        PlacePrefab("loungeChair",      new Vector3( 3.1f, 0f, 2.9f), new Vector3(0,-30,0), 4, true,  S);  // kreslo vpravo
        PlacePrefab("loungeChair",      new Vector3(-3.1f, 0f, 2.9f), new Vector3(0, 30,0), 4, true,  S);  // kreslo vľavo
        PlacePrefab("tableCoffee",      new Vector3(0f, 0f, 3.3f),    Vector3.zero,         4, true,  S);  // konferenčný stolík
        PlacePrefab("books",            new Vector3(0.2f,0.4f,3.3f),  Vector3.zero,         1, true,  S);  // knihy na stolíku
        // --- TV pri zadnej stene, čelom na gauč (-z) ---
        PlacePrefab("cabinetTelevision",new Vector3(0f, 0f, 5.4f),    new Vector3(0,180,0), 5, true,  S);  // TV skrinka
        PlacePrefab("televisionModern", new Vector3(0f, 0.55f, 5.4f), new Vector3(0,180,0), 4, true,  S);  // TV
        // --- Bočný stolík + stolová lampa + rádio pri gauči ---
        PlacePrefab("sideTable",        new Vector3(-2.5f, 0f, 1.8f), Vector3.zero,         4, true,  S);  // bočný stolík
        PlacePrefab("lampRoundTable",   new Vector3(-2.5f, 0.5f, 1.8f),Vector3.zero,        3, true,  S);  // stolová lampa
        PlacePrefab("radio",            new Vector3( 2.5f, 0f, 1.8f), Vector3.zero,         2, true,  S);  // rádio
        // --- Knižnica pri ľavej stene ---
        PlacePrefab("bookcaseOpen",     new Vector3(-5.5f, 0f, -1.0f),new Vector3(0,90,0),  6, true,  S);  // knižnica
        // --- Pracovný kút: stôl + stolička + laptop (ľavá-predná stena) ---
        PlacePrefab("desk",             new Vector3(-5.0f, 0f, -4.4f),new Vector3(0,90,0),  5, true,  S);  // stôl
        PlacePrefab("chairDesk",        new Vector3(-4.1f, 0f, -4.4f),new Vector3(0,-90,0), 4, true,  S);  // stolička
        PlacePrefab("laptop",           new Vector3(-5.0f, 0.62f,-4.4f),new Vector3(0,90,0),2, true,  S);  // laptop
        // --- Doplnky: stojaca lampa, rastliny, vešiak, kôš ---
        PlacePrefab("lampRoundFloor",   new Vector3(5.3f, 0f, 5.2f),  Vector3.zero,         3, true,  S);  // stojaca lampa (roh)
        PlacePrefab("pottedPlant",      new Vector3(-5.3f,0f, 5.2f),  Vector3.zero,         3, true,  S);  // veľká rastlina (roh)
        PlacePrefab("plantSmall1",      new Vector3( 5.3f, 0f, -1.0f),Vector3.zero,         2, true,  S);  // malá rastlina
        PlacePrefab("coatRackStanding", new Vector3( 4.6f, 0f, -5.0f),Vector3.zero,         3, true,  S);  // vešiak pri vchode
        PlacePrefab("trashcan",         new Vector3(-3.4f,0f, -4.9f), Vector3.zero,         2, true,  S);  // kôš

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

    // --- vkladanie reálnych prefabov z Apartment Kit ---
    static GameObject LoadPrefab(string name)
    {
        foreach (var g in AssetDatabase.FindAssets(name))
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            string ext = System.IO.Path.GetExtension(path).ToLower();
            if (ext != ".prefab" && ext != ".fbx") continue;
            if (System.IO.Path.GetFileNameWithoutExtension(path) != name) continue;
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go != null) return go;
        }
        return null;
    }

    static GameObject PlacePrefab(string name, Vector3 pos, Vector3 euler, int hp, bool breakable = true, float scale = 1f)
    {
        var prefab = LoadPrefab(name);
        if (prefab == null) { Debug.LogWarning("Model/prefab nenájdený: " + name); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        if (scale != 1f) go.transform.localScale = go.transform.localScale * scale;
        EnsureCollider(go);
        if (breakable) EnsureBreakable(go, hp);
        return go;
    }

    static void EnsureCollider(GameObject go)
    {
        if (go.GetComponentInChildren<Collider>() != null) return;
        var rends = go.GetComponentsInChildren<Renderer>();
        var bc = go.AddComponent<BoxCollider>();
        if (rends.Length == 0) return;
        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        Vector3 ls = go.transform.lossyScale;
        bc.center = go.transform.InverseTransformPoint(b.center);
        bc.size = new Vector3(b.size.x / Mathf.Max(0.001f, ls.x),
                              b.size.y / Mathf.Max(0.001f, ls.y),
                              b.size.z / Mathf.Max(0.001f, ls.z));
    }

    static void EnsureBreakable(GameObject go, int hp)
    {
        if (go.GetComponent<Breakable>() != null) return;
        var bk = go.AddComponent<Breakable>();
        bk.hp = Mathf.Max(1, hp); bk.damage = 1; bk.xpValue = 12; bk.fragmentCount = 8;
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
