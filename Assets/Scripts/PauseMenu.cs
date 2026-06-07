using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    private GameObject crosshairDot;
    private bool isPaused = false;

    public bool IsPaused => isPaused;

    void Start()
    {
        if (pausePanel == null)
            pausePanel = GameObject.Find("PausePanel");

        crosshairDot = GameObject.Find("Crosshair");

        if (pausePanel != null) pausePanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // ESCAPE = pauza (Tab je pre EndRound v GameManager)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        if (pausePanel   != null) pausePanel.SetActive(false);
        if (crosshairDot != null) crosshairDot.SetActive(true);
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
        // Nedovoľ pauzu ak hra skončila
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) return;

        if (pausePanel   != null) pausePanel.SetActive(true);
        if (crosshairDot != null) crosshairDot.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }
}
