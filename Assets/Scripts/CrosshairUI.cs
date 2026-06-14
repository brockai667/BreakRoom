using UnityEngine;

/// Zameriavač + hit-marker: pri zásahu blikne biely značkový "X", pri rozbití
/// väčší zlatý záblesk. Dáva okamžitú spätnú väzbu na úder.
public class CrosshairUI : MonoBehaviour
{
    static CrosshairUI inst;

    PauseMenu pause;
    float pingTime = -10f;
    bool  pingBroke;

    void Awake() { inst = this; }

    void Start()
    {
        pause = FindFirstObjectByType<PauseMenu>();
    }

    /// Zavolaj pri zásahu (broke=false) alebo rozbití (broke=true).
    public static void Ping(bool broke)
    {
        if (inst == null) return;
        inst.pingTime = Time.unscaledTime;
        inst.pingBroke = broke;
    }

    void OnGUI()
    {
        if (GameManager.Instance != null && !GameManager.Instance.roundActive) return;
        if (pause == null) pause = FindFirstObjectByType<PauseMenu>();
        if (pause != null && pause.IsPaused) return;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        float size = 10f, thickness = 2f;

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(cx - size, cy - thickness / 2f, size * 2f, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - thickness / 2f, cy - size, thickness, size * 2f), Texture2D.whiteTexture);

        // hit-marker (4 rozletujúce sa značky do X)
        float e = Time.unscaledTime - pingTime;
        float dur = pingBroke ? 0.34f : 0.16f;
        if (e >= 0f && e <= dur)
        {
            float k = e / dur;
            float spread = Mathf.Lerp(5f, pingBroke ? 26f : 15f, k);
            float a = 1f - k;
            float d = pingBroke ? 6f : 4f;   // veľkosť značky
            GUI.color = pingBroke ? new Color(1f, 0.82f, 0.2f, a) : new Color(1f, 1f, 1f, a);
            Mark(cx - spread, cy - spread, d);
            Mark(cx + spread, cy - spread, d);
            Mark(cx - spread, cy + spread, d);
            Mark(cx + spread, cy + spread, d);
        }
        GUI.color = Color.white;
    }

    static void Mark(float x, float y, float d)
    {
        GUI.DrawTexture(new Rect(x - d / 2f, y - d / 2f, d, d), Texture2D.whiteTexture);
    }
}
