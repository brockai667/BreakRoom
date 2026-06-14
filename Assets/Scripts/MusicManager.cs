using UnityEngine;
using UnityEngine.SceneManagement;

/// Procedurálna hudba na pozadie: energický beat počas hry, pokojnejšia
/// slučka v hube/menu. Všetko generované za behu (žiadne .mp3 netreba).
/// Bootstrapuje sa sám a prepína slučku podľa scény.
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    const int SR = 44100;

    AudioSource src;
    AudioClip gameLoop, menuLoop;
    float targetVol = 0.18f;
    float basePitch = 1f;   // tonina/tempo podla mapy

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("MusicManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<MusicManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        gameLoop = BuildLoop(false);
        menuLoop = BuildLoop(true);

        src = gameObject.AddComponent<AudioSource>();
        src.loop = true; src.playOnAwake = false; src.spatialBlend = 0f; src.volume = 0f;

        SceneManager.sceneLoaded += OnScene;
        Apply(SceneManager.GetActiveScene().name);
    }

    void OnDestroy() { SceneManager.sceneLoaded -= OnScene; }

    void OnScene(Scene s, LoadSceneMode m) => Apply(s.name);

    void Apply(string scene)
    {
        bool menu = scene == "Hub" || scene == "MainMenu" || scene == "Shop";
        var want = menu ? menuLoop : gameLoop;
        targetVol = menu ? 0.13f : 0.20f;
        // Iná tónina/tempo podľa mapy
        basePitch = menu                 ? 1.0f
                  : scene == "Office"    ? 1.06f
                  : scene == "Factory"   ? 0.93f
                  : scene == "Kitchen"   ? 1.10f
                  : scene == "Garage"    ? 0.90f
                  :                        1.0f;
        src.pitch = basePitch;
        if (src.clip != want) { src.clip = want; src.Play(); }
    }

    void Update()
    {
        // Intenzita podľa comba: hlasnejšie + mierne rýchlejšie
        float intensity = 0f;
        var gm = GameManager.Instance;
        if (gm != null && gm.roundActive && gm.comboCount >= 3)
            intensity = Mathf.Clamp01(gm.comboCount / 25f);

        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        src.volume = Mathf.MoveTowards(src.volume, (targetVol + intensity * 0.10f) * musicVol, Time.unscaledDeltaTime * 0.5f);
        src.pitch  = Mathf.MoveTowards(src.pitch, basePitch + intensity * 0.08f, Time.unscaledDeltaTime * 0.6f);
    }

    // ---------- GENEROVANIE SLUČKY ----------
    AudioClip BuildLoop(bool calm)
    {
        float stepDur = calm ? 0.15f : 0.10f;     // 16-tina
        int steps = 32;                            // 2 takty
        int n = Mathf.CeilToInt(SR * stepDur * steps);
        var b = new float[n];
        int sp = Mathf.RoundToInt(SR * stepDur);   // vzoriek na krok

        // basová linka (Hz) po dobách
        float[] gameBass = { 55f, 55f, 73.4f, 82.4f,  55f, 55f, 98f, 82.4f };
        float[] menuBass = { 55f, 49f, 65.4f, 49f };

        if (calm)
        {
            for (int beat = 0; beat < 8; beat++)
            {
                int at = beat * 4 * sp;
                if (beat % 2 == 0) AddKick(b, n, at, 0.6f);                 // riedke kopáky
                AddBass(b, n, at, sp * 4, menuBass[beat % menuBass.Length], 0.20f);
            }
            for (int s = 0; s < steps; s += 2) AddHat(b, n, s * sp, 0.06f); // jemné hi-haty
        }
        else
        {
            for (int beat = 0; beat < 8; beat++)
            {
                int at = beat * 4 * sp;
                AddKick(b, n, at, 0.95f);                                   // four-on-floor
                AddBass(b, n, at, sp * 4, gameBass[beat % gameBass.Length], 0.26f);
                if (beat % 2 == 1) AddSnare(b, n, at);                      // snare na dobu 2 a 4
            }
            for (int s = 0; s < steps; s++)                                 // hi-haty
                AddHat(b, n, s * sp, s % 2 == 1 ? 0.16f : 0.09f);
            AddBlip(b, n, 8 * sp, sp * 2, 220f, 0.10f);                     // občasná melódia
            AddBlip(b, n, 24 * sp, sp * 2, 293.7f, 0.10f);
        }

        Normalize(b, 0.85f);
        var clip = AudioClip.Create(calm ? "menuLoop" : "gameLoop", n, 1, SR, false);
        clip.SetData(b, 0);
        return clip;
    }

    // index s pretočením (tails plynule pokračujú na začiatku slučky)
    static void W(float[] b, int n, int idx, float v) { b[((idx % n) + n) % n] += v; }

    void AddKick(float[] b, int n, int at, float vol)
    {
        int len = (int)(SR * 0.18f);
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SR;
            float f = Mathf.Lerp(120f, 45f, t / 0.18f);
            float env = Mathf.Exp(-t * 22f);
            W(b, n, at + i, Mathf.Sin(2f * Mathf.PI * f * t) * env * vol);
        }
    }

    void AddSnare(float[] b, int n, int at)
    {
        int len = (int)(SR * 0.14f);
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 30f);
            float noise = Random.value * 2f - 1f;
            float tone = Mathf.Sin(2f * Mathf.PI * 180f * t);
            W(b, n, at + i, (noise * 0.7f + tone * 0.3f) * env * 0.55f);
        }
    }

    void AddHat(float[] b, int n, int at, float vol)
    {
        int len = (int)(SR * 0.03f);
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 120f);
            W(b, n, at + i, (Random.value * 2f - 1f) * env * vol);
        }
    }

    void AddBass(float[] b, int n, int at, int len, float freq, float vol)
    {
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SR;
            float phase = (freq * t) % 1f;
            float saw = 2f * phase - 1f;
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);
            float env = Mathf.Exp(-t * 1.6f);
            W(b, n, at + i, (saw * 0.55f + sine * 0.45f) * env * vol);
        }
    }

    void AddBlip(float[] b, int n, int at, int len, float freq, float vol)
    {
        for (int i = 0; i < len; i++)
        {
            float t = i / (float)SR;
            float sq = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
            float env = Mathf.Exp(-t * 4f);
            W(b, n, at + i, sq * env * vol);
        }
    }

    static void Normalize(float[] b, float peak)
    {
        float max = 0f;
        for (int i = 0; i < b.Length; i++) max = Mathf.Max(max, Mathf.Abs(b[i]));
        if (max < 1e-4f) return;
        float k = peak / max;
        for (int i = 0; i < b.Length; i++) b[i] *= k;
    }
}
