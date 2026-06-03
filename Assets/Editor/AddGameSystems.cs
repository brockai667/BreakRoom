/// Tento skript pridá GameManager, HandDisplay a EndPanel do existujúcej Office scény.
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddGameSystems
{
    [MenuItem("BreakRoom/Add Game Systems to Office")]
    public static void Add()
    {
        // Otvor Office scénu ak nie je otvorená
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "Office")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Office.unity");
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        // Odober starý GameManager ak existuje
        var old = GameObject.Find("GameManager");
        if (old != null) GameObject.DestroyImmediate(old);
        var oldInv = GameObject.Find("PlayerInventory");
        if (oldInv != null) GameObject.DestroyImmediate(oldInv);
        var oldHand = GameObject.Find("HandDisplayRoot");
        if (oldHand != null) GameObject.DestroyImmediate(oldHand);

        // ---- PLAYER INVENTORY (persistent) ----
        new GameObject("PlayerInventory").AddComponent<PlayerInventory>();

        // ---- GAME MANAGER ----
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();

        // ---- END ROUND PANEL (na hlavnom canvase) ----
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("Canvas nenájdený!"); return; }

        // Money text (vedľa LVL)
        var moneyParent = canvas.transform.Find("XP_HUD");
        GameObject moneyGO = null;
        if (moneyParent != null) {
            moneyGO = new GameObject("MoneyText"); moneyGO.transform.SetParent(moneyParent, false);
            var mr = moneyGO.AddComponent<RectTransform>();
            mr.anchorMin = new Vector2(0,1); mr.anchorMax = new Vector2(1,1);
            mr.offsetMin = new Vector2(12,-32); mr.offsetMax = new Vector2(-12,-8);
            var mt = moneyGO.AddComponent<UnityEngine.UI.Text>(); mt.text = "$0";
            mt.fontSize = 18; mt.color = new Color(0.2f,0.9f,0.3f); mt.alignment = TextAnchor.MiddleRight;
            mt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // End Round overlay
        var endPanel = new GameObject("EndPanel"); endPanel.transform.SetParent(canvas.transform, false);
        var epImg = endPanel.AddComponent<UnityEngine.UI.Image>(); epImg.color = new Color(0,0,0,0.88f);
        var epR = endPanel.GetComponent<RectTransform>(); epR.anchorMin = Vector2.zero; epR.anchorMax = Vector2.one; epR.offsetMin = epR.offsetMax = Vector2.zero;

        // Title
        MakeTxt(endPanel, "EndTitle", "KONIEC KOLA", 64, FontStyle.Bold, new Color(1f,0.85f,0.1f), TextAnchor.MiddleCenter, new Vector2(0,160), new Vector2(700,80));

        // Stats
        var timeT   = MakeTxt(endPanel,"TimeText",  "Čas: --:--",  32, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, new Vector2(0,80),  new Vector2(500,45));
        var destT   = MakeTxt(endPanel,"DestrText", "Rozbité: 0",  32, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, new Vector2(0,30),  new Vector2(500,45));
        var earnT   = MakeTxt(endPanel,"EarnText",  "+$0",         42, FontStyle.Bold,   new Color(0.2f,0.9f,0.3f), TextAnchor.MiddleCenter, new Vector2(0,-30), new Vector2(500,55));
        var totalT  = MakeTxt(endPanel,"TotalText", "Celkom: $0",  26, FontStyle.Normal, new Color(0.9f,0.85f,0.5f), TextAnchor.MiddleCenter, new Vector2(0,-85), new Vector2(500,38));

        // Buttons
        var menuBtn  = MakeBtn(endPanel,"MenuBtn",  "MENU",   new Color(0.18f,0.09f,0.04f), new Vector2(-120,-155), new Vector2(200,55));
        var shopBtn  = MakeBtn(endPanel,"ShopBtn",  "SHOP",   new Color(0.05f,0.25f,0.05f), new Vector2(  0f,-155), new Vector2(200,55));
        var replayBtn= MakeBtn(endPanel,"ReplayBtn","HRAŤ ZNOVA",new Color(0.05f,0.05f,0.25f),new Vector2( 120,-155),new Vector2(200,55));

        menuBtn .GetComponent<UnityEngine.UI.Button>().onClick.AddListener(gm.GoToMenu);
        replayBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(gm.Replay);
        shopBtn .GetComponent<UnityEngine.UI.Button>().onClick.AddListener(
            () => UnityEngine.SceneManagement.SceneManager.LoadScene("Shop"));

        endPanel.SetActive(false);

        // Wire up GameManager
        gm.endPanel       = endPanel;
        gm.timeText       = timeT.GetComponent<UnityEngine.UI.Text>();
        gm.destroyedText  = destT.GetComponent<UnityEngine.UI.Text>();
        gm.moneyEarnedText= earnT.GetComponent<UnityEngine.UI.Text>();
        gm.totalMoneyText = totalT.GetComponent<UnityEngine.UI.Text>();

        // ---- HAND DISPLAY (dole vpravo, pod XP HUD) ----
        BuildHandDisplay(canvas);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✅ Game systems pridané do Office scény!");
    }

    static void BuildHandDisplay(GameObject canvas)
    {
        var root = new GameObject("HandDisplayRoot"); root.transform.SetParent(canvas.transform, false);
        var rootR = root.AddComponent<RectTransform>();
        rootR.anchorMin = new Vector2(1,0); rootR.anchorMax = new Vector2(1,0);
        rootR.pivot = new Vector2(1,0); rootR.anchoredPosition = new Vector2(-30, 110);
        rootR.sizeDelta = new Vector2(160, 200);

        // Weapon name label
        var nameLbl = new GameObject("WeaponName"); nameLbl.transform.SetParent(root.transform, false);
        var nlR = nameLbl.AddComponent<RectTransform>();
        nlR.anchorMin = new Vector2(0,1); nlR.anchorMax = new Vector2(1,1);
        nlR.pivot = new Vector2(0.5f,1); nlR.anchoredPosition = new Vector2(0,0); nlR.sizeDelta = new Vector2(0,28);
        var nlT = nameLbl.AddComponent<UnityEngine.UI.Text>(); nlT.text = "Holé ruky";
        nlT.fontSize = 15; nlT.fontStyle = FontStyle.Bold; nlT.color = new Color(1f,0.85f,0.1f);
        nlT.alignment = TextAnchor.MiddleCenter;
        nlT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Handle (ruka / rukoväť)
        var handle = new GameObject("Handle"); handle.transform.SetParent(root.transform, false);
        var hImg = handle.AddComponent<UnityEngine.UI.Image>(); hImg.color = new Color(0.7f,0.5f,0.35f);
        var hRect = handle.AddComponent<RectTransform>();
        hRect.anchoredPosition = new Vector2(20,-110); hRect.sizeDelta = new Vector2(45,120);

        // Blade / head of weapon
        var blade = new GameObject("Blade"); blade.transform.SetParent(handle.transform, false);
        var bImg = blade.AddComponent<UnityEngine.UI.Image>(); bImg.color = new Color(0.85f,0.65f,0.5f);
        var bRect = blade.GetComponent<RectTransform>();
        bRect.anchoredPosition = new Vector2(0,80); bRect.sizeDelta = new Vector2(55,55);

        // HandDisplay component
        var hdGO = new GameObject("HandDisplay_Ctrl"); hdGO.transform.SetParent(root.transform, false);
        var hd = hdGO.AddComponent<HandDisplay>();
        hd.handleRect    = hRect;
        hd.bladeRect     = bRect;
        hd.handleImg     = hImg;
        hd.bladeImg      = bImg;
        hd.weaponNameText= nlT;
    }

    static GameObject MakeTxt(GameObject parent, string name, string txt, int size,
        FontStyle style, Color col, TextAnchor anchor, Vector2 aPos, Vector2 sd)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var r = go.AddComponent<RectTransform>(); r.anchoredPosition=aPos; r.sizeDelta=sd;
        var t = go.AddComponent<UnityEngine.UI.Text>(); t.text=txt; t.fontSize=size;
        t.fontStyle=style; t.color=col; t.alignment=anchor;
        t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    static GameObject MakeBtn(GameObject parent, string name, string label, Color col, Vector2 aPos, Vector2 sd)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<UnityEngine.UI.Image>(); img.color=col;
        var r = go.AddComponent<RectTransform>(); r.anchoredPosition=aPos; r.sizeDelta=sd;
        var btn = go.AddComponent<UnityEngine.UI.Button>(); btn.targetGraphic=img;
        var lbl = new GameObject("L"); lbl.transform.SetParent(go.transform,false);
        var lr = lbl.AddComponent<RectTransform>(); lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one; lr.offsetMin=lr.offsetMax=Vector2.zero;
        var lt = lbl.AddComponent<UnityEngine.UI.Text>(); lt.text=label; lt.fontSize=20; lt.fontStyle=FontStyle.Bold;
        lt.color=Color.white; lt.alignment=TextAnchor.MiddleCenter;
        lt.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }
}
