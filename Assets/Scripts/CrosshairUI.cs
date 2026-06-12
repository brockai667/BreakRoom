using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    private PauseMenu pause;

    void Start()
    {
        pause = FindFirstObjectByType<PauseMenu>();
    }

    void OnGUI()
    {
        // Skry crosshair počas pauzy alebo po skončení kola
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) return;
        if (pause == null) pause = FindFirstObjectByType<PauseMenu>();
        if (pause != null && pause.IsPaused) return;

        float cx = Screen.width / 2;
        float cy = Screen.height / 2;
        float size = 10f;
        float thickness = 2f;

        GUI.color = Color.white;

        // Horizontalna ciara
        GUI.DrawTexture(new Rect(cx - size, cy - thickness / 2, size * 2, thickness), Texture2D.whiteTexture);

        // Vertikalna ciara
        GUI.DrawTexture(new Rect(cx - thickness / 2, cy - size, thickness, size * 2), Texture2D.whiteTexture);
    }
}
