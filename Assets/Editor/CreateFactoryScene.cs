using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using TMPro;
 
/// Postaví mapu "Factory" (továreň) — priemyselná hala s rozbíjateľnými
/// strojmi/boxmi/kolesami z kenney_factory-kit_3.0 a kompletnými hernými
/// systémami (hráč, časovač, pauza, crosshair, ruka, peniaze, XP).
/// Spusti cez menu: BreakRoom ▸ Create Factory Scene
public class CreateFactoryScene
{
    const string BANGERS_GUID = "f62de6debf194a140b0eab5be7f29d66";
 
    // Globálna mierka factory-kit modelov. Ak budú vyzerať príliš malé/veľké,
    // zmeň túto hodnotu a spusti builder znova (level sa prepíše).
    const float S = 1.0f;
 
    [MenuItem("BreakRoom/Create Factory Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bangers = LoadBangers();
 
        // ---------- SVETLO (chladnejšie, priemyselné) ----------
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.0f; light.color = new Color(0.92f, 0.95f, 1f);
        lightGO.transform.rotation = Quaternion.Euler(55, 35, 0);
        RenderSettings.ambientLight = new Color(0.38f, 0.40f, 0.44f);
 
        // ---------- HALA (20×20, výška 6) ----------
        Color floorCol = new Color(0.32f, 0.32f, 0.34f);   // betón
        Color wallCol  = new Color(0.40f, 0.45f, 0.50f);   // oceľová modrosivá
        Color ceilCol  = new Color(0.22f, 0.23f, 0.25f);   // tmavý strop
        Prop("Podlaha",      PrimitiveType.Cube, new Vector3(0, -0.25f, 0), new Vector3(20, 0.5f, 20), floorCol, 0, false);
        Prop("Stena_Zadna",  PrimitiveType.Cube, new Vector3(0, 3f, 10),    new Vector3(20, 6, 0.3f),  wallCol, 0, false);
        Prop("Stena_Predna", PrimitiveType.Cube, new Vector3(0, 3f, -10),   new Vector3(20, 6, 0.3f),  wallCol, 0, false);
        Prop("Stena_Lava",   PrimitiveType.Cube, new Vector3(-10, 3f, 0),   new Vector3(0.3f, 6, 20),  wallCol, 0, false);
        Prop("Stena_Prava",  PrimitiveType.Cube, new Vector3(10, 3f, 0),    new Vector3(0.3f, 6, 20),  wallCol, 0, false);
        Prop("Strop",        PrimitiveType.Cube, new Vector3(0, 6, 0),      new Vector3(20, 0.3f, 20), ceilCol, 0, false);

        // ---------- ATMOSFÉRA (emisívne akcenty + farebné svetlá, ako hangár v Main Menu) ----------
        // oranžové výstražné pruhy na stenách
        PropEmissive("Stripe_Back_Top", new Vector3(0, 5.2f, 9.8f),  new Vector3(19, 0.22f, 0.08f), new Color(1f, 0.46f, 0.12f));
        PropEmissive("Stripe_Back_Bot", new Vector3(0, 0.6f, 9.8f),  new Vector3(19, 0.16f, 0.08f), new Color(0.95f, 0.4f, 0.1f));
        PropEmissive("Stripe_Left",     new Vector3(-9.8f, 4.4f, 0), new Vector3(0.08f, 0.2f, 19),  new Color(0.2f, 0.55f, 1f));
        PropEmissive("Stripe_Right",    new Vector3( 9.8f, 4.4f, 0), new Vector3(0.08f, 0.2f, 19),  new Color(0.2f, 0.55f, 1f));
        // farebné akcentové svetlá v rohoch a nad linkou
        PointLight("Glow_BackL", new Vector3(-6f, 4.2f, 8.5f), new Color(1f, 0.5f, 0.18f), 3.0f, 16f);
        PointLight("Glow_BackR", new Vector3( 6f, 4.2f, 8.5f), new Color(0.25f, 0.55f, 1f), 2.6f, 16f);
        PointLight("Glow_Mid",   new Vector3( 0f, 4.5f, 0f),   new Color(1f, 0.85f, 0.6f), 2.2f, 18f);
        PointLight("Glow_FrontL",new Vector3(-6f, 4.0f, -8f),  new Color(0.9f, 0.45f, 0.15f), 2.2f, 14f);

        // ====== VÝROBNÁ LINKA (prepojená scenéria – nerozbíjateľná) ======
        // Hlavný dopravníkový pás stredom haly: vstup (-z) → výstup (+z)
        PlacePrefab("conveyor-long", new Vector3(0, 0, -6), Vector3.zero, 0, false, S);
        PlacePrefab("conveyor-long", new Vector3(0, 0, -3), Vector3.zero, 0, false, S);
        PlacePrefab("conveyor-long", new Vector3(0, 0,  0), Vector3.zero, 0, false, S);
        PlacePrefab("conveyor-long", new Vector3(0, 0,  3), Vector3.zero, 0, false, S);
        PlacePrefab("conveyor-long", new Vector3(0, 0,  6), Vector3.zero, 0, false, S);
        // Skener-brána, ktorou pás prechádza
        PlacePrefab("scanner-high", new Vector3(0, 0, -1.5f), Vector3.zero, 0, false, S);
        // Podlahové šípky toku výroby (smer +z)
        PlacePrefab("arrow", new Vector3(1.9f, 0.02f, -4f), Vector3.zero, 0, false, S);
        PlacePrefab("arrow", new Vector3(1.9f, 0.02f,  1f), Vector3.zero, 0, false, S);
        PlacePrefab("arrow", new Vector3(1.9f, 0.02f,  5f), Vector3.zero, 0, false, S);
        // Lávka (catwalk) ponad linku + schody
        PlacePrefab("catwalk-straight", new Vector3(0, 3.4f, -2f), Vector3.zero, 0, false, S);
        PlacePrefab("catwalk-straight", new Vector3(0, 3.4f,  1f), Vector3.zero, 0, false, S);
        PlacePrefab("catwalk-straight", new Vector3(0, 3.4f,  4f), Vector3.zero, 0, false, S);
        PlacePrefab("catwalk-stairs",   new Vector3(2.6f, 0, -6.5f), new Vector3(0, -90, 0), 0, false, S);
        // Potrubie pozdĺž pravej steny (prepája stroje)
        PlacePrefab("pipe-large-long",  new Vector3(8.7f, 2.4f, -3f), Vector3.zero, 0, false, S);
        PlacePrefab("pipe-large-long",  new Vector3(8.7f, 2.4f,  0f), Vector3.zero, 0, false, S);
        PlacePrefab("pipe-large-long",  new Vector3(8.7f, 2.4f,  3f), Vector3.zero, 0, false, S);
        PlacePrefab("pipe-large-valve", new Vector3(8.7f, 2.4f,  1.5f), Vector3.zero, 0, false, S);
        // Priemyselné štruktúry v rohoch (pozadie)
        PlacePrefab("structure-tall",   new Vector3(-8.6f, 0, 8.6f), Vector3.zero, 0, false, S);
        PlacePrefab("structure-medium", new Vector3( 8.6f, 0, 8.6f), Vector3.zero, 0, false, S);
        // Žeriav s magnetom nad výstupom
        PlacePrefab("crane-magnet", new Vector3(0, 0, 9f), new Vector3(0, 180, 0), 0, false, S);

        // ====== STROJE NAPOJENÉ NA LINKU (rozbíjateľné) ======
        // Ľavá strana – čelom k pásu (+x)
        PlacePrefab("machine-window", new Vector3(-2.9f, 0, -4f), new Vector3(0, 90, 0), 8, true, S);
        PlacePrefab("machine",        new Vector3(-2.9f, 0,  0f), new Vector3(0, 90, 0), 8, true, S);
        PlacePrefab("machine-bed",    new Vector3(-2.9f, 0,  4f), new Vector3(0, 90, 0), 7, true, S);
        // Pravá strana – čelom k pásu (-x)
        PlacePrefab("machine-fortified",       new Vector3(2.9f, 0, -4f), new Vector3(0, -90, 0), 9, true, S);
        PlacePrefab("machine-connection-pipe", new Vector3(2.9f, 0,  0f), new Vector3(0, -90, 0), 6, true, S);
        PlacePrefab("machine-window-bar",      new Vector3(2.9f, 0,  4f), new Vector3(0, -90, 0), 8, true, S);

        // Násypky / silá (sypú materiál na linku)
        PlacePrefab("hopper-high-round", new Vector3(0, 0, -8.3f), Vector3.zero, 6, true, S);
        PlacePrefab("hopper-round",      new Vector3( 1.8f, 0, 2f), Vector3.zero, 5, true, S);
        PlacePrefab("hopper-square",     new Vector3(-1.8f, 0, 6f), Vector3.zero, 5, true, S);

        // Robotické ramená pri páse
        PlacePrefab("robot-arm-a", new Vector3( 1.7f, 0, -1f), new Vector3(0, -90, 0), 5, true, S);
        PlacePrefab("robot-arm-b", new Vector3(-1.7f, 0,  2f), new Vector3(0,  90, 0), 5, true, S);

        // Produkty na páse (rozbíjateľné)
        PlacePrefab("box-small", new Vector3(0, 0.55f, -4.5f), Vector3.zero, 2, true, S);
        PlacePrefab("box-small", new Vector3(0, 0.55f, -1.5f), Vector3.zero, 2, true, S);
        PlacePrefab("box-small", new Vector3(0, 0.55f,  2f),   Vector3.zero, 2, true, S);
        PlacePrefab("box-small", new Vector3(0, 0.55f,  5f),   Vector3.zero, 2, true, S);

        // Výstupné palety na konci linky (vzadu)
        PlacePrefab("box-large", new Vector3(-1.1f, 0, 8.4f), new Vector3(0, 10, 0),  3, true, S);
        PlacePrefab("box-wide",  new Vector3( 1.1f, 0, 8.4f), new Vector3(0, -8, 0),  3, true, S);
        PlacePrefab("box-long",  new Vector3( 0f,   0, 7.4f), Vector3.zero,           3, true, S);
        PlacePrefab("box-large", new Vector3( 2.4f, 0, 8.6f), new Vector3(0, 20, 0),  3, true, S);

        // Kolesá a piesty na strojoch (akcenty)
        PlacePrefab("cog-a",         new Vector3(-4.8f, 0, -2f), Vector3.zero, 4, true, S);
        PlacePrefab("cog-c",         new Vector3(-4.8f, 0,  2f), Vector3.zero, 4, true, S);
        PlacePrefab("cog-e",         new Vector3( 4.8f, 0,  0f), Vector3.zero, 4, true, S);
        PlacePrefab("piston-round",  new Vector3( 4.8f, 0, -4f), Vector3.zero, 4, true, S);
        PlacePrefab("piston-square", new Vector3( 4.8f, 0,  4f), Vector3.zero, 4, true, S);

        // Obrazovky / panely na stenách + kužele pri vstupe
        PlacePrefab("screen-wide", new Vector3(-9.4f, 1.8f, -2f), new Vector3(0,  90, 0), 3, true, S);
        PlacePrefab("screen-flat", new Vector3( 9.4f, 1.8f,  6f), new Vector3(0, -90, 0), 3, true, S);
        PlacePrefab("cone", new Vector3(-1.5f, 0, -7.5f), Vector3.zero, 1, true, S);
        PlacePrefab("cone", new Vector3( 1.5f, 0, -7.5f), Vector3.zero, 1, true, S);
 
        // ---------- HRÁČ ----------
        int playerLayer = LayerMask.NameToLayer("Player"); if (playerLayer < 0) playerLayer = 3;
        var player = new GameObject("Player");
        player.layer = playerLayer;
        player.transform.position = new Vector3(0, 1, -8);
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
        new GameObject("XPManager").AddComponent<XPManager>();
 
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
        string path = "Assets/Scenes/Factory.unity";
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
        Debug.Log("✅ Továreň vytvorená: " + path + " (pridaná do Build Settings, mapa 'Factory'). " +
                  "Nezabudni pridať záznam do HubManager.MAPS, ak ešte nie je.");
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

    static void PropEmissive(string name, Vector3 pos, Vector3 scale, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.position = pos; g.transform.localScale = scale;
        var m = Mat(c);
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 2.2f); }
        g.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(g.GetComponent<Collider>());
    }

    static void PointLight(string name, Vector3 pos, Color col, float intensity, float range)
    {
        var go = new GameObject(name); go.transform.position = pos;
        var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = col; l.intensity = intensity; l.range = range;
    }

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