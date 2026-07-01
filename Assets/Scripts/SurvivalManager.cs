using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// SURVIVAL mód: miestnosť sa sústavne dopĺňa, klok sa míňa a rozbíjanie
/// ho dopĺňa (combo = väčší časový bonus). Každých WAVE_SECONDS pribudne vlna
/// (rýchlejší respawn, tvrdšie objekty). Pri kloku 0 beh skončí a uloží sa
/// lokálny rekord skóre na mapu. Beží len v herných scénach pri Mode==Survival.
/// Recykluje GameManager (combo/AwardBreak), Breakable a juice.
public class SurvivalManager : MonoBehaviour
{
    public static SurvivalManager Instance { get; private set; }

    // ── ladiace konštanty ──
    const float START_CLOCK        = 30f;   // počiatočný čas
    const float MAX_CLOCK          = 45f;   // strop (nedá sa naťahovať donekonečna)
    const float BASE_TIME_PER_BREAK= 0.8f;  // základný čas za rozbitie
    const float WAVE_SECONDS       = 30f;   // dĺžka vlny
    const int   TARGET_MIN         = 8;
    const int   TARGET_MAX         = 16;

    // ── stav behu ──
    float clock;
    float runTime;
    float waveTimer;
    float respawnDelay = 1.2f;
    float spawnCooldown;
    int   targetAlive  = 12;
    /// Aktuálne číslo vlny (rastie každých WAVE_SECONDS, sprísňuje respawn a cieľový počet objektov).
    public int Wave { get; private set; }
    /// Aktuálne skóre behu (rastie o pay*2 pri každom rozbití).
    public int Score { get; private set; }
    /// Najvyššie dosiahnuté combo v tomto behu (na end-screen).
    public int BestCombo { get; private set; }
    /// Zostávajúci čas (klok) v sekundách.
    public float Clock => clock;
    bool ended;

    struct Slot { public Vector3 pos; }
    readonly List<Slot> slots = new();
    readonly List<GameObject> tracked = new();
    PauseMenu pause;

    // ── bootstrap: vytvor inštanciu len v herných scénach pri survival móde ──
    static readonly string[] SKIP = { "Hub", "MainMenu", "Shop", "Collection" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded += (s, m) => OnScene(s.name);
        OnScene(SceneManager.GetActiveScene().name);
    }

    static void OnScene(string scene)
    {
        bool gameplay = System.Array.IndexOf(SKIP, scene) < 0;
        if (!gameplay || GameSession.Mode != GameSession.GameMode.Survival) return;
        if (Instance != null) return;
        var go = new GameObject("SurvivalManager");
        go.AddComponent<SurvivalManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        Wave = 1; Score = 0; BestCombo = 0;
        clock = START_CLOCK;
        pause = FindFirstObjectByType<PauseMenu>();

        // zaznamenaj spawn pointy z pôvodných (scénových) rozbitných vecí
        foreach (var b in FindObjectsByType<Breakable>(FindObjectsSortMode.None))
        {
            if (b == null || b.isChunk) continue;
            slots.Add(new Slot { pos = b.transform.position });
            tracked.Add(b.gameObject);     // pôvodný nábytok = prvá vlna
        }
        targetAlive = Mathf.Clamp(slots.Count, TARGET_MIN, TARGET_MAX);

        if (Announcer.Instance != null) Announcer.Show("SURVIVAL!", true);
    }

    void Update()
    {
        if (ended) return;
        if (pause == null) pause = FindFirstObjectByType<PauseMenu>();
        if (pause != null && pause.IsPaused) return;

        float dt = Time.deltaTime;
        runTime += dt;
        clock   -= dt;

        // najlepšie combo (na end-screen)
        if (GameManager.Instance != null && GameManager.Instance.comboCount > BestCombo)
            BestCombo = GameManager.Instance.comboCount;

        // ── vlny: zrýchli respawn, sprísni objekty ──
        waveTimer += dt;
        if (waveTimer >= WAVE_SECONDS)
        {
            waveTimer -= WAVE_SECONDS;
            Wave++;
            respawnDelay = Mathf.Max(0.4f, respawnDelay - 0.15f);
            targetAlive  = Mathf.Min(TARGET_MAX, targetAlive + 1);
            if (Announcer.Instance != null) Announcer.Show("WAVE " + Wave + "!", true);
            if (SfxManager.Instance != null) SfxManager.Sting();
        }

        // ── doplň miestnosť na cieľový počet ──
        tracked.RemoveAll(g => g == null);
        spawnCooldown -= dt;
        if (tracked.Count < targetAlive && spawnCooldown <= 0f && slots.Count > 0)
        {
            SpawnOne();
            spawnCooldown = respawnDelay;
        }

        // ── HUD klok (scénový timerText) + červené pulzovanie pri nízkom kloku ──
        var tt = GameManager.Instance != null ? GameManager.Instance.timerText : null;
        if (tt != null)
        {
            tt.text = Mathf.CeilToInt(Mathf.Max(0f, clock)) + "s";
            if (clock <= 5f)
            {
                float k = Mathf.PingPong(Time.unscaledTime * 6f, 1f);
                tt.color = Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(1f, 0.15f, 0.1f), k);
            }
            else tt.color = Color.white;
        }

        if (clock <= 0f) EndSurvival();
    }

    /// Volá GameManager.AwardBreak po každom rozbití: pridá skóre + čas.
    public void OnBreak(int pay, float mult)
    {
        if (ended) return;

        Score += Mathf.Max(5, Mathf.RoundToInt(pay * 2f));

        float gain = BASE_TIME_PER_BREAK;
        gain *= Mathf.Lerp(1f, 1.7f, Mathf.InverseLerp(1f, 3f, mult));   // combo bonus
        gain += Mathf.Min(1.0f, pay * 0.03f);                            // bonus za hodnotu
        clock = Mathf.Min(MAX_CLOCK, clock + gain);
    }

    void SpawnOne()
    {
        var slot = slots[Random.Range(0, slots.Count)];

        // veľkosť debny (občas vyššia bedňa)
        float w = Random.Range(0.55f, 0.9f);
        float h = Random.value < 0.3f ? Random.Range(0.9f, 1.3f) : w;
        Vector3 size = new Vector3(w, h, w);

        var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "SurvivalCrate";
        Vector3 jitter = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));
        crate.transform.position   = new Vector3(slot.pos.x, h * 0.5f + 0.03f, slot.pos.z) + jitter;
        crate.transform.localScale = size;
        crate.transform.rotation   = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // typ + farba
        bool golden    = Random.value < 0.08f;
        bool explosive = !golden && Random.value < 0.06f;
        Color col = golden    ? new Color(1f, 0.82f, 0.2f)
                  : explosive ? new Color(0.7f, 0.16f, 0.12f)
                              : new Color(Random.Range(0.42f, 0.56f), Random.Range(0.30f, 0.38f), Random.Range(0.14f, 0.2f));
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader") mat = new Material(Shader.Find("Standard"));
        mat.color = col; if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
        crate.GetComponent<Renderer>().material = mat;

        var rb = crate.AddComponent<Rigidbody>(); rb.isKinematic = true;

        var b = crate.AddComponent<Breakable>();
        b.golden = golden; b.explosive = explosive;
        // hp/reward/xp dopočíta Breakable.Configure podľa veľkosti

        tracked.Add(crate);
    }

    /// Ukončenie behu (klok 0 alebo quit z pauzy). Uloží rekord a ide do hubu.
    public void EndSurvival()
    {
        if (ended) return;
        ended = true;

        int destroyed = GameManager.Instance != null ? GameManager.Instance.destroyedCount : 0;
        int earned    = GameManager.Instance != null ? GameManager.Instance.roundMoney : 0;
        if (GameManager.Instance != null) GameManager.Instance.roundActive = false;

        if (Announcer.Instance != null) Announcer.Show("TIME!", true);

        GameSession.SetSurvivalResult(Score, destroyed, runTime, Wave, BestCombo, earned);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Hub");
    }

    /// Vynútené okamžité ukončenie behu (napr. z pauzy) - alias pre EndSurvival.
    public void EndNow() => EndSurvival();
}
