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

        // ESC alebo TAB = pauza
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.tabKey.wasPressedThisFrame)
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

    /// Tlačidlo QUIT v pause menu: ukonči kolo, spočítaj peniaze a choď do hubu.
    /// (Názov metódy ostal kvôli existujúcemu prepojeniu tlačidla v scéne.)
    public void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.QuitToHub();
        else
            SceneManager.LoadScene("Hub");
    }

    // Alias s jasnejším názvom
    public void QuitToHub() => GoToMainMenu();

    void Pause()
    {
        // Nedovoľ pauzu ak kolo skončilo
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) return;

        if (pausePanel   != null) pausePanel.SetActive(true);
        if (crosshairDot != null) crosshairDot.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }
}
