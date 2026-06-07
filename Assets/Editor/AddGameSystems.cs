using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddGameSystems
{
    [MenuItem("BreakRoom/Add Game Systems to Office")]
    public static void Add()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "Office")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Office.unity");
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        // Odober staré objekty
        foreach (var n in new[]{"GameManager","PlayerInventory","HandDisplayRoot"})
        { var g=GameObject.Find(n); if (g!=null) GameObject.DestroyImmediate(g); }
        var cv = GameObject.Find("Canvas");
        if (cv != null) {
            foreach (var cn in new[]{"EndPanel","HandDisplayRoot"}) {
                var t = cv.transform.Find(cn); if (t!=null) GameObject.DestroyImmediate(t.gameObject);
            }
        }

        // PlayerInventory
        new GameObject("PlayerInventory").AddComponent<PlayerInventory>();

        // GameManager
        var gmGO = new GameObject("GameManager");
        var gm   = gmGO.AddComponent<GameManager>();

        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("Canvas nenájdený!"); return; }

        // END ROUND PANEL
        var ep = new GameObject("EndPanel"); ep.transform.SetParent(canvas.transform,false);
        ep.AddComponent<Image>().color = new Color(0,0,0,0.88f);
        var epR=ep.GetComponent<RectTransform>(); epR.anchorMin=Vector2.zero; epR.anchorMax=Vector2.one; epR.offsetMin=epR.offsetMax=Vector2.zero;

        MkT(ep,"EndTitle",  "KONIEC KOLA",    64,FontStyle.Bold,  new Color(1f,0.85f,0.1f),  TextAnchor.MiddleCenter, new Vector2(0, 170), new Vector2(700,80));
        var tT=MkT(ep,"TimeText",  "Čas: 00:00.0",  30,FontStyle.Normal,Color.white,         TextAnchor.MiddleCenter, new Vector2(0, 95),  new Vector2(600,42));
        var dT=MkT(ep,"DestrText", "Rozbité: 0",    30,FontStyle.Normal,Color.white,         TextAnchor.MiddleCenter, new Vector2(0, 48),  new Vector2(600,42));
        var eT=MkT(ep,"EarnText",  "+$0",           46,FontStyle.Bold,  new Color(0.2f,0.9f,0.3f),TextAnchor.MiddleCenter,new Vector2(0,-10),new Vector2(600,58));
        var lT=MkT(ep,"TotalText", "Celkom: $0",    24,FontStyle.Normal,new Color(0.9f,0.85f,0.5f),TextAnchor.MiddleCenter,new Vector2(0,-68),new Vector2(600,36));

        var line=new GameObject("Line"); line.transform.SetParent(ep.transform,false);
        line.AddComponent<Image>().color=new Color(1f,0.85f,0.1f,0.5f);
        var lr=line.GetComponent<RectTransform>(); lr.anchoredPosition=new Vector2(0,-95); lr.sizeDelta=new Vector2(500,2);

        var menuBtn  =MkBtn(ep,"MenuBtn",  "MENU",       new Color(0.18f,0.09f,0.04f),new Vector2(-160,-148),new Vector2(200,52));
        var shopBtn  =MkBtn(ep,"ShopBtn",  "SHOP",       new Color(0.05f,0.30f,0.05f),new Vector2(   0,-148),new Vector2(200,52));
        var replayBtn=MkBtn(ep,"ReplayBtn","HRAŤ ZNOVA", new Color(0.05f,0.05f,0.30f),new Vector2( 160,-148),new Vector2(200,52));

        // Persistent listeners (nie lambdy)
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn  .GetComponent<Button>().onClick, gm.GoToMenu);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(shopBtn  .GetComponent<Button>().onClick, gm.GoToShop);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(replayBtn.GetComponent<Button>().onClick, gm.Replay);

        ep.SetActive(false);

        gm.endPanel        = ep;
        gm.timeText        = tT.GetComponent<Text>();
        gm.destroyedText   = dT.GetComponent<Text>();
        gm.moneyEarnedText = eT.GetComponent<Text>();
        gm.totalMoneyText  = lT.GetComponent<Text>();

        BuildHandDisplay(canvas);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✅ Game systems pridané!");
    }

    static void BuildHandDisplay(GameObject canvas)
    {
        var root=new GameObject("HandDisplayRoot"); root.transform.SetParent(canvas.transform,false);
        var rR=root.AddComponent<RectTransform>();
        rR.anchorMin=new Vector2(1,0); rR.anchorMax=new Vector2(1,0); rR.pivot=new Vector2(1,0);
        rR.anchoredPosition=new Vector2(-30,110); rR.sizeDelta=new Vector2(160,200);

        // Weapon name label
        var nGO=new GameObject("WeaponName"); nGO.transform.SetParent(root.transform,false);
        var nR=nGO.AddComponent<RectTransform>(); nR.anchorMin=new Vector2(0,1); nR.anchorMax=new Vector2(1,1);
        nR.pivot=new Vector2(0.5f,1); nR.anchoredPosition=Vector2.zero; nR.sizeDelta=new Vector2(0,28);
        var nT=nGO.AddComponent<Text>(); nT.text="Holé ruky"; nT.fontSize=15; nT.fontStyle=FontStyle.Bold;
        nT.color=new Color(1f,0.85f,0.1f); nT.alignment=TextAnchor.MiddleCenter;
        nT.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Handle (rukoväť)
        var hGO=new GameObject("Handle"); hGO.transform.SetParent(root.transform,false);
        var hI=hGO.AddComponent<Image>(); hI.color=new Color(0.7f,0.5f,0.35f);
        var hR=hGO.GetComponent<RectTransform>(); hR.anchoredPosition=new Vector2(20,-110); hR.sizeDelta=new Vector2(45,120);

        // Blade (hlava zbrane)
        var bGO=new GameObject("Blade"); bGO.transform.SetParent(hGO.transform,false);
        var bI=bGO.AddComponent<Image>(); bI.color=new Color(0.85f,0.65f,0.5f);
        var bR=bGO.GetComponent<RectTransform>(); bR.anchoredPosition=new Vector2(0,80); bR.sizeDelta=new Vector2(55,55);

        // HandDisplay component
        var hdGO=new GameObject("HandDisplay_Ctrl"); hdGO.transform.SetParent(root.transform,false);
        hdGO.AddComponent<RectTransform>();
        var hd=hdGO.AddComponent<HandDisplay>();
        hd.handleRect=hR; hd.bladeRect=bR; hd.handleImg=hI; hd.bladeImg=bI; hd.weaponNameText=nT;
    }

    static GameObject MkT(GameObject p,string n,string txt,int sz,FontStyle fs,Color c,TextAnchor a,Vector2 pos,Vector2 sd)
    {
        var go=new GameObject(n); go.transform.SetParent(p.transform,false);
        var r=go.AddComponent<RectTransform>(); r.anchoredPosition=pos; r.sizeDelta=sd;
        var t=go.AddComponent<Text>(); t.text=txt; t.fontSize=sz; t.fontStyle=fs; t.color=c; t.alignment=a;
        t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static GameObject MkBtn(GameObject p,string n,string lbl,Color c,Vector2 pos,Vector2 sd)
    {
        var go=new GameObject(n); go.transform.SetParent(p.transform,false);
        var img=go.AddComponent<Image>(); img.color=c;
        var r=go.GetComponent<RectTransform>(); r.anchoredPosition=pos; r.sizeDelta=sd;
        var btn=go.AddComponent<Button>(); btn.targetGraphic=img;
        var l=new GameObject("L"); l.transform.SetParent(go.transform,false);
        var lr=l.AddComponent<RectTransform>(); lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one; lr.offsetMin=lr.offsetMax=Vector2.zero;
        var lt=l.AddComponent<Text>(); lt.text=lbl; lt.fontSize=20; lt.fontStyle=FontStyle.Bold;
        lt.color=Color.white; lt.alignment=TextAnchor.MiddleCenter;
        lt.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }
}
