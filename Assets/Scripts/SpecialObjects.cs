using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Za behu označí niektoré rozbitné veci v mape ako špeciálne:
///  - výbušné (sudy, fľaše, nádrže) -> reťazová reakcia + otras
///  - elektronika (TV, monitor, PC, lampa) -> iskry
///  - náhodné "zlaté" veci -> bonusové peniaze
/// Funguje vo všetkých mapách bez úpravy scén.
public class SpecialObjects : MonoBehaviour
{
    static readonly string[] EXPLOSIVE = { "barrel", "sud", "canister", "tank", "gas", "propan", "extinguisher", "hasiac", "bomb", "drum", "fuel" };
    static readonly string[] ELECTRONIC = { "tv", "television", "monitor", "screen", "pc", "computer", "laptop", "radio", "lamp", "console", "machine", "robot" };

    static readonly Color GOLD = new Color(1f, 0.78f, 0.12f);
    static readonly Color DANGER = new Color(0.85f, 0.22f, 0.12f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("SpecialObjects");
        DontDestroyOnLoad(go);
        SceneManager.sceneLoaded += (s, m) => go.GetComponent<SpecialObjects>().Promote(s.name);
        go.AddComponent<SpecialObjects>().Promote(SceneManager.GetActiveScene().name);
    }

    void Promote(string scene)
    {
        if (scene == "Hub" || scene == "MainMenu" || scene == "Shop") return;

        var all = FindObjectsByType<Breakable>(FindObjectsSortMode.None);
        int goldenLeft = 6;                 // max zlatých vecí na mapu
        var plain = new List<Breakable>();

        foreach (var b in all)
        {
            if (b == null || b.isChunk) continue;
            string n = b.gameObject.name.ToLowerInvariant();

            if (Matches(n, EXPLOSIVE))
            {
                b.explosive = true;
                Tint(b, DANGER);
            }
            else if (Matches(n, ELECTRONIC))
            {
                b.electronic = true;
            }
            else plain.Add(b);
        }

        // náhodné zlaté veci z bežných
        Shuffle(plain);
        for (int i = 0; i < plain.Count && goldenLeft > 0; i++)
        {
            if (Random.value < 0.10f)
            {
                plain[i].golden = true;
                Tint(plain[i], GOLD);
                goldenLeft--;
            }
        }

        // JACKPOT: jedna veľká "mini-boss" vec za kolo (veľa HP, veľká odmena)
        Breakable boss = null; float bossSize = 0f;
        foreach (var b in plain)
        {
            if (b == null) continue;
            var c = b.GetComponent<Collider>();
            float s = c != null ? c.bounds.size.magnitude : b.transform.lossyScale.magnitude;
            if (s > bossSize) { bossSize = s; boss = b; }
        }
        if (boss != null)
        {
            boss.jackpot = true;
            boss.golden  = true;
            Tint(boss, new Color(1f, 0.85f, 0.2f));
            var lgo = new GameObject("JackpotGlow");
            lgo.transform.SetParent(boss.transform, false);
            lgo.transform.localPosition = Vector3.zero;
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point; l.color = new Color(1f, 0.8f, 0.2f); l.range = 4.5f; l.intensity = 2.2f;
        }
    }

    static bool Matches(string name, string[] keys)
    {
        foreach (var k in keys) if (name.Contains(k)) return true;
        return false;
    }

    static void Tint(Breakable b, Color c)
    {
        var rend = b.GetComponent<Renderer>();
        if (rend == null) rend = b.GetComponentInChildren<Renderer>();
        if (rend == null) return;
        var m = rend.material;            // inštancia, neovplyvní ostatné
        m.color = c;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * 0.5f); }
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
