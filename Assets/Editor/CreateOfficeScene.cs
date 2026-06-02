using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem.UI;

public class CreateOfficeScene
{
    [MenuItem("BreakRoom/Create Office Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // LIGHTING
        var sun = new GameObject("Directional Light").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 0.75f;
        sun.color = new Color(1f,0.95f,0.85f);
        sun.transform.rotation = Quaternion.Euler(50,-25,0);

        var fill = new GameObject("Fill Light").AddComponent<Light>();
        fill.type = LightType.Point; fill.intensity = 0.45f; fill.range = 28f;
        fill.color = new Color(0.8f,0.85f,1f);
        fill.transform.position = new Vector3(0,4.8f,0);

        // ROOM 22x6x18
        Stat("Podlaha", new Vector3(0,-0.5f,0),  new Vector3(22,1,18),    new Color(0.42f,0.30f,0.16f));
        Stat("Strop",   new Vector3(0,5.5f,0),   new Vector3(22,0.3f,18), new Color(0.93f,0.93f,0.93f));
        Stat("Stena_Z", new Vector3(0,2.5f,9),   new Vector3(22,6,0.3f),  new Color(0.87f,0.83f,0.77f));
        Stat("Stena_P", new Vector3(0,2.5f,-9),  new Vector3(22,6,0.3f),  new Color(0.87f,0.83f,0.77f));
        Stat("Stena_L", new Vector3(-11,2.5f,0), new Vector3(0.3f,6,18),  new Color(0.87f,0.83f,0.77f));
        Stat("Stena_R", new Vector3(11,2.5f,0),  new Vector3(0.3f,6,18),  new Color(0.87f,0.83f,0.77f));
        Stat("Sokl_Z",  new Vector3(0,-0.1f,8.85f),   new Vector3(22,0.2f,0.1f), new Color(0.96f,0.96f,0.96f));
        Stat("Sokl_P",  new Vector3(0,-0.1f,-8.85f),  new Vector3(22,0.2f,0.1f), new Color(0.96f,0.96f,0.96f));
        Stat("Sokl_L",  new Vector3(-10.85f,-0.1f,0), new Vector3(0.1f,0.2f,18), new Color(0.96f,0.96f,0.96f));
        Stat("Sokl_R",  new Vector3(10.85f,-0.1f,0),  new Vector3(0.1f,0.2f,18), new Color(0.96f,0.96f,0.96f));
        for (int i=-1;i<=1;i++) {
            Stat("Svietidlo_"+i, new Vector3(i*4.5f,5.4f,0), new Vector3(0.18f,0.08f,2.2f), new Color(0.96f,0.96f,1f));
            var lg = new GameObject("CeilLight_"+i); lg.transform.position = new Vector3(i*4.5f,5f,0);
            var ll = lg.AddComponent<Light>(); ll.type=LightType.Point; ll.intensity=1.3f; ll.range=14f;
            ll.color = new Color(0.96f,0.98f,1f);
        }

        // PLAYER
        var player = new GameObject("Player"); player.transform.position = new Vector3(0,1,-7);
        int pl = LayerMask.NameToLayer("Player"); player.layer = pl==-1?0:pl;
        player.AddComponent<CharacterController>(); player.AddComponent<PlayerController>();
        var camGO = new GameObject("Main Camera"); camGO.transform.SetParent(player.transform);
        camGO.transform.localPosition = new Vector3(0,0.7f,-1f);
        var cam = camGO.AddComponent<Camera>(); camGO.AddComponent<AudioListener>();
        var wh = player.AddComponent<WeaponHit>(); wh.playerCamera=cam; wh.hitDistance=4.5f;

        // MAIN CANVAS (crosshair + XP HUD — zostane viditeľné počas pauzy)
        var cvGO = new GameObject("Canvas");
        var cv = cvGO.AddComponent<Canvas>(); cv.renderMode=RenderMode.ScreenSpaceOverlay; cv.sortingOrder=1;
        cvGO.AddComponent<UnityEngine.UI.CanvasScaler>(); cvGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var dot = new GameObject("Crosshair"); dot.transform.SetParent(cvGO.transform,false);
        dot.AddComponent<UnityEngine.UI.Image>().color=Color.white;
        dot.GetComponent<RectTransform>().sizeDelta=new Vector2(6,6);

        // XP HUD panel dole vpravo
        var hudGO = new GameObject("XP_HUD"); hudGO.transform.SetParent(cvGO.transform,false);
        var hudR = hudGO.AddComponent<RectTransform>();
        hudR.anchorMin=new Vector2(1,0); hudR.anchorMax=new Vector2(1,0); hudR.pivot=new Vector2(1,0);
        hudR.anchoredPosition=new Vector2(-20,20); hudR.sizeDelta=new Vector2(340,95);
        hudGO.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,0.65f);

        var lvlGO=new GameObject("LevelText"); lvlGO.transform.SetParent(hudGO.transform,false);
        var lvlR=lvlGO.AddComponent<RectTransform>(); lvlR.anchorMin=Vector2.zero; lvlR.anchorMax=new Vector2(1,1);
        lvlR.offsetMin=new Vector2(12,55); lvlR.offsetMax=new Vector2(-12,-5);
        var lvlT=lvlGO.AddComponent<UnityEngine.UI.Text>(); lvlT.text="LVL 1"; lvlT.fontSize=28;
        lvlT.fontStyle=FontStyle.Bold; lvlT.color=new Color(1f,0.85f,0.1f); lvlT.alignment=TextAnchor.MiddleLeft;
        lvlT.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var xpGO2=new GameObject("XPText"); xpGO2.transform.SetParent(hudGO.transform,false);
        var xpR2=xpGO2.AddComponent<RectTransform>(); xpR2.anchorMin=Vector2.zero; xpR2.anchorMax=new Vector2(1,1);
        xpR2.offsetMin=new Vector2(12,33); xpR2.offsetMax=new Vector2(-12,-33);
        var xpT2=xpGO2.AddComponent<UnityEngine.UI.Text>(); xpT2.text="0 / 80 XP"; xpT2.fontSize=16;
        xpT2.color=Color.white; xpT2.alignment=TextAnchor.MiddleLeft;
        xpT2.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var barBgGO=new GameObject("XP_BarBG"); barBgGO.transform.SetParent(hudGO.transform,false);
        var barBgR=barBgGO.AddComponent<RectTransform>();
        barBgR.anchorMin=new Vector2(0,0); barBgR.anchorMax=new Vector2(1,0); barBgR.pivot=new Vector2(0.5f,0);
        barBgR.offsetMin=new Vector2(12,8); barBgR.offsetMax=new Vector2(-12,26);
        barBgGO.AddComponent<UnityEngine.UI.Image>().color=new Color(0.15f,0.15f,0.15f);

        var barFillGO=new GameObject("XP_BarFill"); barFillGO.transform.SetParent(barBgGO.transform,false);
        var barFillR=barFillGO.AddComponent<RectTransform>();
        barFillR.anchorMin=Vector2.zero; barFillR.anchorMax=Vector2.one; barFillR.offsetMin=barFillR.offsetMax=Vector2.zero;
        var barFillI=barFillGO.AddComponent<UnityEngine.UI.Image>();
        barFillI.color=new Color(0.2f,0.8f,1f); barFillI.type=UnityEngine.UI.Image.Type.Filled;
        barFillI.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal; barFillI.fillAmount=0f;

        var luGO=new GameObject("LevelUpText"); luGO.transform.SetParent(cvGO.transform,false);
        var luR=luGO.AddComponent<RectTransform>();
        luR.anchorMin=new Vector2(0.5f,0.5f); luR.anchorMax=new Vector2(0.5f,0.5f);
        luR.anchoredPosition=new Vector2(0,140); luR.sizeDelta=new Vector2(520,85);
        var luT=luGO.AddComponent<UnityEngine.UI.Text>(); luT.text="LEVEL UP!"; luT.fontSize=50;
        luT.fontStyle=FontStyle.Bold; luT.color=new Color(1f,0.85f,0.1f); luT.alignment=TextAnchor.MiddleCenter;
        luT.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        luGO.SetActive(false);

        var xpMgrGO=new GameObject("XPManager");
        var xpMgr=xpMgrGO.AddComponent<XPManager>(); xpMgr.xpBarFill=barFillI;
        var leg=xpMgrGO.AddComponent<LegacyXPUI>();
        leg.levelText=lvlT; leg.xpText=xpT2; leg.levelUpText=luT; leg.levelUpGO=luGO;

        // PAUSE CANVAS
        var pCanvas=new GameObject("PauseCanvas");
        var pc2=pCanvas.AddComponent<Canvas>(); pc2.renderMode=RenderMode.ScreenSpaceOverlay; pc2.sortingOrder=10;
        pCanvas.AddComponent<UnityEngine.UI.CanvasScaler>(); pCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var pPanel=new GameObject("PausePanel"); pPanel.transform.SetParent(pCanvas.transform,false);
        pPanel.AddComponent<UnityEngine.UI.Image>().color=new Color(0,0,0,0.75f);
        var ppR=pPanel.GetComponent<RectTransform>(); ppR.anchorMin=Vector2.zero; ppR.anchorMax=Vector2.one; ppR.offsetMin=ppR.offsetMax=Vector2.zero;
        pPanel.SetActive(false);
        new GameObject("PauseMenuController").AddComponent<PauseMenu>().pausePanel=pPanel;

        // EVENT SYSTEM — new Input System (opravuje UnityEngine.Input error)
        var es=new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        // ======== NÁBYTOK ========
        // Riadok 1 – pri zadnej stene
        WS("WS_A", new Vector3(-5.5f,0,4.5f), false);
        WS("WS_B", new Vector3(   0f,0,4.5f), false);
        WS("WS_C", new Vector3( 5.5f,0,4.5f), false);
        // Riadok 2 – ostrovček
        WS("WS_D", new Vector3(-3f,0,1f), true);
        WS("WS_E", new Vector3( 3f,0,1f), true);
        // Riadok 3 – bližší k vchodu
        WS("WS_F", new Vector3(-6f,0,-2.5f), false);
        WS("WS_G", new Vector3( 6f,0,-2.5f), false);

        // Archivačné skrine – pravá stena
        for (int i=0;i<5;i++) FilingCab("Cab_"+i, new Vector3(10.2f,0,-6f+i*3f));

        // Knižnice – ľavá stena (offsetované od steny kvôli klipu)
        Bookshelf("Shelf_A", new Vector3(-9.3f,0,-5f));
        Bookshelf("Shelf_B", new Vector3(-9.3f,0,-0.5f));
        Bookshelf("Shelf_C", new Vector3(-9.3f,0, 4f));

        // Whiteboardy
        Whiteboard("WB_Main", new Vector3( 3.5f,2.8f,8.8f));
        Whiteboard("WB_Side", new Vector3(-3.5f,2.8f,8.8f));

        // Rohový kútik
        Printer("Printer", new Vector3(8.5f,0,7f));
        WaterCooler("WaterCooler", new Vector3(-8.5f,0,6.5f));
        SideTable("CoffeeCorner",  new Vector3(-7.2f,0,6.5f));

        // Rastliny – rohy
        Plant("Plant_1", new Vector3(-9.5f,0,-7.5f));
        Plant("Plant_2", new Vector3( 9.5f,0,-7.5f));
        Plant("Plant_3", new Vector3( 9.5f,0, 7.5f));
        Plant("Plant_4", new Vector3(-9.5f,0, 7.5f));

        // Odpadkové koše
        Trash("Bin_1", new Vector3(-6.5f,0, 2.5f));
        Trash("Bin_2", new Vector3( 1.2f,0,-1.5f));
        Trash("Bin_3", new Vector3( 6.5f,0, 2.5f));
        Trash("Bin_4", new Vector3(   0f,0, 7f));

        // Floor clutter
        MB("Paper1",    new Vector3( 2f,   0.04f,-1f),   new Vector3(0.38f,0.06f,0.32f), new Color(0.95f,0.95f,0.9f),1,2);
        MB("Paper2",    new Vector3(-2.5f, 0.04f, 2.5f), new Vector3(0.34f,0.06f,0.28f), new Color(0.95f,0.93f,0.85f),1,2);
        MB("Stapler2",  new Vector3( 4f,   0.12f,-1.5f), new Vector3(0.18f,0.12f,0.34f), Color.black,1,3);
        MB("Pen2",      new Vector3(-1.5f, 0.04f, 0.5f), new Vector3(0.025f,0.025f,0.22f),Color.blue,1,1);
        MB("Mug2",      new Vector3( 0.5f, 0.06f, 3f),   new Vector3(0.12f,0.15f,0.12f), new Color(0.7f,0.1f,0.1f),1,5);
        MB("Monitor2",  new Vector3(-4f,   0.35f, 2f),   new Vector3(0.70f,0.48f,0.06f), new Color(0.05f,0.05f,0.09f),3,15);
        MB("Keyboard2", new Vector3( 2.5f, 0.04f,-3f),   new Vector3(0.52f,0.04f,0.20f), new Color(0.15f,0.15f,0.18f),1,3);

        string spath="Assets/Scenes/Office.unity";
        EditorSceneManager.SaveScene(scene,spath);
        Debug.Log("✅ Office scéna vytvorená: "+spath);
        var bsc=EditorBuildSettings.scenes;
        if (!System.Array.Exists(bsc,s=>s.path==spath))
        {
            var ns=new EditorBuildSettingsScene[bsc.Length+1];
            System.Array.Copy(bsc,ns,bsc.Length);
            ns[bsc.Length]=new EditorBuildSettingsScene(spath,true);
            EditorBuildSettings.scenes=ns;
        }
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(spath);
    }

    // ================================================================ WORKSTATION (1.25x väčší)
    static void WS(string id, Vector3 p, bool back)
    {
        Color dk=new Color(0.58f,0.38f,0.18f), lg=new Color(0.42f,0.27f,0.12f);
        Color metal=new Color(0.55f,0.55f,0.60f);
        float ms=back?1f:-1f, mz=back?0.30f:-0.30f;

        // Stôl: 2.0 × 0.06 × 1.1  (predtým 1.6×0.05×0.9)
        MB(id+"_top",   p+new Vector3(0,0.78f,0),          new Vector3(2.0f,0.07f,1.1f),   dk,   10,20);
        MB(id+"_panel", p+new Vector3(0,0.38f,-ms*0.51f),  new Vector3(1.95f,0.75f,0.05f), lg,    6,10);
        foreach (var o in new[]{new Vector3(-0.93f,0,-0.50f),new Vector3(0.93f,0,-0.50f),
                                  new Vector3(-0.93f,0, 0.50f),new Vector3(0.93f,0, 0.50f)})
            MB(id+"_leg",p+o,new Vector3(0.09f,0.78f,0.09f),metal,5,5);

        // Monitor: 0.75 × 0.50
        MB(id+"_screen",p+new Vector3(-0.40f,1.22f,mz),          new Vector3(0.75f,0.50f,0.06f), new Color(0.05f,0.05f,0.09f),2,15);
        MB(id+"_mstand",p+new Vector3(-0.40f,0.87f,mz+ms*0.08f), new Vector3(0.08f,0.18f,0.08f), metal,2,3);
        MB(id+"_mbase", p+new Vector3(-0.40f,0.82f,mz+ms*0.18f), new Vector3(0.32f,0.04f,0.24f), metal,2,3);

        // PC, klávesnica, myš, veci na stole
        MB(id+"_pc",    p+new Vector3(0.75f,0.24f,-ms*0.28f), new Vector3(0.24f,0.55f,0.48f), new Color(0.1f,0.1f,0.12f),5,10);
        MB(id+"_kb",    p+new Vector3(-0.1f,0.84f,mz-ms*0.20f), new Vector3(0.55f,0.04f,0.20f), new Color(0.15f,0.15f,0.18f),1,3);
        MB(id+"_mouse", p+new Vector3(0.30f,0.84f,mz-ms*0.20f), new Vector3(0.09f,0.04f,0.15f), new Color(0.1f,0.1f,0.1f),1,2);
        MB(id+"_mug",   p+new Vector3(0.62f,0.86f,-ms*0.08f),   new Vector3(0.11f,0.15f,0.11f), RandMug(),1,5);
        MB(id+"_papers",p+new Vector3(0.40f,0.85f,ms*0.26f),    new Vector3(0.28f,0.05f,0.35f), new Color(0.96f,0.96f,0.9f),1,2);
        MB(id+"_bind1", p+new Vector3(0.78f,0.93f,ms*0.30f),    new Vector3(0.08f,0.32f,0.38f), RandBinder(),1,4);
        MB(id+"_bind2", p+new Vector3(0.89f,0.93f,ms*0.30f),    new Vector3(0.08f,0.32f,0.38f), RandBinder(),1,4);
        MB(id+"_cup",   p+new Vector3(-0.78f,0.88f,ms*0.26f),   new Vector3(0.09f,0.16f,0.09f), new Color(0.5f,0.3f,0.1f),1,2);
        MB(id+"_stapler",p+new Vector3(0.20f,0.85f,ms*0.34f),   new Vector3(0.15f,0.08f,0.30f), Color.black,1,3);
        MB(id+"_phone", p+new Vector3(-0.65f,0.86f,-ms*0.20f),  new Vector3(0.22f,0.07f,0.28f), new Color(0.14f,0.14f,0.19f),2,6);

        // Stolička
        Chair(id+"_chair", p+new Vector3(0,0,back?1.0f:-1.0f));
    }

    // ================================================================ CHAIR (1.25x)
    static void Chair(string id, Vector3 p)
    {
        Color seat=new Color(0.08f,0.08f,0.10f), metal=new Color(0.50f,0.50f,0.55f);
        MB(id+"_seat",  p+new Vector3(0,0.52f,0),       new Vector3(0.70f,0.14f,0.70f), seat,6,8);
        MB(id+"_seatF", p+new Vector3(0,0.47f,0.30f),   new Vector3(0.68f,0.10f,0.10f), new Color(0.12f,0.12f,0.14f),3,4);
        MB(id+"_back",  p+new Vector3(0,1.02f,-0.30f),  new Vector3(0.68f,0.95f,0.11f), seat,5,8);
        MB(id+"_lumbar",p+new Vector3(0,0.70f,-0.32f),  new Vector3(0.62f,0.20f,0.09f), new Color(0.15f,0.15f,0.18f),2,3);
        MB(id+"_head",  p+new Vector3(0,1.55f,-0.28f),  new Vector3(0.36f,0.26f,0.10f), seat,2,4);
        MB(id+"_armL",  p+new Vector3(-0.38f,0.70f,0.06f),new Vector3(0.06f,0.06f,0.60f),metal,2,3);
        MB(id+"_armR",  p+new Vector3( 0.38f,0.70f,0.06f),new Vector3(0.06f,0.06f,0.60f),metal,2,3);
        MB(id+"_padL",  p+new Vector3(-0.38f,0.73f,0.06f),new Vector3(0.11f,0.05f,0.26f),new Color(0.2f,0.2f,0.22f),1,2);
        MB(id+"_padR",  p+new Vector3( 0.38f,0.73f,0.06f),new Vector3(0.11f,0.05f,0.26f),new Color(0.2f,0.2f,0.22f),1,2);
        MB(id+"_gas",   p+new Vector3(0,0.28f,0),       new Vector3(0.09f,0.55f,0.09f), metal,3,5);
        MB(id+"_baseP", p+new Vector3(0,0.05f,0),       new Vector3(0.19f,0.07f,0.19f), metal,2,3);
        for (int a=0;a<5;a++) {
            float ang=a*72f*Mathf.Deg2Rad;
            MB(id+"_arm"+a,  p+new Vector3(Mathf.Sin(ang)*0.36f,0.05f,Mathf.Cos(ang)*0.36f),new Vector3(0.10f,0.06f,0.38f),metal,2,3);
            MB(id+"_whl"+a,  p+new Vector3(Mathf.Sin(ang)*0.55f,0.05f,Mathf.Cos(ang)*0.55f),new Vector3(0.08f,0.08f,0.08f),new Color(0.15f,0.15f,0.15f),1,2);
        }
    }

    static void FilingCab(string id, Vector3 p) {
        Color body=new Color(0.52f,0.56f,0.60f);
        MB(id+"_body",p+new Vector3(0,0.85f,0),new Vector3(0.62f,1.70f,0.75f),body,8,15);
        for (int d=0;d<3;d++) {
            MB(id+"_drw"+d,p+new Vector3(0,0.28f+d*0.54f,0.38f),new Vector3(0.56f,0.44f,0.05f),new Color(0.56f,0.60f,0.64f),3,5);
            MB(id+"_hdl"+d,p+new Vector3(0,0.28f+d*0.54f,0.42f),new Vector3(0.22f,0.04f,0.04f),new Color(0.78f,0.74f,0.58f),1,2);
        }
        MB(id+"_topbox",p+new Vector3(0,1.78f,0),new Vector3(0.52f,0.24f,0.65f),new Color(0.88f,0.82f,0.68f),2,5);
    }

    // BOOKSHELF FIX: rám orientovaný správne, knihy v správnych radoch
    static void Bookshelf(string id, Vector3 p) {
        // Rám: tenký v X (do miestnosti), vysoký v Y, široký v Z (pozdĺž steny)
        Color wood=new Color(0.48f,0.29f,0.11f);
        // Zadná doska pri stene
        Stat(id+"_back", p+new Vector3(-0.18f,1.25f,0), new Vector3(0.06f,2.50f,1.40f), new Color(0.38f,0.22f,0.09f));
        // Bočné stĺpy
        Stat(id+"_sideL",p+new Vector3(0.05f,1.25f,-0.65f),new Vector3(0.25f,2.50f,0.06f),wood);
        Stat(id+"_sideR",p+new Vector3(0.05f,1.25f, 0.65f),new Vector3(0.25f,2.50f,0.06f),wood);
        // 5 horizontálnych políc
        float[] shelfY={0.10f,0.58f,1.05f,1.52f,1.98f};
        foreach (float sy in shelfY)
            Stat(id+"_sh"+sy,p+new Vector3(0.05f,sy,0),new Vector3(0.25f,0.05f,1.35f),new Color(wood.r*0.85f,wood.g*0.85f,wood.b*0.85f));
        // Knihy na každej polici
        for (int row=0;row<5;row++) {
            float bookY=shelfY[row]+0.05f;
            int cnt=UnityEngine.Random.Range(7,11);
            float startZ=-0.55f;
            for (int b=0;b<cnt && startZ<0.56f;b++) {
                float bw=UnityEngine.Random.Range(0.06f,0.10f);
                float bh=UnityEngine.Random.Range(0.30f,0.44f);
                // Knihy sú viditeľné z miestnosti (face v X smere)
                MB(id+"_r"+row+"b"+b,
                   p+new Vector3(0.10f, bookY+bh*0.5f, startZ+bw*0.5f),
                   new Vector3(0.20f, bh, bw),
                   RandBook(),1,3);
                startZ+=bw+0.005f;
            }
        }
    }

    static void Whiteboard(string id, Vector3 p) {
        MB(id+"_frame",p,new Vector3(3.0f,1.65f,0.06f),new Color(0.58f,0.58f,0.62f),3,8);
        MB(id+"_board",p,new Vector3(2.88f,1.55f,0.08f),new Color(0.97f,0.97f,0.97f),4,10);
        MB(id+"_tray", p+new Vector3(0,-0.86f,0.06f),new Vector3(2.80f,0.08f,0.14f),Color.gray,2,3);
        Color[] mc={Color.red,Color.blue,Color.black,new Color(0f,0.6f,0f)};
        for (int m=0;m<4;m++) MB(id+"_mk"+m,p+new Vector3(-0.65f+m*0.42f,-0.82f,0.14f),new Vector3(0.04f,0.04f,0.20f),mc[m],1,1);
    }

    static void Printer(string id, Vector3 p) {
        MB(id+"_body", p+new Vector3(0,0.40f,0),      new Vector3(0.75f,0.80f,0.70f), new Color(0.13f,0.13f,0.16f),8,20);
        MB(id+"_tray", p+new Vector3(0,0.82f,0.26f),  new Vector3(0.58f,0.05f,0.38f), new Color(0.22f,0.22f,0.26f),3,5);
        MB(id+"_paper",p+new Vector3(0,0.86f,0.28f),  new Vector3(0.52f,0.06f,0.36f), new Color(0.96f,0.96f,0.9f),1,3);
        MB(id+"_table",p+new Vector3(0,-0.06f,0),     new Vector3(0.90f,0.10f,0.82f), new Color(0.52f,0.32f,0.13f),5,8);
        MB(id+"_leg1", p+new Vector3(-0.40f,-0.55f,0),new Vector3(0.09f,1.0f,0.09f),  new Color(0.38f,0.22f,0.09f),4,5);
        MB(id+"_leg2", p+new Vector3( 0.40f,-0.55f,0),new Vector3(0.09f,1.0f,0.09f),  new Color(0.38f,0.22f,0.09f),4,5);
    }

    static void WaterCooler(string id, Vector3 p) {
        MB(id+"_body",  p+new Vector3(0,0.68f,0), new Vector3(0.45f,1.35f,0.45f), new Color(0.84f,0.89f,0.95f),5,8);
        MB(id+"_bottle",p+new Vector3(0,1.55f,0), new Vector3(0.36f,0.62f,0.36f), new Color(0.68f,0.83f,1.00f),2,5);
        MB(id+"_base",  p+new Vector3(0,0.06f,0), new Vector3(0.48f,0.10f,0.48f), new Color(0.28f,0.28f,0.34f),3,4);
        for (int c=0;c<5;c++) MB(id+"_cup"+c,p+new Vector3(0.36f,0.09f+c*0.07f,0),new Vector3(0.09f,0.08f,0.09f),new Color(0.9f,0.9f,0.9f),1,1);
    }

    static void SideTable(string id, Vector3 p) {
        Color w=new Color(0.52f,0.32f,0.13f);
        MB(id+"_top", p+new Vector3(0,0.80f,0),           new Vector3(1.12f,0.07f,0.68f),w,5,8);
        MB(id+"_lg1", p+new Vector3(-0.50f,0.38f,-0.28f), new Vector3(0.09f,0.80f,0.09f),w,3,4);
        MB(id+"_lg2", p+new Vector3( 0.50f,0.38f,-0.28f), new Vector3(0.09f,0.80f,0.09f),w,3,4);
        MB(id+"_lg3", p+new Vector3(-0.50f,0.38f, 0.28f), new Vector3(0.09f,0.80f,0.09f),w,3,4);
        MB(id+"_lg4", p+new Vector3( 0.50f,0.38f, 0.28f), new Vector3(0.09f,0.80f,0.09f),w,3,4);
        MB(id+"_cm",  p+new Vector3(-0.28f,1.05f,0),      new Vector3(0.36f,0.52f,0.32f),new Color(0.07f,0.07f,0.09f),5,12);
        MB(id+"_tank",p+new Vector3(-0.28f,1.38f,-0.08f), new Vector3(0.22f,0.30f,0.17f),new Color(0.68f,0.83f,1f),2,5);
        MB(id+"_mu1", p+new Vector3(0.30f,0.84f,-0.14f),  new Vector3(0.12f,0.14f,0.12f),RandMug(),1,5);
        MB(id+"_mu2", p+new Vector3(0.46f,0.84f, 0.07f),  new Vector3(0.12f,0.14f,0.12f),RandMug(),1,5);
        MB(id+"_mu3", p+new Vector3(0.30f,0.84f, 0.20f),  new Vector3(0.12f,0.14f,0.12f),RandMug(),1,5);
    }

    static void Plant(string id, Vector3 p) {
        MB(id+"_pot", p+new Vector3(0,0.24f,0),       new Vector3(0.40f,0.48f,0.40f), new Color(0.48f,0.23f,0.09f),3,5);
        MB(id+"_dirt",p+new Vector3(0,0.50f,0),       new Vector3(0.38f,0.06f,0.38f), new Color(0.28f,0.18f,0.09f),1,1);
        MB(id+"_lf1", p+new Vector3(0,0.90f,0),       new Vector3(0.40f,0.58f,0.40f), new Color(0.09f,0.52f,0.09f),2,5);
        MB(id+"_lf2", p+new Vector3(0.16f,0.82f,0.12f),new Vector3(0.28f,0.44f,0.28f),new Color(0.11f,0.58f,0.11f),1,3);
        MB(id+"_lf3", p+new Vector3(-0.14f,0.84f,-0.10f),new Vector3(0.25f,0.38f,0.25f),new Color(0.07f,0.48f,0.07f),1,3);
    }

    static void Trash(string id, Vector3 p) {
        MB(id+"_body",p+new Vector3(0,0.25f,0),new Vector3(0.34f,0.50f,0.34f),new Color(0.18f,0.18f,0.20f),3,5);
        for (int i=0;i<UnityEngine.Random.Range(2,5);i++)
            MB(id+"_p"+i,p+new Vector3(UnityEngine.Random.Range(-0.10f,0.10f),0.52f+i*0.07f,UnityEngine.Random.Range(-0.10f,0.10f)),
               new Vector3(0.09f,0.09f,0.09f),new Color(0.9f,0.9f,0.85f),1,1);
    }

    // ---------------------------------------------------------------- PRIMITIVES
    static void Stat(string n, Vector3 pos, Vector3 sc, Color col) {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=n; go.transform.position=pos; go.transform.localScale=sc; Paint(go,col);
    }
    static void MB(string n, Vector3 pos, Vector3 sc, Color col, int hp, int xp) {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=n; go.transform.position=pos; go.transform.localScale=sc; Paint(go,col);
        var rb=go.AddComponent<Rigidbody>(); rb.mass=Mathf.Max(0.5f,sc.x*sc.y*sc.z*100f); rb.isKinematic=true;
        var b=go.AddComponent<Breakable>(); b.hp=hp; b.damage=1; b.xpValue=xp;
        b.fragmentCount=Mathf.Clamp((int)(sc.x*sc.y*sc.z*90f)+4,4,14);
    }
    static void Paint(GameObject go, Color col) {
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader.name=="Hidden/InternalErrorShader") mat=new Material(Shader.Find("Standard"));
        mat.color=col; go.GetComponent<Renderer>().material=mat;
    }
    static Color RandMug()    { Color[] c={new Color(0.75f,0.1f,0.1f),new Color(0.1f,0.3f,0.85f),new Color(0.1f,0.55f,0.2f),new Color(0.95f,0.65f,0.1f),Color.white,new Color(0.55f,0.1f,0.55f)}; return c[UnityEngine.Random.Range(0,c.Length)]; }
    static Color RandBinder() { Color[] c={new Color(0.1f,0.3f,0.9f),new Color(0.9f,0.15f,0.1f),new Color(0.1f,0.75f,0.2f),new Color(0.85f,0.85f,0.1f),new Color(0.65f,0.1f,0.65f)}; return c[UnityEngine.Random.Range(0,c.Length)]; }
    static Color RandBook()   { Color[] c={new Color(0.75f,0.1f,0.1f),new Color(0.1f,0.2f,0.75f),new Color(0.1f,0.55f,0.15f),new Color(0.65f,0.65f,0.1f),new Color(0.55f,0.1f,0.55f),new Color(0.85f,0.45f,0.1f)}; return c[UnityEngine.Random.Range(0,c.Length)]; }
}
