using UnityEngine;

/// Globalne perky kupene za peniaze. Trvale, levely v PlayerPrefs ("Perk_<id>").
/// Efekty citaju GameManager (money, combo) a WeaponHit (swing, reach).
public static class Perks
{
    public const int MAX = 5;

    // id, zobrazovany nazov, popis efektu
    public static readonly (string id, string name, string desc)[] LIST =
    {
        ("money", "Money Boost",  "+10% money per level"),
        ("combo", "Combo Window", "+0.25s combo window per level"),
        ("swing", "Swing Speed",  "+8% swing speed per level"),
        ("reach", "Reach",        "+0.2m reach per level"),
    };

    public static int  Level(string id)  => Mathf.Clamp(PlayerPrefs.GetInt("Perk_" + id, 0), 0, MAX);
    /// Cena ďalšieho levelu: 300 * (aktuálny level + 1).
    public static int  Cost(string id)   => 300 * (Level(id) + 1);
    public static bool CanBuy(string id) => Level(id) < MAX;

    /// Skúsi kúpiť ďalší level perku za peniaze z PlayerInventory; pri úspechu
    /// odpočíta cenu a uloží nový level do PlayerPrefs.
    public static bool TryBuy(string id)
    {
        if (!CanBuy(id)) return false;
        int cost = Cost(id);
        var inv = PlayerInventory.Instance;
        if (inv == null || inv.Money < cost) return false;
        inv.AddMoney(-cost);
        PlayerPrefs.SetInt("Perk_" + id, Level(id) + 1);
        PlayerPrefs.Save();
        return true;
    }

    // ---------- EFEKTY ----------
    /// Násobič peňazí: +10% za level.
    public static float MoneyMult()        => 1f + 0.10f * Level("money");
    /// Bonus k dĺžke combo okna v sekundách: +0.25s za level.
    public static float ComboWindowBonus() => 0.25f * Level("combo");
    /// Násobič rýchlosti švihu: +8% za level.
    public static float SwingMult()        => 1f + 0.08f * Level("swing");
    /// Bonus k dosahu v metroch: +0.2m za level.
    public static float ReachBonus()       => 0.20f * Level("reach");
}
