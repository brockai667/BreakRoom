using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string id;           // unikátny kľúč (uloženie)
    public string displayName;  // meno v UI
    public string description;  // popis v shope
    public int price;           // cena v $
    public int damage;          // poškodenie na hit
    public float splashRadius;  // polomer splash poškodenia (0 = žiadny)
    public float hitDistance;   // dosah zbrane
    public float swingSpeed;    // rýchlosť švihu (1 = normálna)
    public Color handColor;     // farba v ruke
    public Color handleColor;   // farba rukoväte

    // Všetky zbrane v hre
    public static WeaponData[] All = new WeaponData[]
    {
        new WeaponData {
            id="fists", displayName="Holé ruky", description="Zadarmo.\nŽiadna výhoda.",
            price=0, damage=1, splashRadius=0f, hitDistance=3.5f, swingSpeed=1.2f,
            handColor=new Color(0.85f,0.65f,0.5f), handleColor=new Color(0.7f,0.5f,0.35f)
        },
        new WeaponData {
            id="bat", displayName="Baseball palka", description="Klasická palka.\nSlabší splash.",
            price=150, damage=2, splashRadius=0.6f, hitDistance=4.0f, swingSpeed=1.0f,
            handColor=new Color(0.6f,0.35f,0.1f), handleColor=new Color(0.45f,0.25f,0.05f)
        },
        new WeaponData {
            id="gloves", displayName="Boxovacie rukavice", description="Rýchle údery.\nMalý splash.",
            price=300, damage=3, splashRadius=0.4f, hitDistance=3.2f, swingSpeed=1.8f,
            handColor=new Color(0.8f,0.1f,0.1f), handleColor=new Color(0.6f,0.05f,0.05f)
        },
        new WeaponData {
            id="hammer", displayName="Kladivo", description="Silné, pomalé.\nStredný splash.",
            price=600, damage=5, splashRadius=1.0f, hitDistance=3.8f, swingSpeed=0.75f,
            handColor=new Color(0.4f,0.4f,0.45f), handleColor=new Color(0.55f,0.35f,0.1f)
        },
        new WeaponData {
            id="axe", displayName="Sekera", description="Ostrá a ďaleká.\nVeľký splash.",
            price=1200, damage=7, splashRadius=1.4f, hitDistance=4.5f, swingSpeed=0.9f,
            handColor=new Color(0.55f,0.55f,0.6f), handleColor=new Color(0.35f,0.2f,0.05f)
        },
        new WeaponData {
            id="sledge", displayName="Búracie kladivo", description="Maximálna sila.\nObrovský splash.",
            price=2500, damage=12, splashRadius=2.0f, hitDistance=4.0f, swingSpeed=0.55f,
            handColor=new Color(0.2f,0.2f,0.22f), handleColor=new Color(0.3f,0.15f,0.05f)
        },
        new WeaponData {
            id="flamethrower", displayName="Plameňomet", description="Nepretržité poškodenie.\nMaximálny splash.",
            price=5000, damage=3, splashRadius=2.5f, hitDistance=5.0f, swingSpeed=1.0f,
            handColor=new Color(0.9f,0.35f,0.05f), handleColor=new Color(0.3f,0.3f,0.35f)
        },
    };

    public static WeaponData Get(string id)
    {
        foreach (var w in All) if (w.id == id) return w;
        return All[0];
    }
}
