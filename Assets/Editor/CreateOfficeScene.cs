using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateOfficeScene
{
    [MenuItem("BreakRoom/Create Office Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---- LIGHTING ----
        var sunGO = new GameObject("Directional Light");
        var sun = sunGO.AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 0.8f;
        sun.color = new Color(1f,0.95f,0.85f);
        sunGO.transform.rotation = Quaternion.Euler(45,-30,0);

        var fillGO = new GameObject("Fill Light");
        var fill = fillGO.AddComponent<Light>();
        fill.type = LightType.Point; fill.intensity = 0.4f; fill.range = 25f;
        fill.color = new Color(0.8f,0.85f,1f);
        fillGO.transform.position = new Vector3(0,4.5f,0);

        // ---- ROOM 22x6x18 ----
        CreateStatic("Podlaha", new Vector3(0,-0.5f,0),  new Vector3(22,1,18),    new Color(0.42f,0.30f,0.16f));
        CreateStatic("Strop",   new Vector3(0,5.5f,0),   new Vector3(22,0.3f,18), new Color(0.93f,0.93f,0.93f));
        CreateStatic("Stena_Z", new Vector3(0,2.5f,9),   new Vector3(22,6,0.3f),  new Color(0.87f,0.83f,0.77f));
        CreateStatic("Stena_P", new Vector3(0,2.5f,-9),  new Vector3(22,6,0.3f),  new Color(0.87f,0.83f,0.77f));
        CreateStatic("Stena_L", new Vector3(-11,2.5f,0), new Vector3(0.3f,6,18),  new Color(0.87f,0.83f,0.77f));
        CreateStatic("Stena_R", new Vector3(11,2.5f,0),  new Vector3(0.3f,6,18),  new Color(0.87f,0.83f,0.77f));
        // Baseboards
        CreateStatic("Sokl_Z", new Vector3(0,-0.1f,8.85f),   new Vector3(22,0.2f,0.1f), new Color(0.96f,0.96f,0.96f));
        CreateStatic("Sokl_P", new Vector3(0,-0.1f,-8.85f),  new Vector3(22,0.2f,0.1f), new Color(0.96f,0.96f,0.96f));
        CreateStatic("Sokl_L", new Vector3(-10.85f,-0.1f,0), new Vector3(0.1f,0.2f,18), new Color(0.96f,0.96f,0.96f));
        CreateStatic("Sokl_R", new Vector3(10.85f,-0.1f,0),  new Vector3(0.1f,0.2f,18), new Color(0.96f,0.96f,0.96f));

        // Ceiling fixtures
        for (int i = -1; i <= 1; i++)
        {
            CreateStatic("Svietidlo_"+i, new Vector3(i*4.5f,5.4f,0), new Vector3(0.18f,0.07f,2f), new Color(0.96f,0.96f,1f));
            var lg = new GameObject("CeilLight_"+i); lg.transform.position = new Vector3(i*4.5f,5f,0);
            var ll = lg.AddComponent<Light>(); ll.type=LightType.Point; ll.intensity=1.3f; ll.range=14f; ll.color=new Color(0.96f,0.98f,1f);
        }

        // ---- PLAYER ----
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0,1,-7);
        int pl = LayerMask.NameToLayer("Player");
        player.layer = pl==-1?0:pl;
        player.AddComponent<CharacterController>(); player.AddComponent<PlayerController>();
        var camGO = new GameObject("Main Camera"); camGO.transform.SetParent(player.transform);
        camGO.transform.localPosition = new Vector3(0,0.7f,-1f);
        var cam = camGO.AddComponent<Camera>(); camGO.AddComponent<AudioListener>();
        var wh = player.AddComponent<WeaponHit>(); wh.playerCamera=cam; wh.hitDistance=4f;

        // ---- CROSSHAIR ----
        var cvGO = new GameObject("Canvas"); var cv=cvGO.AddComponent<Canvas>(); cv.renderMode=RenderMode.ScreenSpaceOverlay;
        cvGO.AddComponent<UnityEngine.UI.CanvasScaler>(); cvGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var dot=new GameObject("Crosshair"); dot.transform.SetParent(cvGO.transform,false);
        dot.AddComponent<UnityEngine.UI.Image>().color=Color.white;
        dot.GetComponent<RectTransform>().sizeDelta=new Vector2(6,6);

        // ---- XP HUD (bottom right) ----
        BuildXPHud(cvGO);

        // ---- PAUSE ----
        var pCanvas=new GameObject("PauseCanvas"); var pc2=pCanvas.AddComponent<Canvas>();
        pc2.renderMode=RenderMode.ScreenSpaceOverlay; pc2.sortingOrder=10;
        pCanvas.AddComponent<UnityEngine.UI.CanvasScaler>(); pCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var pPanel=new GameObject("PausePanel"); pPanel.transform.SetParent(pCanvas.transform,false);
        var ppI=pPanel.AddComponent<UnityEngine.UI.Image>(); ppI.color=new Color(0,0,0,0.7f);
        var ppR=pPanel.GetComponent<RectTransform>(); ppR.anchorMin=Vector2.zero; ppR.anchorMax=Vector2.one; ppR.offsetMin=ppR.offsetMax=Vector2.zero;
        pPanel.SetActive(false);
        new GameObject("PauseMenuController").AddComponent<PauseMenu>().pausePanel=pPanel;

        // ---- EVENT SYSTEM ----
        var es=new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ======== FURNITURE ========
        // Row 1 – back wall (3 WS)
        MakeWorkstation("WS_A", new Vector3(-5.5f,0,4f), false);
        MakeWorkstation("WS_B", new Vector3( 0f,  0,4f), false);
        MakeWorkstation("WS_C", new Vector3( 5.5f,0,4f), false);
        // Row 2 – island
        MakeWorkstation("WS_D", new Vector3(-3.5f,0,0.5f), true);
        MakeWorkstation("WS_E", new Vector3( 3.5f,0,0.5f), true);
        // Row 3 – extra pair near front
        MakeWorkstation("WS_F", new Vector3(-6f,  0,-3f), false);
        MakeWorkstation("WS_G", new Vector3( 6f,  0,-3f), false);

        // Filing cabinets right wall
        for (int i=0;i<5;i++) MakeFilingCabinet("Cabinet_"+i, new Vector3(10.4f,0,-6f+i*3f));

        // Bookshelves left wall
        MakeBookshelf("Shelf_A", new Vector3(-10.4f,0,-4f));
        MakeBookshelf("Shelf_B", new Vector3(-10.4f,0, 0f));
        MakeBookshelf("Shelf_C", new Vector3(-10.4f,0, 4f));

        // Whiteboard + extra notice board
        MakeWhiteboard("Whiteboard_Main", new Vector3( 3f,2.8f,8.8f));
        MakeWhiteboard("Noticeboard",     new Vector3(-3f,2.8f,8.8f));

        // Printer
        MakePrinter("Printer", new Vector3(8.5f,0,7f));

        // Water cooler
        MakeWaterCooler("WaterCooler", new Vector3(-9.5f,0,6f));

        // Coffee corner (side table)
        MakeSideTable("CoffeeTable", new Vector3(-8f,0,6f));

        // Plants
        MakePlant("Plant_1", new Vector3(-9.5f,0,-7.5f));
        MakePlant("Plant_2", new Vector3( 9.5f,0,-7.5f));
        MakePlant("Plant_3", new Vector3( 9.5f,0, 7.5f));
        MakePlant("Plant_4", new Vector3(-9.5f,0, 7.5f));

        // Trash cans
        MakeTrashCan("Trash_1", new Vector3(-6f,  0, 2f));
        MakeTrashCan("Trash_2", new Vector3( 1f,  0,-2.5f));
        MakeTrashCan("Trash_3", new Vector3( 6f,  0, 2f));
        MakeTrashCan("Trash_4", new Vector3( 0f,  0, 7f));

        // Floor clutter
        MB("PaperStack1",  new Vector3( 2f,  0.03f,-1f),   new Vector3(0.32f,0.05f,0.26f), new Color(0.95f,0.95f,0.9f), 1, 2);
        MB("PaperStack2",  new Vector3(-2.5f,0.03f, 2.5f), new Vector3(0.28f,0.05f,0.22f), new Color(0.95f,0.93f,0.85f),1, 2);
        MB("Stapler_floor",new Vector3( 4f,  0.1f, -1.5f), new Vector3(0.15f,0.1f, 0.28f), Color.black,                 1, 3);
        MB("Pen_floor",    new Vector3(-1.5f,0.03f, 0.5f), new Vector3(0.02f,0.02f,0.18f), Color.blue,                  1, 1);
        MB("Mug_floor",    new Vector3( 0.5f,0.05f, 3f),   new Vector3(0.1f, 0.12f,0.1f),  new Color(0.7f,0.1f,0.1f),  1, 5);
        MB("Monitor_floor",new Vector3(-4f,  0.3f,  2f),   new Vector3(0.55f,0.38f,0.04f), new Color(0.06f,0.06f,0.1f),3,15);

        // ---- SAVE + BUILD SETTINGS ----
        string path = "Assets/Scenes/Office.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("✅ Office scéna vytvorená: " + path);
        var bsc = EditorBuildSettings.scenes;
        if (!System.Array.Exists(bsc, s=>s.path==path))
        {
            var ns = new EditorBuildSettingsScene[bsc.Length+1];
            System.Array.Copy(bsc,ns,bsc.Length);
            ns[bsc.Length]=new EditorBuildSettingsScene(path,true);
            EditorBuildSettings.scenes=ns;
        }
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(path);
    }

    // ================================================================
    //  XP HUD builder
    // ================================================================
    static void BuildXPHud(GameObject canvas)
    {
        // Container panel – bottom right
        var hud = new GameObject("XP_HUD");
        hud.transform.SetParent(canvas.transform, false);
        var hudRect = hud.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(1,0); hudRect.anchorMax = new Vector2(1,0);
        hudRect.pivot     = new Vector2(1,0);
        hudRect.anchoredPosition = new Vector2(-20,20);
        hudRect.sizeDelta = new Vector2(320, 90);
        var hudBg = hud.AddComponent<UnityEngine.UI.Image>();
        hudBg.color = new Color(0,0,0,0.65f);

        // Level label
        var lvlGO = new GameObject("LevelText"); lvlGO.transform.SetParent(hud.transform,false);
        var lvlR = lvlGO.AddComponent<RectTransform>();
        lvlR.anchorMin=Vector2.zero; lvlR.anchorMax=new Vector2(1,1);
        lvlR.offsetMin=new Vector2(10,52); lvlR.offsetMax=new Vector2(-10,-5);
        var lvlT = lvlGO.AddComponent<UnityEngine.UI.Text>();
        lvlT.text="LVL 1"; lvlT.fontSize=26; lvlT.fontStyle=FontStyle.Bold;
        lvlT.color=new Color(1f,0.85f,0.1f); lvlT.alignment=TextAnchor.MiddleLeft;
        lvlT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // XP label
        var xpGO = new GameObject("XPText"); xpGO.transform.SetParent(hud.transform,false);
        var xpR = xpGO.AddComponent<RectTransform>();
        xpR.anchorMin=Vector2.zero; xpR.anchorMax=new Vector2(1,1);
        xpR.offsetMin=new Vector2(10,32); xpR.offsetMax=new Vector2(-10,-32);
        var xpT = xpGO.AddComponent<UnityEngine.UI.Text>();
        xpT.text="0 / 80 XP"; xpT.fontSize=16; xpT.color=Color.white;
        xpT.alignment=TextAnchor.MiddleLeft;
        xpT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // XP bar background
        var barBgGO = new GameObject("XP_BarBG"); barBgGO.transform.SetParent(hud.transform,false);
        var barBgR = barBgGO.AddComponent<RectTransform>();
        barBgR.anchorMin=new Vector2(0,0); barBgR.anchorMax=new Vector2(1,0);
        barBgR.pivot=new Vector2(0.5f,0); barBgR.offsetMin=new Vector2(10,10); barBgR.offsetMax=new Vector2(-10,28);
        var barBgI = barBgGO.AddComponent<UnityEngine.UI.Image>(); barBgI.color=new Color(0.15f,0.15f,0.15f);

        // XP bar fill
        var barFillGO = new GameObject("XP_BarFill"); barFillGO.transform.SetParent(barBgGO.transform,false);
        var barFillR = barFillGO.AddComponent<RectTransform>();
        barFillR.anchorMin=Vector2.zero; barFillR.anchorMax=Vector2.one; barFillR.offsetMin=barFillR.offsetMax=Vector2.zero;
        barFillR.anchorMax=new Vector2(0,1);
        var barFillI = barFillGO.AddComponent<UnityEngine.UI.Image>();
        barFillI.color=new Color(0.2f,0.8f,1f); barFillI.type=UnityEngine.UI.Image.Type.Filled;
        barFillI.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal; barFillI.fillAmount=0f;

        // Level-up flash text
        var luGO = new GameObject("LevelUpText"); luGO.transform.SetParent(canvas.transform,false);
        var luR = luGO.AddComponent<RectTransform>();
        luR.anchorMin=new Vector2(0.5f,0.5f); luR.anchorMax=new Vector2(0.5f,0.5f);
        luR.anchoredPosition=new Vector2(0,120); luR.sizeDelta=new Vector2(500,80);
        var luT = luGO.AddComponent<UnityEngine.UI.Text>();
        luT.text="LEVEL UP!"; luT.fontSize=48; luT.fontStyle=FontStyle.Bold;
        luT.color=new Color(1f,0.85f,0.1f); luT.alignment=TextAnchor.MiddleCenter;
        luT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        luGO.SetActive(false);

        // Wire up XPManager
        var xpMgrGO = new GameObject("XPManager");
        var xpMgr = xpMgrGO.AddComponent<XPManager>();
        xpMgr.xpBarFill = barFillI;
        xpMgr.levelText = null;   // using legacy Text, not TMP – assign manually if TMP installed
        xpMgr.xpText    = null;
        xpMgr.levelUpText = null;
        // Store references as legacy UI Text via simple LegacyXPUI helper
        var legHelper = xpMgrGO.AddComponent<LegacyXPUI>();
        legHelper.levelText = lvlT;
        legHelper.xpText    = xpT;
        legHelper.levelUpText = luT;
        legHelper.levelUpGO = luGO;
    }

    // ================================================================
    //  WORKSTATION – bigger, realistic proportions
    // ================================================================
    static void MakeWorkstation(string id, Vector3 pos, bool facingBack)
    {
        Color dk = new Color(0.58f,0.38f,0.18f);
        Color lg = new Color(0.42f,0.27f,0.12f);
        float ms = facingBack?1f:-1f;
        float mz = facingBack?0.25f:-0.25f;

        // Desk surface: 1.8m wide, 0.75m high, 0.9m deep
        MB(id+"_top", pos+new Vector3(0,0.75f,0), new Vector3(1.8f,0.06f,0.9f), dk, 10, 20);
        // Modesty panel (back of desk)
        MB(id+"_panel", pos+new Vector3(0,0.37f,-ms*0.42f), new Vector3(1.75f,0.72f,0.04f), lg, 6, 10);
        // Legs (metal look)
        Color metal = new Color(0.55f,0.55f,0.6f);
        foreach (var o in new[]{new Vector3(-0.83f,0,-0.41f),new Vector3(0.83f,0,-0.41f),
                                  new Vector3(-0.83f,0, 0.41f),new Vector3(0.83f,0, 0.41f)})
            MB(id+"_leg", pos+o, new Vector3(0.07f,0.75f,0.07f), metal, 5, 5);

        // Monitor: 0.62 wide x 0.40 tall
        MB(id+"_screen", pos+new Vector3(-0.35f,1.18f,mz),          new Vector3(0.62f,0.40f,0.05f), new Color(0.05f,0.05f,0.09f), 2,15);
        MB(id+"_mstand", pos+new Vector3(-0.35f,0.84f,mz+ms*0.06f), new Vector3(0.07f,0.15f,0.07f), metal, 2,3);
        MB(id+"_mbase",  pos+new Vector3(-0.35f,0.80f,mz+ms*0.14f), new Vector3(0.28f,0.03f,0.20f), metal, 2,3);

        // PC tower (big desktop)
        MB(id+"_pc", pos+new Vector3(0.68f,0.22f,-ms*0.25f), new Vector3(0.20f,0.46f,0.40f), new Color(0.1f,0.1f,0.12f),5,10);
        // Keyboard
        MB(id+"_kb", pos+new Vector3(-0.1f,0.81f,mz-ms*0.18f), new Vector3(0.46f,0.03f,0.18f), new Color(0.15f,0.15f,0.18f),1,3);
        // Mouse
        MB(id+"_mouse", pos+new Vector3(0.25f,0.81f,mz-ms*0.18f), new Vector3(0.08f,0.03f,0.13f), new Color(0.1f,0.1f,0.1f),1,2);
        // Mug
        MB(id+"_mug", pos+new Vector3(0.55f,0.83f,-ms*0.06f), new Vector3(0.10f,0.13f,0.10f), RandMugColor(),1,5);
        // Papers
        MB(id+"_papers", pos+new Vector3(0.38f,0.82f,ms*0.22f), new Vector3(0.24f,0.04f,0.30f), new Color(0.96f,0.96f,0.9f),1,2);
        // 2 binders
        MB(id+"_bind1", pos+new Vector3(0.70f,0.90f,ms*0.27f), new Vector3(0.07f,0.28f,0.32f), RandBinderColor(),1,4);
        MB(id+"_bind2", pos+new Vector3(0.80f,0.90f,ms*0.27f), new Vector3(0.07f,0.28f,0.32f), RandBinderColor(),1,4);
        // Pencil cup
        MB(id+"_cup", pos+new Vector3(-0.70f,0.85f,ms*0.22f), new Vector3(0.08f,0.14f,0.08f), new Color(0.5f,0.3f,0.1f),1,2);
        // Stapler
        MB(id+"_stapler", pos+new Vector3(0.18f,0.82f,ms*0.30f), new Vector3(0.13f,0.07f,0.25f), Color.black,1,3);
        // Desk phone
        MB(id+"_phone", pos+new Vector3(-0.60f,0.83f,-ms*0.17f), new Vector3(0.20f,0.06f,0.25f), new Color(0.14f,0.14f,0.19f),2,6);

        // CHAIR
        MakeChair(id+"_chair", pos+new Vector3(0,0,facingBack?0.85f:-0.85f));
    }

    // ================================================================
    //  CHAIR – realistic office chair
    // ================================================================
    static void MakeChair(string id, Vector3 pos)
    {
        Color seat  = new Color(0.08f,0.08f,0.10f);  // black fabric
        Color metal = new Color(0.50f,0.50f,0.55f);  // chrome

        // Seat cushion – wider, thicker
        MB(id+"_seat",   pos+new Vector3(0,0.50f,0),       new Vector3(0.58f,0.12f,0.58f), seat, 6,8);
        // Seat edge padding
        MB(id+"_seatF",  pos+new Vector3(0,0.46f,0.25f),   new Vector3(0.56f,0.08f,0.08f), new Color(0.12f,0.12f,0.14f),3,4);

        // Backrest – tall, slightly reclined
        MB(id+"_back",   pos+new Vector3(0,0.98f,-0.24f),  new Vector3(0.56f,0.80f,0.09f), seat, 5,8);
        // Lumbar bump
        MB(id+"_lumbar", pos+new Vector3(0,0.68f,-0.26f),  new Vector3(0.52f,0.16f,0.07f), new Color(0.15f,0.15f,0.18f),2,3);
        // Headrest
        MB(id+"_head",   pos+new Vector3(0,1.43f,-0.22f),  new Vector3(0.30f,0.22f,0.08f), seat,2,4);

        // Armrests
        MB(id+"_armL",   pos+new Vector3(-0.32f,0.67f, 0.05f), new Vector3(0.05f,0.05f,0.50f), metal,2,3);
        MB(id+"_armR",   pos+new Vector3( 0.32f,0.67f, 0.05f), new Vector3(0.05f,0.05f,0.50f), metal,2,3);
        MB(id+"_armPadL",pos+new Vector3(-0.32f,0.70f, 0.05f), new Vector3(0.09f,0.04f,0.22f), new Color(0.2f,0.2f,0.22f),1,2);
        MB(id+"_armPadR",pos+new Vector3( 0.32f,0.70f, 0.05f), new Vector3(0.09f,0.04f,0.22f), new Color(0.2f,0.2f,0.22f),1,2);

        // Gas cylinder
        MB(id+"_gas",    pos+new Vector3(0,0.26f,0),       new Vector3(0.08f,0.48f,0.08f), metal,3,5);
        // Base plate
        MB(id+"_baseP",  pos+new Vector3(0,0.04f,0),       new Vector3(0.16f,0.06f,0.16f), metal,2,3);
        // 5 star arms + caster wheels
        for (int a=0;a<5;a++)
        {
            float ang = a*72f*Mathf.Deg2Rad;
            float bx=Mathf.Sin(ang)*0.30f, bz=Mathf.Cos(ang)*0.30f;
            MB(id+"_arm"+a, pos+new Vector3(bx,0.04f,bz), new Vector3(0.09f,0.05f,0.32f), metal,2,3);
            MB(id+"_wheel"+a, pos+new Vector3(bx*1.5f,0.04f,bz*1.5f), new Vector3(0.07f,0.07f,0.07f), new Color(0.15f,0.15f,0.15f),1,2);
        }
    }

    // ================================================================
    //  FILING CABINET
    // ================================================================
    static void MakeFilingCabinet(string id, Vector3 pos)
    {
        Color body = new Color(0.52f,0.56f,0.60f);
        MB(id+"_body", pos+new Vector3(0,0.75f,0), new Vector3(0.55f,1.50f,0.65f), body,8,15);
        for (int d=0;d<3;d++)
        {
            MB(id+"_drawer"+d, pos+new Vector3(0,0.25f+d*0.48f,0.33f), new Vector3(0.48f,0.38f,0.04f), new Color(0.56f,0.60f,0.64f),3,5);
            MB(id+"_handle"+d, pos+new Vector3(0,0.25f+d*0.48f,0.37f), new Vector3(0.18f,0.03f,0.03f), new Color(0.78f,0.74f,0.58f),1,2);
        }
        MB(id+"_top_box", pos+new Vector3(0,1.57f,0), new Vector3(0.45f,0.20f,0.55f), new Color(0.88f,0.82f,0.68f),2,5);
    }

    static void MakeBookshelf(string id, Vector3 pos)
    {
        Color wood = new Color(0.48f,0.29f,0.11f);
        // Frame
        MB(id+"_frame", pos+new Vector3(0,1.2f,0), new Vector3(0.28f,2.4f,1.3f), wood,12,15);
        // Shelves (horizontal boards)
        for (int s=0;s<5;s++)
            MB(id+"_shelf"+s, pos+new Vector3(0.05f,0.1f+s*0.46f,0), new Vector3(0.08f,0.04f,1.25f), new Color(wood.r*0.85f,wood.g*0.85f,wood.b*0.85f),3,5);
        // Books per shelf
        for (int row=0;row<5;row++)
        {
            float y=0.25f+row*0.46f; int cnt=Random.Range(6,10);
            for (int b=0;b<cnt;b++)
                MB(id+"_b"+row+"_"+b, pos+new Vector3(0.08f,y,-0.48f+b*0.10f),
                    new Vector3(0.09f,Random.Range(0.28f,0.40f),Random.Range(0.05f,0.09f)), RandBookColor(),1,3);
        }
    }

    static void MakeWhiteboard(string id, Vector3 pos)
    {
        MB(id+"_frame", pos, new Vector3(2.8f,1.5f,0.05f), new Color(0.58f,0.58f,0.62f),3,8);
        MB(id+"_board", pos, new Vector3(2.7f,1.4f,0.07f), new Color(0.97f,0.97f,0.97f),4,10);
        MB(id+"_tray",  pos+new Vector3(0,-0.76f,0.05f), new Vector3(2.65f,0.07f,0.12f), Color.gray,2,3);
        Color[] mc={Color.red,Color.blue,Color.black,new Color(0f,0.6f,0f)};
        for (int m=0;m<4;m++) MB(id+"_mkr"+m, pos+new Vector3(-0.6f+m*0.38f,-0.73f,0.12f), new Vector3(0.03f,0.03f,0.18f), mc[m],1,1);
    }

    static void MakePrinter(string id, Vector3 pos)
    {
        MB(id+"_body",  pos+new Vector3(0,0.35f,0),      new Vector3(0.65f,0.70f,0.60f), new Color(0.13f,0.13f,0.16f),8,20);
        MB(id+"_tray",  pos+new Vector3(0,0.72f,0.22f),  new Vector3(0.50f,0.04f,0.32f), new Color(0.22f,0.22f,0.26f),3,5);
        MB(id+"_paper", pos+new Vector3(0,0.75f,0.24f),  new Vector3(0.44f,0.05f,0.30f), new Color(0.96f,0.96f,0.9f),1,3);
        MB(id+"_table", pos+new Vector3(0,-0.05f,0),     new Vector3(0.80f,0.09f,0.72f), new Color(0.52f,0.32f,0.13f),5,8);
        MB(id+"_tleg1", pos+new Vector3(-0.35f,-0.50f,0),new Vector3(0.08f,0.88f,0.08f), new Color(0.38f,0.22f,0.09f),4,5);
        MB(id+"_tleg2", pos+new Vector3( 0.35f,-0.50f,0),new Vector3(0.08f,0.88f,0.08f), new Color(0.38f,0.22f,0.09f),4,5);
    }

    static void MakeWaterCooler(string id, Vector3 pos)
    {
        MB(id+"_body",   pos+new Vector3(0,0.60f,0), new Vector3(0.40f,1.20f,0.40f), new Color(0.84f,0.89f,0.95f),5,8);
        MB(id+"_bottle", pos+new Vector3(0,1.38f,0), new Vector3(0.32f,0.55f,0.32f), new Color(0.68f,0.83f,1.00f),2,5);
        MB(id+"_base",   pos+new Vector3(0,0.05f,0), new Vector3(0.42f,0.09f,0.42f), new Color(0.28f,0.28f,0.34f),3,4);
        for (int c=0;c<5;c++)
            MB(id+"_cup"+c, pos+new Vector3(0.32f,0.08f+c*0.06f,0), new Vector3(0.08f,0.07f,0.08f), new Color(0.9f,0.9f,0.9f),1,1);
    }

    static void MakePlant(string id, Vector3 pos)
    {
        MB(id+"_pot",  pos+new Vector3(0,0.22f,0),       new Vector3(0.35f,0.42f,0.35f), new Color(0.48f,0.23f,0.09f),3,5);
        MB(id+"_dirt", pos+new Vector3(0,0.45f,0),       new Vector3(0.33f,0.05f,0.33f), new Color(0.28f,0.18f,0.09f),1,1);
        MB(id+"_lf1",  pos+new Vector3(0,0.80f,0),       new Vector3(0.35f,0.50f,0.35f), new Color(0.09f,0.52f,0.09f),2,5);
        MB(id+"_lf2",  pos+new Vector3(0.14f,0.74f,0.1f),new Vector3(0.24f,0.38f,0.24f),new Color(0.11f,0.58f,0.11f),1,3);
        MB(id+"_lf3",  pos+new Vector3(-0.12f,0.76f,-0.09f),new Vector3(0.22f,0.34f,0.22f),new Color(0.07f,0.48f,0.07f),1,3);
    }

    static void MakeTrashCan(string id, Vector3 pos)
    {
        MB(id+"_body", pos+new Vector3(0,0.22f,0), new Vector3(0.30f,0.44f,0.30f), new Color(0.18f,0.18f,0.20f),3,5);
        for (int p=0;p<Random.Range(2,5);p++)
            MB(id+"_p"+p, pos+new Vector3(Random.Range(-0.09f,0.09f),0.47f+p*0.06f,Random.Range(-0.09f,0.09f)),
                new Vector3(0.08f,0.08f,0.08f), new Color(0.9f,0.9f,0.85f),1,1);
    }

    static void MakeSideTable(string id, Vector3 pos)
    {
        Color w=new Color(0.52f,0.32f,0.13f);
        MB(id+"_top",  pos+new Vector3(0,0.76f,0),          new Vector3(1.0f,0.06f,0.60f), w,5,8);
        MB(id+"_leg1", pos+new Vector3(-0.45f,0.37f,-0.25f),new Vector3(0.08f,0.74f,0.08f),w,3,4);
        MB(id+"_leg2", pos+new Vector3( 0.45f,0.37f,-0.25f),new Vector3(0.08f,0.74f,0.08f),w,3,4);
        MB(id+"_leg3", pos+new Vector3(-0.45f,0.37f, 0.25f),new Vector3(0.08f,0.74f,0.08f),w,3,4);
        MB(id+"_leg4", pos+new Vector3( 0.45f,0.37f, 0.25f),new Vector3(0.08f,0.74f,0.08f),w,3,4);
        MB(id+"_cm",   pos+new Vector3(-0.25f,1.00f,0),     new Vector3(0.32f,0.48f,0.28f), new Color(0.07f,0.07f,0.09f),5,12);
        MB(id+"_tank", pos+new Vector3(-0.25f,1.30f,-0.07f),new Vector3(0.20f,0.28f,0.15f), new Color(0.68f,0.83f,1f),2,5);
        MB(id+"_mug1", pos+new Vector3(0.28f,0.81f,-0.12f), new Vector3(0.10f,0.12f,0.10f), RandMugColor(),1,5);
        MB(id+"_mug2", pos+new Vector3(0.42f,0.81f, 0.06f), new Vector3(0.10f,0.12f,0.10f), RandMugColor(),1,5);
        MB(id+"_mug3", pos+new Vector3(0.28f,0.81f, 0.18f), new Vector3(0.10f,0.12f,0.10f), RandMugColor(),1,5);
        MB(id+"_plate",pos+new Vector3(0.35f,0.79f, 0.03f), new Vector3(0.30f,0.02f,0.22f), new Color(0.9f,0.9f,0.85f),1,2);
    }

    // ================================================================
    //  PRIMITIVES
    // ================================================================
    static void CreateStatic(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name; go.transform.position=pos; go.transform.localScale=scale;
        ApplyColor(go,color);
    }

    // MB = MakeBreakable shorthand  (hp, xp)
    static void MB(string name, Vector3 pos, Vector3 scale, Color color, int hp, int xp)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name; go.transform.position=pos; go.transform.localScale=scale;
        ApplyColor(go,color);
        var rb=go.AddComponent<Rigidbody>();
        rb.mass=Mathf.Max(0.5f, scale.x*scale.y*scale.z*100f);
        rb.isKinematic=true;
        var b=go.AddComponent<Breakable>(); b.hp=hp; b.damage=1; b.xpValue=xp;
        // Fragment count scales with object size
        b.fragmentCount = Mathf.Clamp((int)(scale.x*scale.y*scale.z*80f)+4, 4, 12);
    }

    static void ApplyColor(GameObject go, Color color)
    {
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader.name=="Hidden/InternalErrorShader") mat=new Material(Shader.Find("Standard"));
        mat.color=color; go.GetComponent<Renderer>().material=mat;
    }

    static Color RandMugColor()    { Color[] c={new Color(0.75f,0.1f,0.1f),new Color(0.1f,0.3f,0.85f),new Color(0.1f,0.55f,0.2f),new Color(0.95f,0.65f,0.1f),Color.white,new Color(0.55f,0.1f,0.55f)}; return c[Random.Range(0,c.Length)]; }
    static Color RandBinderColor() { Color[] c={new Color(0.1f,0.3f,0.9f),new Color(0.9f,0.15f,0.1f),new Color(0.1f,0.75f,0.2f),new Color(0.85f,0.85f,0.1f),new Color(0.65f,0.1f,0.65f)}; return c[Random.Range(0,c.Length)]; }
    static Color RandBookColor()   { Color[] c={new Color(0.75f,0.1f,0.1f),new Color(0.1f,0.2f,0.75f),new Color(0.1f,0.55f,0.15f),new Color(0.65f,0.65f,0.1f),new Color(0.55f,0.1f,0.55f),new Color(0.85f,0.45f,0.1f)}; return c[Random.Range(0,c.Length)]; }
}
