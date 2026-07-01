using UnityEngine;

/// Singleton (DontDestroyOnLoad) pre peniaze, vlastníctvo/equip/upgrade zbraní.
/// Perzistuje v PlayerPrefs (kľúče "Money", "Equipped", "Own_<id>", "Up_<id>");
/// po každej zmene stavu uloží a vyvolá OnChanged.
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    /// Aktuálny zostatok hráča v $.
    public int Money { get; private set; }
    /// Id práve vybavenej zbrane (default "fists").
    public string EquippedId { get; private set; } = "fists";

    /// Maximálna úroveň vylepšenia zbrane.
    public const int MAX_UPGRADE = 3;

    /// Vyvolané po každej zmene peňazí/vlastníctva/equipu/upgrade.
    public event System.Action OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Application.isPlaying guard: DontDestroyOnLoad nemá v edit-mode zmysel a
        // takto sa dá PlayerInventory bezpečne vytvoriť aj v EditMode testoch.
        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        Money      = PlayerPrefs.GetInt("Money", 0);
        EquippedId = PlayerPrefs.GetString("Equipped", "fists");
        if (!Owns("fists")) GiveWeapon("fists");
    }

    void Save()
    {
        PlayerPrefs.SetInt("Money", Money);
        PlayerPrefs.SetString("Equipped", EquippedId);
        PlayerPrefs.Save();
    }

    /// Vlastní hráč danú zbraň? ("fists" vlastní vždy).
    public bool Owns(string id)
    {
        return PlayerPrefs.GetInt("Own_" + id, id == "fists" ? 1 : 0) == 1;
    }

    /// Označí zbraň ako vlastnenú (PlayerPrefs "Own_<id>"), bez odpočtu peňazí.
    public void GiveWeapon(string id)
    {
        PlayerPrefs.SetInt("Own_" + id, 1);
        PlayerPrefs.Save();
    }

    /// Kúpi zbraň za jej cenu z WeaponData, ak ju hráč ešte nevlastní a má dosť peňazí.
    public bool TryBuy(string id)
    {
        var w = WeaponData.Get(id);
        if (Owns(id) || Money < w.price) return false;
        Money -= w.price;
        GiveWeapon(id);
        Save();
        OnChanged?.Invoke();
        return true;
    }

    /// Vybaví vlastnenú zbraň a hneď ju premietne do WeaponHit/HandDisplay v scéne.
    public void Equip(string id)
    {
        if (!Owns(id)) return;
        EquippedId = id;
        Save();
        OnChanged?.Invoke();
        var wh = FindFirstObjectByType<WeaponHit>();
        if (wh != null) wh.ApplyWeapon(WeaponData.Get(id));
        var hd = FindFirstObjectByType<HandDisplay>();
        if (hd != null) hd.SetWeapon(WeaponData.Get(id));
    }

    /// Pripočíta (alebo odpočíta pri zápornom amount) peniaze; nikdy nejde pod nulu.
    public void AddMoney(int amount)
    {
        Money = Mathf.Max(0, Money + amount);
        Save();
        OnChanged?.Invoke();
    }

    // ---------- UPGRADY ----------
    /// Aktuálna úroveň vylepšenia zbrane (0..MAX_UPGRADE).
    public int UpgradeLevel(string id)
    {
        return Mathf.Clamp(PlayerPrefs.GetInt("Up_" + id, 0), 0, MAX_UPGRADE);
    }

    /// Cena ďalšieho levelu upgrade: max(80, cena/3) * (aktuálny level + 1).
    public int UpgradeCost(string id)
    {
        var w = WeaponData.Get(id);
        int lvl = UpgradeLevel(id);
        return Mathf.Max(80, w.price / 3) * (lvl + 1);
    }

    /// Dá sa zbraň ešte vylepšiť (vlastnená a pod MAX_UPGRADE)?
    public bool CanUpgrade(string id)
    {
        return Owns(id) && UpgradeLevel(id) < MAX_UPGRADE;
    }

    /// Kúpi ďalší level upgrade za UpgradeCost; ak je zbraň práve vybavená, ihneď ju znova aplikuje na WeaponHit.
    public bool TryUpgrade(string id)
    {
        if (!CanUpgrade(id)) return false;
        int cost = UpgradeCost(id);
        if (Money < cost) return false;
        Money -= cost;
        PlayerPrefs.SetInt("Up_" + id, UpgradeLevel(id) + 1);
        Save();
        OnChanged?.Invoke();
        if (EquippedId == id)
        {
            var wh = FindFirstObjectByType<WeaponHit>();
            if (wh != null) wh.ApplyWeapon(WeaponData.Get(id));
        }
        return true;
    }

    /// Dátový záznam aktuálne vybavenej zbrane.
    public WeaponData GetEquipped() => WeaponData.Get(EquippedId);
}
