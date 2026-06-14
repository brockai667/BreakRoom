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
            id="fists", displayName="Bare Hands", description="Free.\nShort reach, weak hit.",
            price=0, damage=1, splashRadius=0f, hitDistance=3.0f, swingSpeed=1.2f,
            handColor=new Color(0.85f,0.65f,0.5f), handleColor=new Color(0.7f,0.5f,0.35f)
        },
        new WeaponData {
            id="bat", displayName="Baseball Bat", description="Classic bat.\nSolid hit, no splash.",
            price=150, damage=2, splashRadius=0f, hitDistance=3.8f, swingSpeed=1.0f,
            handColor=new Color(0.6f,0.35f,0.1f), handleColor=new Color(0.45f,0.25f,0.05f)
        },
        new WeaponData {
            id="gloves", displayName="Boxing Gloves", description="Fast hits.\nShort reach, no splash.",
            price=280, damage=2, splashRadius=0f, hitDistance=2.8f, swingSpeed=1.8f,
            handColor=new Color(0.8f,0.1f,0.1f), handleColor=new Color(0.6f,0.05f,0.05f)
        },
        new WeaponData {
            id="hammer", displayName="Hammer", description="Heavy, slower.\nSmall splash.",
            price=550, damage=3, splashRadius=0.3f, hitDistance=3.4f, swingSpeed=0.8f,
            handColor=new Color(0.4f,0.4f,0.45f), handleColor=new Color(0.55f,0.35f,0.1f)
        },
        new WeaponData {
            id="axe", displayName="Axe", description="Sharp, long reach.\nModerate splash.",
            price=1100, damage=4, splashRadius=0.4f, hitDistance=3.8f, swingSpeed=0.95f,
            handColor=new Color(0.55f,0.55f,0.6f), handleColor=new Color(0.35f,0.2f,0.05f)
        },
        new WeaponData {
            id="sledge", displayName="Sledgehammer", description="Strongest hit.\nMedium splash.",
            price=2200, damage=5, splashRadius=0.7f, hitDistance=3.6f, swingSpeed=0.6f,
            handColor=new Color(0.2f,0.2f,0.22f), handleColor=new Color(0.3f,0.15f,0.05f)
        },
        new WeaponData {
            id="flamethrower", displayName="Flamethrower", description="Continuous damage.\nLate-game unlock.",
            price=4500, damage=1, splashRadius=1.1f, hitDistance=4.2f, swingSpeed=1.0f,
            handColor=new Color(0.9f,0.35f,0.05f), handleColor=new Color(0.3f,0.3f,0.35f)
        },
        new WeaponData {
            id="crowbar", displayName="Crowbar", description="Quick and handy.\nNo splash.",
            price=450, damage=3, splashRadius=0f, hitDistance=3.7f, swingSpeed=1.15f,
            handColor=new Color(0.75f,0.25f,0.18f), handleColor=new Color(0.55f,0.18f,0.12f)
        },
        new WeaponData {
            id="katana", displayName="Katana", description="Fast wide slashes.\nSmall splash.",
            price=1800, damage=4, splashRadius=0.5f, hitDistance=4.2f, swingSpeed=1.5f,
            handColor=new Color(0.85f,0.86f,0.9f), handleColor=new Color(0.15f,0.15f,0.18f)
        },
        new WeaponData {
            id="chainsaw", displayName="Chainsaw", description="Rapid aggressive hits.\nSmall splash.",
            price=3000, damage=4, splashRadius=0.4f, hitDistance=3.5f, swingSpeed=1.7f,
            handColor=new Color(0.95f,0.5f,0.08f), handleColor=new Color(0.3f,0.3f,0.32f)
        },
        new WeaponData {
            id="bowling", displayName="Bowling Ball", description="Heavy single smash.\nMedium splash.",
            price=1600, damage=6, splashRadius=0.6f, hitDistance=3.3f, swingSpeed=0.7f,
            handColor=new Color(0.12f,0.12f,0.3f), handleColor=new Color(0.08f,0.08f,0.2f)
        },
        new WeaponData {
            id="grenade", displayName="Grenade", description="Throw it!\nArea explosion.",
            price=2400, damage=8, splashRadius=0f, hitDistance=4.0f, swingSpeed=1.0f,
            handColor=new Color(0.3f,0.4f,0.2f), handleColor=new Color(0.2f,0.25f,0.15f)
        },
    };

    public static WeaponData Get(string id)
    {
        foreach (var w in All) if (w.id == id) return w;
        return All[0];
    }

    // Progresia: na kupu zbrane v shope treba aspon tento level.
    public static int UnlockLevel(string id)
    {
        switch (id)
        {
            case "crowbar":      return 2;
            case "gloves":       return 3;
            case "hammer":       return 4;
            case "axe":          return 6;
            case "bowling":      return 7;
            case "grenade":      return 8;
            case "katana":       return 9;
            case "sledge":       return 11;
            case "chainsaw":     return 13;
            case "flamethrower": return 15;   // moc OP -> az neskoro
            default:             return 1;    // fists, bat
        }
    }
}
