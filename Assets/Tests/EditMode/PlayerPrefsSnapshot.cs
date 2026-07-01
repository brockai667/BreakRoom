using System.Collections.Generic;
using UnityEngine;

/// Pomocník pre testy: uloží pôvodné hodnoty daných PlayerPrefs kľúčov (alebo že
/// kľúč neexistoval) a vie ich presne vrátiť späť. Používa sa v [SetUp]/[TearDown],
/// nech testy nezasahujú do reálneho uloženého progresu hráča a sú deterministické.
/// Int a string kľúče sa pamätajú oddelene (nikdy nečíta kľúč iným accessorom,
/// než akým bol zapísaný — zmiešaný GetString/GetInt na tom istom kľúči nie je
/// bezpečný naprieč platformami).
public class PlayerPrefsSnapshot
{
    class Entry<T> { public bool hadKey; public T value; }

    readonly Dictionary<string, Entry<int>> ints = new();
    readonly Dictionary<string, Entry<string>> strings = new();

    /// Zapamätá si aktuálnu int hodnotu kľúča (ak ešte nebola zapamätaná) a nastaví ho na newValue.
    public void SetInt(string key, int newValue)
    {
        if (!ints.ContainsKey(key))
            ints[key] = new Entry<int> { hadKey = PlayerPrefs.HasKey(key), value = PlayerPrefs.GetInt(key, 0) };
        PlayerPrefs.SetInt(key, newValue);
    }

    /// Zapamätá si aktuálnu string hodnotu kľúča a nastaví ho na newValue (alebo ho zmaže, ak newValue je null).
    public void SetString(string key, string newValue)
    {
        if (!strings.ContainsKey(key))
            strings[key] = new Entry<string> { hadKey = PlayerPrefs.HasKey(key), value = PlayerPrefs.GetString(key, "") };
        if (newValue == null) PlayerPrefs.DeleteKey(key);
        else PlayerPrefs.SetString(key, newValue);
    }

    /// Zmaže int-typový kľúč (napr. "Own_id"/"Up_id") a zapamätá si ho na obnovenie ako int.
    public void Delete(string key)
    {
        if (!ints.ContainsKey(key))
            ints[key] = new Entry<int> { hadKey = PlayerPrefs.HasKey(key), value = PlayerPrefs.GetInt(key, 0) };
        PlayerPrefs.DeleteKey(key);
    }

    /// Vráti späť pôvodnú int hodnotu kľúča (alebo ho zmaže, ak predtým neexistoval). Volať v [TearDown].
    public void RestoreAsInt(string key)
    {
        if (!ints.TryGetValue(key, out var e)) return;
        if (e.hadKey) PlayerPrefs.SetInt(key, e.value);
        else PlayerPrefs.DeleteKey(key);
    }

    /// Vráti späť pôvodnú string hodnotu kľúča (alebo ho zmaže, ak predtým neexistoval). Volať v [TearDown].
    public void RestoreAsString(string key)
    {
        if (!strings.TryGetValue(key, out var e)) return;
        if (e.hadKey) PlayerPrefs.SetString(key, e.value);
        else PlayerPrefs.DeleteKey(key);
    }

    /// Vráti späť všetky doteraz zapamätané int kľúče naraz.
    public void RestoreAllAsInt()
    {
        foreach (var key in ints.Keys) RestoreAsInt(key);
    }
}
