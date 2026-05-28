// Assets/Editor/SceneFix.cs
// Break Room -> Fix Scene

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneFix : Editor
{
    [MenuItem("Break Room/Fix Scene")]
    static void FixScene()
    {
        int fixed_count = 0;

        // ── 1. Zmaz missing scripty zo vsetkych GameObjectov ─────────
        foreach (var go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                Debug.Log($"Odstranene {removed} missing scripty z: {go.name}");
                fixed_count += removed;
            }
        }

        // ── 2. Zapni Canvas (crosshair) ───────────────────────────────
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null && !canvas.activeSelf)
        {
            canvas.SetActive(true);
            Debug.Log("✅ Canvas (crosshair) zapnuty.");
        }

        // ── 3. Skontroluj Player — pridaj skripty ak chybaju ─────────
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // CharacterController
            if (player.GetComponent<CharacterController>() == null)
            {
                player.AddComponent<CharacterController>();
                Debug.Log("✅ CharacterController pridany na Player.");
            }

            // PlayerController
            var pc = player.GetComponent("PlayerController");
            if (pc == null)
            {
                var type = System.Type.GetType("PlayerController");
                if (type != null)
                {
                    player.AddComponent(type);
                    Debug.Log("✅ PlayerController pridany na Player.");
                }
                else Debug.LogWarning("PlayerController.cs nenajdeny v projekte!");
            }

            // WeaponHit
            var wh = player.GetComponent("WeaponHit");
            if (wh == null)
            {
                var type = System.Type.GetType("WeaponHit");
                if (type != null)
                {
                    player.AddComponent(type);
                    Debug.Log("✅ WeaponHit pridany na Player.");
                }
            }

            // Layer = Player
            if (player.layer != LayerMask.NameToLayer("Player"))
            {
                player.layer = LayerMask.NameToLayer("Player");
                Debug.Log("✅ Player layer nastaveny.");
            }
        }
        else Debug.LogWarning("Player GameObject nenajdeny!");

        // ── 4. Skontroluj PauseMenu script ───────────────────────────
        GameObject pauseCanvas = GameObject.Find("PauseCanvas");
        if (pauseCanvas != null)
        {
            var pm = pauseCanvas.GetComponent("PauseMenu");
            if (pm == null)
            {
                var type = System.Type.GetType("PauseMenu");
                if (type != null)
                {
                    pauseCanvas.AddComponent(type);
                    Debug.Log("✅ PauseMenu pridany na PauseCanvas.");
                }
            }
        }

        // ── 5. Time.timeScale reset (ak zostal 0) ─────────────────────
        Time.timeScale = 1f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        if (fixed_count > 0)
            Debug.Log($"✅ Fix dokonceny! Odstranene {fixed_count} missing skriptov. Ctrl+S uloz.");
        else
            Debug.Log("✅ Fix dokonceny! Ctrl+S uloz scenu.");
    }
}
#endif
