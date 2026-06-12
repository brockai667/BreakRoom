using UnityEngine;
using UnityEngine.SceneManagement;

/// Náhodný cieľ/výzva na kolo. Pri splnení dá bonus a hlášku.
/// Postaví sa sám z kódu, beží len v herných scénach. Stav číta HUD.
public class Objectives : MonoBehaviour
{
    public static Objectives Instance;

    static readonly string[] SKIP = { "Hub", "MainMenu", "Shop", "Collection" };

    enum Kind { Electronics, Golden, UnderTime, Count }

    Kind   kind;
    string desc = "";
    int    target;
    int    progress;
    bool   done;
    bool   active;
    int    bonus;
    float  timeLimit;
    float  sampleDelay;   // počkaj kým SpecialObjects označí veci

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("Objectives");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<Objectives>();
        SceneManager.sceneLoaded += (s, m) => Instance.OnScene(s.name);
        Instance.OnScene(SceneManager.GetActiveScene().name);
    }

    void OnScene(string scene)
    {
        active = System.Array.IndexOf(SKIP, scene) < 0;
        done = false; progress = 0; sampleDelay = 0.5f; desc = "";
        if (!active) return;

        // Vyber náhodný cieľ
        kind = (Kind)Random.Range(0, 4);
        switch (kind)
        {
            case Kind.Golden:
                target = 1; bonus = 60;
                desc = "Nájdi a rozbi zlatý predmet";
                break;
            case Kind.UnderTime:
                target = 1; bonus = 80; timeLimit = 75f;
                desc = "Vyčisti miestnosť pod 1:15";
                break;
            case Kind.Count:
                target = 25; bonus = 40;
                desc = "Rozbi aspoň 25 vecí";
                break;
            default: // Electronics - cieľ doplníme po vzorkovaní
                kind = Kind.Electronics; target = 0; bonus = 70;
                desc = "Rozbi všetku elektroniku";
                break;
        }
    }

    /// Hlásenie z Breakable pri rozbití (golden/electronic).
    public static void NotifyBreak(bool golden, bool electronic)
    {
        if (Instance == null || !Instance.active || Instance.done) return;
        if (Instance.kind == Kind.Golden && golden) Instance.progress = 1;
        if (Instance.kind == Kind.Electronics && electronic) Instance.progress++;
    }

    void Update()
    {
        if (!active || done) return;
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Elektronika: spočítaj koľko jej v mape je (po označení SpecialObjects)
        if (kind == Kind.Electronics && target == 0)
        {
            sampleDelay -= Time.unscaledDeltaTime;
            if (sampleDelay <= 0f)
            {
                int n = 0;
                foreach (var b in FindObjectsByType<Breakable>(FindObjectsSortMode.None))
                    if (b != null && b.electronic && !b.isChunk) n++;
                if (n <= 0) { kind = Kind.Count; target = 25; bonus = 40; desc = "Rozbi aspoň 25 vecí"; }
                else { target = n; desc = "Rozbi všetku elektroniku"; }
            }
            return;
        }

        // Priebežné ciele
        if (kind == Kind.Count) progress = gm.destroyedCount;

        bool complete =
            kind == Kind.Golden      ? progress >= 1 :
            kind == Kind.Electronics ? (target > 0 && progress >= target) :
            kind == Kind.Count       ? gm.destroyedCount >= target :
            /* UnderTime */            gm.DestructionPct() >= 0.999f && gm.elapsedTime <= timeLimit;

        if (complete) Complete(gm);
    }

    void Complete(GameManager gm)
    {
        done = true;
        gm.AddRoundMoney(bonus);
        if (Announcer.Instance != null) Announcer.Show("CIEĽ SPLNENÝ!  +$" + bonus, true);
        if (SfxManager.Instance != null) SfxManager.Coin();
    }

    /// Text do HUD.
    public string HudText()
    {
        if (!active || string.IsNullOrEmpty(desc)) return "";
        if (done) return "✓ " + desc + "  +$" + bonus;
        if (kind == Kind.Electronics && target > 0) return $"Cieľ: {desc} ({progress}/{target})";
        if (kind == Kind.Count) return $"Cieľ: {desc} ({progress}/{target})";
        return "Cieľ: " + desc;
    }
}
