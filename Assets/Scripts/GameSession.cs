using UnityEngine;

/// Drží stav medzi scénami počas behu hry (neprežije reštart aplikácie).
/// Slúži na výber mapy, počiatočný tab v hube a výsledky posledného kola
/// (na animáciu počítania peňazí v hube).
public static class GameSession
{
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

    public static void SetResult(int earned, int destroyed, float time,
                                 string grade = "C", bool cleared = false)
    {
        PendingEarned    = earned;
        PendingDestroyed = destroyed;
        PendingTime      = time;
        PendingGrade     = grade;
        PendingCleared   = cleared;
        HasPendingResult = true;
    }

    public static void ClearResult()
    {
        HasPendingResult = false;
        PendingEarned    = 0;
        PendingDestroyed = 0;
        PendingTime      = 0f;
        PendingGrade     = "C";
        PendingCleared   = false;
    }
}
