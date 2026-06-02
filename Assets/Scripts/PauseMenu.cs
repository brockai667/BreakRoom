using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    private GameObject crosshairDot;   // len samotny crosshair bod, nie cely canvas
    private bool isPaused = false;

    void Start()
    {
        if (pausePanel == null)
            pausePanel = GameObject.Find("PausePanel");

        // Skryj len crosshair DOT, nie cely canvas (XP HUD zostane)
        crosshairDot = GameObject.Find("Crosshair");

        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.tabKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        if (pausePanel  != null) pausePanel.SetActive(false);
        if (crosshairDot!= null) crosshairDot.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void Pause()
    {
        if (pausePanel  != null) pausePanel.SetActive(true);
        if (crosshairDot!= null) crosshairDot.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }
}
