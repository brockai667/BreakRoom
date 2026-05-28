using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject crosshairCanvas;

    private bool isPaused = false;

    void Start()
    {
        // Auto-najdi ak nie su priradene
        if (pausePanel == null)
            pausePanel = GameObject.Find("PausePanel");

        if (crosshairCanvas == null)
            crosshairCanvas = GameObject.Find("Canvas");

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Uisti sa ze hra bezi normalne
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (crosshairCanvas != null) crosshairCanvas.SetActive(true);
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
        if (pausePanel != null) pausePanel.SetActive(true);
        if (crosshairCanvas != null) crosshairCanvas.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }
}
