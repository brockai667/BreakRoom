using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    void OnGUI()
    {
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