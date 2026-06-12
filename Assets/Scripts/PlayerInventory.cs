using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public int Money { get; private set; }
    public string EquippedId { get; private set; } = "fists";

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
        // Uisti sa, že vždy vlastníme holé ruky
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
        // Notify weapon hit in current scene
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

    public WeaponData GetEquipped() => WeaponData.Get(EquippedId);
}
