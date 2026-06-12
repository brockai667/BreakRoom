using System.Collections;
using UnityEngine;

/// Mikro hit-stop (kratke zastavenie casu) pre uder. Self-bootstrapping,
/// netreba ho nikam pridavat. Respektuje pauzu aj slow-mo.
public class Juice : MonoBehaviour
{
    static Juice inst;
    bool busy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (inst != null) return;
        var go = new GameObject("Juice");
        DontDestroyOnLoad(go);
        inst = go.AddComponent<Juice>();
    }

    public static void HitStop(float seconds)
    {
        if (inst != null) inst.Run(seconds);
    }

    void Run(float s)
    {
        if (busy || s <= 0f) return;
        StartCoroutine(Co(s));
    }

    IEnumerator Co(float s)
    {
        busy = true;
        float prev = Time.timeScale;
        if (prev <= 0.05f) { busy = false; yield break; }   // pauza/zamrznute -> nehybat
        Time.timeScale = 0f;
        float t = 0f;
        while (t < s) { t += Time.unscaledDeltaTime; yield return null; }
        if (Time.timeScale == 0f) Time.timeScale = prev;     // obnov len ak este drzime stop
        busy = false;
    }
}
