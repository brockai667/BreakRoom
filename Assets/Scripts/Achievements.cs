using UnityEngine;

/// Achievementy / mileniky. Self-bootstrapping, sleduje lifetime statistiky
/// (celkovo rozbite, najlepsie combo, vycistene miestnosti, najlepsie hodnotenie
/// kola S/A/B/C/D) v PlayerPrefs a pri prekroceni prahu odomkne achievement
/// -> odmena $ + hlaska.
public class Achievements : MonoBehaviour
{
    // id, nazov, typ ("smashed"/"combo"/"cleared"), prah, odmena $
    public static readonly (string id, string name, string type, int need, int reward)[] DEFS =
    {
        ("smash100",  "Demolisher",         "smashed",  100,  100),
        ("smash1000", "Wrecking Crew",      "smashed", 1000,  500),
        ("smash5000", "Total Destruction",  "smashed", 5000, 2000),
        ("combo10",   "Combo Starter",      "combo",     10,   80),
        ("combo25",   "Combo Master",       "combo",     25,  250),
        ("combo50",   "Unstoppable",        "combo",     50,  750),
        ("clear10",   "Cleaner",            "cleared",   10,  150),
        ("clear50",   "Janitor Supreme",    "cleared",   50,  600),
    };

    bool hadPending;
    int  maxComboThisRound;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("Achievements");
        DontDestroyOnLoad(go);
        go.AddComponent<Achievements>();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm != null && gm.roundActive)
            maxComboThisRound = Mathf.Max(maxComboThisRound, gm.comboCount);

        // Koniec kola = GameSession dostane vysledok (HasPendingResult false->true)
        bool pending = GameSession.HasPendingResult;
        if (pending && !hadPending)
        {
            CommitRound(GameSession.PendingDestroyed, GameSession.PendingCleared, maxComboThisRound, GameSession.PendingGrade);
            maxComboThisRound = 0;
        }
        hadPending = pending;
    }

    void CommitRound(int smashedThisRound, bool cleared, int maxCombo, string grade)
    {
        int smashed     = PlayerPrefs.GetInt("Stat_smashed", 0) + Mathf.Max(0, smashedThisRound);
        int bestCombo    = Mathf.Max(PlayerPrefs.GetInt("Stat_bestCombo", 0), maxCombo);
        int clearedTotal = PlayerPrefs.GetInt("Stat_cleared", 0) + (cleared ? 1 : 0);

        PlayerPrefs.SetInt("Stat_smashed", smashed);
        PlayerPrefs.SetInt("Stat_bestCombo", bestCombo);
        PlayerPrefs.SetInt("Stat_cleared", clearedTotal);

        // Najlepsie dosiahnute hodnotenie kola (S najlepsie, D najhorsie)
        string bestGrade = PlayerPrefs.GetString("Stat_bestGrade", "");
        if (bestGrade == "" || GradeRank(grade) > GradeRank(bestGrade))
            PlayerPrefs.SetString("Stat_bestGrade", grade);

        PlayerPrefs.Save();

        CheckAll(smashed, bestCombo, clearedTotal);
    }

    static int GradeRank(string grade) => grade switch
    {
        "S" => 5, "A" => 4, "B" => 3, "C" => 2, "D" => 1, _ => 0
    };

    void CheckAll(int smashed, int bestCombo, int cleared)
    {
        foreach (var d in DEFS)
        {
            if (PlayerPrefs.GetInt("Ach_" + d.id, 0) == 1) continue;   // uz odomknute
            int have = d.type == "smashed" ? smashed
                     : d.type == "combo"   ? bestCombo
                                           : cleared;
            if (have >= d.need) Unlock(d.id, d.name, d.reward);
        }
    }

    void Unlock(string id, string name, int reward)
    {
        PlayerPrefs.SetInt("Ach_" + id, 1);
        PlayerPrefs.Save();
        if (PlayerInventory.Instance != null) PlayerInventory.Instance.AddMoney(reward);
        if (Announcer.Instance != null) Announcer.Show("ACHIEVEMENT: " + name + "   +$" + reward, true);
        if (SfxManager.Instance != null) SfxManager.LevelUp();
    }
}
