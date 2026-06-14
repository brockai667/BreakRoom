using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Re-theme novych map (kopie Obyvacky) podla mena sceny: prefarbi velke
/// plochy (podlaha/steny/strop) A aj rozbitne predmety vlastnou paletou, aby
/// Garage/Kitchen nevyzerali identicky ako obyvacka (ine "veci na rozbitie").
public class RoomTheme : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("RoomTheme");
        DontDestroyOnLoad(go);
        var rt = go.AddComponent<RoomTheme>();
        SceneManager.sceneLoaded += (s, m) => rt.Apply(s.name);
        rt.Apply(SceneManager.GetActiveScene().name);
    }

    // Palety predmetov pre jednotlive temy.
    static readonly Color[] GarageItems =
    {
        new Color(0.50f, 0.20f, 0.12f),  // hrdza
        new Color(0.35f, 0.40f, 0.46f),  // oceľ
        new Color(0.22f, 0.23f, 0.25f),  // tmavá sivá
        new Color(0.25f, 0.32f, 0.22f),  // olejová zelená
        new Color(0.10f, 0.10f, 0.11f),  // pneumatika
        new Color(0.42f, 0.30f, 0.16f),  // drevo
        new Color(0.72f, 0.60f, 0.12f),  // žltý nástroj
    };
    static readonly Color[] KitchenItems =
    {
        new Color(0.90f, 0.90f, 0.88f),  // biela
        new Color(0.70f, 0.72f, 0.75f),  // nerez
        new Color(0.55f, 0.70f, 0.82f),  // keramická modrá
        new Color(0.86f, 0.82f, 0.70f),  // krémová
        new Color(0.75f, 0.20f, 0.18f),  // červený spotrebič
        new Color(0.58f, 0.78f, 0.68f),  // mentolová
        new Color(0.60f, 0.62f, 0.66f),  // chróm
    };
    static readonly Color[] OfficeItems =
    {
        new Color(0.16f, 0.17f, 0.20f),  // monitor / čierna technika
        new Color(0.50f, 0.52f, 0.56f),  // sivá skriňa
        new Color(0.50f, 0.38f, 0.24f),  // drevený stôl
        new Color(0.30f, 0.42f, 0.62f),  // modrá stolička
        new Color(0.88f, 0.88f, 0.85f),  // biely papier
        new Color(0.78f, 0.74f, 0.64f),  // béžová
        new Color(0.35f, 0.55f, 0.45f),  // zelená doska
    };
    static readonly Color[] FactoryItems =
    {
        new Color(0.45f, 0.47f, 0.50f),  // oceľ
        new Color(0.52f, 0.26f, 0.15f),  // hrdza
        new Color(0.80f, 0.70f, 0.12f),  // výstražná žltá
        new Color(0.25f, 0.26f, 0.28f),  // tmavý kov
        new Color(0.50f, 0.36f, 0.20f),  // drevená debna
        new Color(0.30f, 0.45f, 0.32f),  // olejová zelená
        new Color(0.62f, 0.30f, 0.10f),  // medená
    };

    void Apply(string scene)
    {
        Color wall, floor, ceil;
        Color[] items;
        if (scene == "Garage")
        {
            wall  = new Color(0.32f, 0.34f, 0.38f);
            floor = new Color(0.27f, 0.27f, 0.29f);
            ceil  = new Color(0.22f, 0.23f, 0.25f);
            items = GarageItems;
        }
        else if (scene == "Kitchen")
        {
            wall  = new Color(0.86f, 0.86f, 0.82f);
            floor = new Color(0.78f, 0.80f, 0.82f);
            ceil  = new Color(0.93f, 0.93f, 0.90f);
            items = KitchenItems;
        }
        else if (scene == "Office")
        {
            wall  = new Color(0.74f, 0.76f, 0.80f);   // svetlá kancelária
            floor = new Color(0.40f, 0.42f, 0.46f);   // koberec
            ceil  = new Color(0.92f, 0.93f, 0.94f);   // stropné panely
            items = OfficeItems;
        }
        else if (scene == "Factory")
        {
            wall  = new Color(0.45f, 0.46f, 0.48f);   // betón
            floor = new Color(0.30f, 0.30f, 0.32f);   // tmavá podlaha
            ceil  = new Color(0.36f, 0.37f, 0.39f);   // kovový strop
            items = FactoryItems;
        }
        else if (scene == "Bedroom")
        {
            wall  = new Color(0.62f, 0.56f, 0.66f);   // teplá levanduľová
            floor = new Color(0.46f, 0.34f, 0.24f);   // drevená podlaha
            ceil  = new Color(0.90f, 0.88f, 0.90f);   // svetlý strop
            items = null;                              // vlastné materiály z buildera necháme
        }
        else return;

        StartCoroutine(Tint(wall, floor, ceil, items));
    }

    IEnumerator Tint(Color wall, Color floor, Color ceil, Color[] items)
    {
        yield return null;   // pockaj na objekty v scene

        // 1) velke plochy podla mena
        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if (r == null) continue;
            string n = r.gameObject.name.ToLowerInvariant();
            Color c;
            if      (n.Contains("podlaha")) c = floor;
            else if (n.Contains("strop"))   c = ceil;
            else if (n.Contains("stena"))   c = wall;
            else continue;

            var m = r.material;
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        }

        // 2) rozbitne predmety prefarbi temovou paletou (deterministicky podla mena)
        if (items != null)
        {
            foreach (var b in FindObjectsByType<Breakable>(FindObjectsSortMode.None))
            {
                if (b == null || b.isChunk) continue;
                int idx = Mathf.Abs(b.gameObject.name.GetHashCode()) % items.Length;
                b.Retint(items[idx]);
            }
        }
    }
}
