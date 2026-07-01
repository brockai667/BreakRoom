// Assets/Editor/ButtonSetup.cs
// Unity: Break Room -> Setup Buttons

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using System.Collections.Generic;

public class ButtonSetup : Editor
{
    [MenuItem("Break Room/Setup Buttons")]
    static void Setup()
    {
        // ── 1. Vytvor Shop a Collection sceny ────────────────────────
        CreateScene("Shop");
        CreateScene("Collection");

        // ── 2. Pridaj vsetky sceny do Build Settings ──────────────────
        AddScenesToBuild();

        // ── 3. Vrat sa do MainMenu sceny ─────────────────────────────
        string mainMenuPath = "Assets/Scenes/MainMenu.unity";
        if (System.IO.File.Exists(mainMenuPath))
            EditorSceneManager.OpenScene(mainMenuPath);

        // ── 4. Najdi alebo vytvor MainMenu GameObject so skriptom ─────
        MainMenu mainMenuScript = FindFirstObjectByType<MainMenu>();
        GameObject mainMenuGO;

        if (mainMenuScript == null)
        {
            mainMenuGO = new GameObject("MainMenuController");
            mainMenuScript = mainMenuGO.AddComponent<MainMenu>();
            Debug.Log("Vytvoreny MainMenuController GameObject.");
        }
        else
        {
            mainMenuGO = mainMenuScript.gameObject;
        }

        // ── 5. Napoj buttony ──────────────────────────────────────────
        WireButton("PlayButton",       mainMenuScript, "HratHru");
        WireButton("ShopButton",       mainMenuScript, "OpenShop");
        WireButton("CollectionButton", mainMenuScript, "OpenCollection");

        // Quit button ak existuje
        WireButton("QuitButton", mainMenuScript, "KoniecHry");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Buttony napojene! Ctrl+S uloz scenu.");
    }

    static void CreateScene(string sceneName)
    {
        string path = $"Assets/Scenes/{sceneName}.unity";

        if (System.IO.File.Exists(path))
        {
            Debug.Log($"Scena {sceneName} uz existuje, preskakujem.");
            return;
        }

        // Uloz aktualnu scenu
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Vytvor novu scenu
        var newScene = EditorSceneManager.NewScene(
            NewSceneSetup.DefaultGameObjects,
            NewSceneMode.Additive);

        // Pridaj Back to Menu button
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject btnGO = new GameObject("BackButton");
        btnGO.transform.SetParent(canvasGO.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.02f, 0.02f, 0.92f);
        var btn = btnGO.AddComponent<Button>();
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchoredPosition = new Vector2(-820f, -470f);
        btnRT.sizeDelta = new Vector2(200f, 60f);

        GameObject txtGO = new GameObject("Text (TMP)");
        txtGO.transform.SetParent(btnGO.transform, false);

        // Pridaj EventSystem
        new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>()
            .gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Uloz scenu
        EditorSceneManager.SaveScene(newScene, path);
        EditorSceneManager.CloseScene(newScene, true);

        Debug.Log($"✅ Scena {sceneName} vytvorena: {path}");
    }

    static void AddScenesToBuild()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] toAdd = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Shop.unity",
            "Assets/Scenes/Collection.unity"
        };

        foreach (string scenePath in toAdd)
        {
            bool exists = false;
            foreach (var s in scenes)
                if (s.path == scenePath) { exists = true; break; }

            if (!exists && System.IO.File.Exists(scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"Pridana do Build Settings: {scenePath}");
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static void WireButton(string buttonName, MainMenu target, string methodName)
    {
        GameObject go = GameObject.Find(buttonName);
        if (go == null) { Debug.LogWarning($"Button nenajdeny: {buttonName}"); return; }

        Button btn = go.GetComponent<Button>();
        if (btn == null) { Debug.LogWarning($"Button komponent chyba: {buttonName}"); return; }

        // Zmaz stare listenery
        btn.onClick.RemoveAllListeners();

        // Pridaj novy listener
        var method = target.GetType().GetMethod(methodName);
        if (method == null) { Debug.LogWarning($"Metoda nenajdena: {methodName}"); return; }

        var action = System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), target, method)
            as UnityEngine.Events.UnityAction;

        UnityEventTools.AddPersistentListener(btn.onClick, action);
        Debug.Log($"✅ {buttonName} -> {methodName}");
    }
}
#endif
