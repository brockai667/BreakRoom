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

        // Pozn.: staré End Round UI (EndPanel + text polia) je odstránené —
        // koniec kola dnes rieši GameManager.EndAndGoHub() prechodom do scény Hub.
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
}
