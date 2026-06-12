using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// Centrálny zvukový systém pre celú hru. Všetky zvuky sú generované
/// procedurálne za behu (žiadne .wav súbory netreba importovať), takže
/// fungujú v každej scéne bez úprav.
///
/// - rozbitie veci (smash)         -> SfxManager.Break(pos)
/// - úder bez rozbitia (thud)      -> SfxManager.Hit(pos)
/// - švih zbraňou (whoosh)         -> SfxManager.Swing()
/// - plameňomet (loop)             -> SfxManager.FlameOn() / FlameOff()
/// - kliknutie v UI                -> SfxManager.UI()   (+ auto na všetkých tlačidlách)
/// - peniaze / level up            -> SfxManager.Coin() / SfxManager.LevelUp()
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    const int SR = 44100;

    AudioClip[] breakClips;
    AudioClip   hitClip;
    AudioClip[] swingClips;
    AudioClip   flameClip;
    AudioClip   uiClip;
    AudioClip   coinClip;
    AudioClip   levelClip;
    AudioClip   boomClip;
    AudioClip   zapClip;
    AudioClip   stingClip;

    AudioSource flameSource;   // looping plameňomet
    AudioSource uiSource;      // 2D UI / peniaze

    // ---------- BOOTSTRAP (žiadne úpravy scén netreba) ----------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("SfxManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<SfxManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        GenerateClips();

        flameSource = gameObject.AddComponent<AudioSource>();
        flameSource.clip = flameClip; flameSource.loop = true; flameSource.playOnAwake = false;
        flameSource.volume = 0.0f; flameSource.spatialBlend = 0f;

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false; uiSource.spatialBlend = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureListener();
        HookButtons();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        FlameOff();
        EnsureListener();
        HookButtons();
    }

    // Zaisti, že v scéne je AudioListener na kamere (inak nepočuť nič / zle 3D)
    void EnsureListener()
    {
        if (FindFirstObjectByType<AudioListener>() != null) return;
        var cam = Camera.main; if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam != null && cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();
    }

    // Pridaj klik-zvuk na všetky tlačidlá v aktuálnej scéne
    void HookButtons()
    {
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            b.onClick.AddListener(UIClick);
    }

    void UIClick() => UI();

    // ---------- VEREJNÉ API ----------
    public static void Break(Vector3 pos)
    {
        if (Instance == null || Instance.breakClips == null) return;
        var c = Instance.breakClips[Random.Range(0, Instance.breakClips.Length)];
        Instance.PlayAt(c, pos, Random.Range(0.85f, 1.25f), 0.9f);
    }

    public static void Hit(Vector3 pos)
    {
        if (Instance == null) return;
        Instance.PlayAt(Instance.hitClip, pos, Random.Range(0.9f, 1.15f), 0.55f);
    }

    public static void Swing()
    {
        if (Instance == null || Instance.swingClips == null) return;
        var c = Instance.swingClips[Random.Range(0, Instance.swingClips.Length)];
        Instance.uiSource.PlayOneShot(c, 0.35f);
    }

    public static void UI()
    {
        if (Instance == null) return;
        Instance.uiSource.pitch = Random.Range(0.97f, 1.05f);
        Instance.uiSource.PlayOneShot(Instance.uiClip, 0.5f);
    }

    public static void Coin()
    {
        if (Instance == null) return;
        Instance.uiSource.pitch = 1f;
        Instance.uiSource.PlayOneShot(Instance.coinClip, 0.5f);
    }

    public static void LevelUp()
    {
        if (Instance == null) return;
        Instance.uiSource.pitch = 1f;
        Instance.uiSource.PlayOneShot(Instance.levelClip, 0.6f);
    }

    public static void Boom(Vector3 pos)
    {
        if (Instance == null) return;
        Instance.PlayAt(Instance.boomClip, pos, Random.Range(0.9f, 1.1f), 1f);
    }

    public static void Zap(Vector3 pos)
    {
        if (Instance == null) return;
        Instance.PlayAt(Instance.zapClip, pos, Random.Range(0.95f, 1.2f), 0.5f);
    }

    public static void Sting()
    {
        if (Instance == null) return;
        Instance.uiSource.pitch = 1f;
        Instance.uiSource.PlayOneShot(Instance.stingClip, 0.55f);
    }

    public static void FlameOn()
    {
        if (Instance == null || Instance.flameSource == null) return;
        if (!Instance.flameSource.isPlaying) Instance.flameSource.Play();
        Instance.flameSource.volume = 0.45f;
    }

    public static void FlameOff()
    {
        if (Instance == null || Instance.flameSource == null) return;
        Instance.flameSource.volume = 0f;
        Instance.flameSource.Stop();
    }

    // ---------- PREHRÁVANIE 3D ----------
    void PlayAt(AudioClip clip, Vector3 pos, float pitch, float vol)
    {
        if (clip == null) return;
        var go = new GameObject("sfx");
        go.transform.position = pos;
        var src = go.AddComponent<AudioSource>();
        src.clip = clip; src.pitch = pitch; src.volume = vol;
        src.spatialBlend = 0.7f; src.minDistance = 2f; src.maxDistance = 30f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.Play();
        Destroy(go, clip.length / Mathf.Max(0.1f, pitch) + 0.1f);
    }

    // ---------- GENEROVANIE ZVUKOV ----------
    void GenerateClips()
    {
        breakClips = new[] {
            MakeBreak(0.42f, 90f),    // drevo / nábytok
            MakeBreak(0.30f, 150f),   // sklo / menšie
            MakeBreak(0.55f, 60f),    // ťažké / kov
        };
        hitClip   = MakeThud(0.16f, 95f);
        swingClips = new[] { MakeSwing(0.22f), MakeSwing(0.26f) };
        flameClip = MakeFlameLoop(1.0f);
        uiClip    = MakeBlip(0.06f, 760f);
        coinClip  = MakeCoin();
        levelClip = MakeLevel();
        boomClip  = MakeBoom(0.7f);
        zapClip   = MakeZap(0.18f);
        stingClip = MakeSting(0.28f);
    }

    // Výbuch: hlboké dunenie + nárazový šum
    AudioClip MakeBoom(float dur)
    {
        int n = (int)(SR * dur); var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 5f);
            float rumble = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(78f, 36f, t / dur) * t) * 0.6f;
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 8f) * 0.6f;
            float crack = i < 400 ? (Random.value * 2f - 1f) * (1f - i / 400f) * 0.5f : 0f;
            d[i] = Mathf.Clamp(rumble * env + noise + crack, -1f, 1f);
        }
        return MakeClip("boom", d);
    }

    // Elektrické iskrenie: bzučivý šum s klesajúcim tónom
    AudioClip MakeZap(float dur)
    {
        int n = (int)(SR * dur); var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 16f);
            float f = Mathf.Lerp(1400f, 300f, t / dur);
            float buzz = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t));
            float noise = Random.value * 2f - 1f;
            float am = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 60f * t);
            d[i] = Mathf.Clamp((buzz * 0.4f + noise * 0.4f) * am * env, -1f, 1f);
        }
        return MakeClip("zap", d);
    }

    // Hlásičový "sting": jasný úderový akord
    AudioClip MakeSting(float dur)
    {
        int n = (int)(SR * dur); var d = new float[n];
        float[] f = { 392f, 523f, 784f };
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 9f);
            float s = 0f; for (int k = 0; k < f.Length; k++) s += Mathf.Sin(2f * Mathf.PI * f[k] * t);
            s /= f.Length;
            float atk = i < 300 ? (Random.value * 2f - 1f) * (1f - i / 300f) * 0.3f : 0f;
            d[i] = Mathf.Clamp((s * 0.7f + atk) * env, -1f, 1f);
        }
        return MakeClip("sting", d);
    }

    AudioClip MakeClip(string name, float[] data)
    {
        var c = AudioClip.Create(name, data.Length, 1, SR, false);
        c.SetData(data, 0);
        return c;
    }

    // Rozbitie: šum + nízky "tresk" + niekoľko praskov
    AudioClip MakeBreak(float dur, float thumpHz)
    {
        int n = (int)(SR * dur);
        var d = new float[n];
        // pripravené časy praskov
        int cracks = Random.Range(4, 8);
        var crackAt = new float[cracks];
        for (int k = 0; k < cracks; k++) crackAt[k] = Random.Range(0f, dur * 0.7f);

        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 11f);
            float noise = (Random.value * 2f - 1f) * 0.55f * env;
            // nízky náraz, frekvencia mierne klesá
            float f = thumpHz * (1f - 0.4f * t / dur);
            float thump = Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * 16f) * 0.55f;
            // ostré prasky
            float crk = 0f;
            for (int k = 0; k < cracks; k++)
            {
                float dt = t - crackAt[k];
                if (dt >= 0f) crk += (Random.value * 2f - 1f) * Mathf.Exp(-dt * 90f) * 0.4f;
            }
            d[i] = Mathf.Clamp(noise + thump + crk, -1f, 1f);
        }
        return MakeClip("break", d);
    }

    // Úder bez rozbitia: krátky tupý "tok"
    AudioClip MakeThud(float dur, float hz)
    {
        int n = (int)(SR * dur);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float click = (i < 200) ? (Random.value * 2f - 1f) * (1f - i / 200f) * 0.4f : 0f;
            float body = Mathf.Sin(2f * Mathf.PI * hz * t) * Mathf.Exp(-t * 26f) * 0.7f;
            d[i] = Mathf.Clamp(click + body, -1f, 1f);
        }
        return MakeClip("hit", d);
    }

    // Švih zbraňou: krátky "whoosh" (pásmový šum s nábehom a doznením)
    AudioClip MakeSwing(float dur)
    {
        int n = (int)(SR * dur);
        var d = new float[n];
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float x = t / dur;
            float env = Mathf.Sin(x * Mathf.PI);          // 0->1->0
            float white = Random.value * 2f - 1f;
            prev = Mathf.Lerp(prev, white, 0.12f);        // jemný low-pass -> "vzduch"
            d[i] = Mathf.Clamp(prev * env * 0.6f, -1f, 1f);
        }
        return MakeClip("swing", d);
    }

    // Plameňomet: hučiaci low-pass šum s amplitúdovou moduláciou, plynulý loop
    AudioClip MakeFlameLoop(float dur)
    {
        int n = (int)(SR * dur);
        var d = new float[n];
        float lp = 0f, lp2 = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float white = Random.value * 2f - 1f;
            lp  = Mathf.Lerp(lp,  white, 0.08f);          // hlavné dunenie
            lp2 = Mathf.Lerp(lp2, white, 0.30f);          // syčanie
            float roar = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 7f * t);
            d[i] = Mathf.Clamp((lp * 0.8f + lp2 * 0.35f) * roar, -1f, 1f);
        }
        // jemný cross-fade na bezšvíkový loop
        int f = Mathf.Min(1500, n / 4);
        for (int i = 0; i < f; i++)
        {
            float k = i / (float)f;
            d[i] = Mathf.Lerp(d[n - f + i], d[i], k);
        }
        return MakeClip("flame", d);
    }

    // UI klik
    AudioClip MakeBlip(float dur, float hz)
    {
        int n = (int)(SR * dur);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 55f);
            d[i] = Mathf.Sin(2f * Mathf.PI * hz * t) * env * 0.6f;
        }
        return MakeClip("ui", d);
    }

    // Cinknutie mincí (dva tóny)
    AudioClip MakeCoin()
    {
        float dur = 0.18f; int n = (int)(SR * dur); var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            float hz = t < 0.06f ? 980f : 1460f;
            float env = Mathf.Exp(-((t % 0.06f)) * 40f);
            d[i] = Mathf.Sin(2f * Mathf.PI * hz * t) * env * 0.5f;
        }
        return MakeClip("coin", d);
    }

    // Level up (vzostupné tri tóny)
    AudioClip MakeLevel()
    {
        float dur = 0.42f; int n = (int)(SR * dur); var d = new float[n];
        float[] hz = { 660f, 880f, 1175f };
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SR;
            int step = Mathf.Min(2, (int)(t / 0.14f));
            float lt = t - step * 0.14f;
            float env = Mathf.Exp(-lt * 12f);
            d[i] = Mathf.Sin(2f * Mathf.PI * hz[step] * t) * env * 0.45f;
        }
        return MakeClip("level", d);
    }
}
