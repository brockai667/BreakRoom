using UnityEngine;

/// Drží stav medzi scénami počas behu hry (neprežije reštart aplikácie).
/// Slúži na výber mapy, počiatočný tab v hube a výsledky posledného kola
/// (na animáciu počítania peňazí v hube).
public static class GameSession
{
    // Herný mód: Normal = bežné kolo, Survival = nekonečný mód (klok + respawn)
    public enum GameMode { Normal, Survival }
    public static GameMode Mode = GameMode.Normal;

    // Vybraná mapa, ktorú spustí tlačidlo START v hube (Obývačka = prvý level)
    public static string SelectedMap = "Obyvacka";

    // Ktorý tab sa otvorí pri vstupe do hubu ("Play", "Loadout", "Shop")
    public static string InitialHubTab = "Play";

    // --- Výsledky posledného kola (na vyhodnotenie v hube) ---
    public static bool   HasPendingResult = false;
    public static int    PendingEarned    = 0;
    public static int    PendingDestroyed = 0;
    public static float  PendingTime      = 0f;
    public static string PendingGrade     = "C";    // S/A/B/C/D
    public static bool   PendingCleared   = false;  // zničil hráč celú miestnosť?
    public static int    PendingBase      = 0;      // peniaze z rozbíjania (vrátane combo/cieľov)
    public static int    PendingBonus     = 0;      // koncový bonus (množstvo + clear)
    public static bool   PendingNewBest   = false;  // nový rekord na mape?

    // --- Survival výsledok (na end-screen + rebríček v hube) ---
    public static bool PendingSurvival  = false;
    public static int  PendingScore     = 0;
    public static int  PendingWave      = 0;
    public static int  PendingBestCombo = 0;
    public static bool PendingScoreBest = false;

    public static void SetResult(int earned, int destroyed, float time,
                                 string grade = "C", bool cleared = false,
                                 int baseMoney = 0, int bonus = 0)
    {
        PendingEarned    = earned;
        PendingDestroyed = destroyed;
        PendingTime      = time;
        PendingGrade     = grade;
        PendingCleared   = cleared;
        PendingBase      = baseMoney;
        PendingBonus     = bonus;
        PendingSurvival  = false;

        // Nový rekord (najvyšší zárobok na danej mape)
        string key = "Best_" + SelectedMap;
        PendingNewBest = earned > PlayerPrefs.GetInt(key, 0);
        if (PendingNewBest) { PlayerPrefs.SetInt(key, earned); PlayerPrefs.Save(); }

        HasPendingResult = true;
    }

    /// Výsledok survival behu: uloží lokálny rekord (skóre + čas) na mapu
    /// a celkový rekord pre Main Menu, a pripraví dáta pre end-screen v hube.
    public static void SetSurvivalResult(int score, int destroyed, float time,
                                         int wave, int bestCombo, int earned)
    {
        PendingEarned    = earned;
        PendingDestroyed = destroyed;
        PendingTime      = time;
        PendingGrade     = SurvivalGrade(score);
        PendingCleared   = false;
        PendingBase      = earned;
        PendingBonus     = 0;
        PendingNewBest   = false;

        PendingSurvival  = true;
        PendingScore     = score;
        PendingWave      = wave;
        PendingBestCombo = bestCombo;

        string key = "Survival_Best_" + SelectedMap;
        PendingScoreBest = score > PlayerPrefs.GetInt(key, 0);
        if (PendingScoreBest)
        {
            PlayerPrefs.SetInt(key, score);
            PlayerPrefs.SetFloat("Survival_BestTime_" + SelectedMap, time);
        }
        if (score > PlayerPrefs.GetInt("Survival_BestEver", 0))
            PlayerPrefs.SetInt("Survival_BestEver", score);
        PlayerPrefs.Save();

        HasPendingResult = true;
    }

    public static string SurvivalGrade(int score)
        => score >= 5000 ? "S"
         : score >= 3000 ? "A"
         : score >= 1500 ? "B"
         : score >=  600 ? "C" : "D";

    public static void ClearResult()
    {
        HasPendingResult = false;
        PendingEarned    = 0;
        PendingDestroyed = 0;
        PendingTime      = 0f;
        PendingGrade     = "C";
        PendingCleared   = false;
        PendingBase      = 0;
        PendingBonus     = 0;
        PendingNewBest   = false;
        PendingSurvival  = false;
        PendingScore     = 0;
        PendingWave      = 0;
        PendingBestCombo = 0;
        PendingScoreBest = false;
    }
}
