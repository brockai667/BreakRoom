using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public int Money { get; private set; }
    public string EquippedId { get; private set; } = "fists";

    public const int MAX_UPGRADE = 3;

    public event System.Action OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    public bool Owns(string id)
    {
        return PlayerPrefs.GetInt("Own_" + id, id == "fists" ? 1 : 0) == 1;
    }

    public void GiveWeapon(string id)
    {
        PlayerPrefs.SetInt("Own_" + id, 1);
        PlayerPrefs.Save();
    }

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

    public void AddMoney(int amount)
    {
        Money += amount;
        Save();
        OnChanged?.Invoke();
    }

    // ---------- UPGRADY ----------
    public int UpgradeLevel(string id)
    {
        return Mathf.Clamp(PlayerPrefs.GetInt("Up_" + id, 0), 0, MAX_UPGRADE);
    }

    public int UpgradeCost(string id)
    {
        var w = WeaponData.Get(id);
        int lvl = UpgradeLevel(id);
        return Mathf.Max(80, w.price / 3) * (lvl + 1);
    }

    public bool CanUpgrade(string id)
    {
        return Owns(id) && UpgradeLevel(id) < MAX_UPGRADE;
    }

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

    public WeaponData GetEquipped() => WeaponData.Get(EquippedId);
}
