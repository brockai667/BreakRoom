// Assets/Editor/FactoryKitProbe.cs
// Odmeria reálne rozmery (AABB) modelov z factory-kitu a Kenney nábytku,
// aby sa kúsky dali ukladať presne na mriežku (spájať na doraz).
// Výsledok zapíše do Assets/kit_sizes.txt.

#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;

public class FactoryKitProbe
{
    const string FAC = "Assets/kenney_factory-kit_3.0/Models/FBX format/";
    const string FUR = "Assets/KenneyFurniture/Models/FBX format/";

    [MenuItem("BreakRoom/Art/Probe Kit Sizes")]
    static void Probe()
    {
        var sb = new StringBuilder();
        string[] fac = {
            "conveyor-long","conveyor","conveyor-corner","conveyor-cross","conveyor-junction-t",
            "machine","machine-bed","machine-fortified","machine-window","machine-window-bar","machine-connection-pipe",
            "hopper-round","hopper-square","hopper-high-round","hopper-high-square",
            "robot-arm-a","robot-arm-b",
            "pipe-large-long","pipe-large-bend","pipe-large-curve","pipe-large-valve","pipe-large-junction",
            "catwalk-straight","catwalk-corner","catwalk-stairs","scanner-high","scanner-low",
            "crane","crane-magnet","crane-lift",
            "structure-tall","structure-medium","structure-short","structure-high",
            "box-large","box-small","box-wide","box-long","arrow",
            "cog-a","cog-e","piston-round","piston-square","cone","floor","floor-large"
        };
        sb.AppendLine("=== FACTORY KIT (native scale) ===");
        foreach (var n in fac) Measure(FAC, n, sb);

        string[] fur = {
            "desk","chairDesk","chairModernCushion","computerScreen","laptop","books",
            "bookcaseClosed","bookcaseOpen","bookcaseClosedDoors","cabinetBedDrawer",
            "loungeChair","loungeSofa","pottedPlant","plantSmall1","trashcan",
            "lampSquareTable","lampRoundFloor","tableCross","tableRound","chair","chairRounded",
            "cabinetTelevision","televisionModern","rugRectangle","coatRackStanding","sideTable","radio"
        };
        sb.AppendLine("");
        sb.AppendLine("=== KENNEY FURNITURE (native scale) ===");
        foreach (var n in fur) Measure(FUR, n, sb);

        sb.AppendLine("");
        sb.AppendLine("=== BRICK PROJECT STUDIO (prefab, native) ===");
        string BPK = "Assets/Brick Project Studio/Apartment Kit/_Prefabs/";
        string[] bps = {
            BPK+"Furniture/Kitchen/Cabinets/Cabinet_Base_SD_01.prefab",
            BPK+"Furniture/Kitchen/Cabinets/Cabinet_Base_DD_01.prefab",
            BPK+"Furniture/Kitchen/Cabinets/Cabinet_Base_Sink_01.prefab",
            BPK+"Furniture/Kitchen/Cabinets/Cabinet_Base_Corner_01.prefab",
            BPK+"Furniture/Kitchen/Cabinets/Cabinet_Wall_DD_01.prefab",
            BPK+"Furniture/Kitchen/Cabinets/Cabinet_Tall_DD_01.prefab",
            BPK+"Furniture/Kitchen/Appliances/Fridge_01.prefab",
            BPK+"Furniture/Kitchen/Appliances/Stove_01.prefab",
            BPK+"Furniture/Kitchen/Appliances/Range_Hood.prefab",
            BPK+"Furniture/Living Room/Sofa_Apt_01.prefab",
            BPK+"Furniture/Living Room/Chair_Apt_01.prefab",
            BPK+"Furniture/Living Room/Table_Coffee_01.prefab",
            BPK+"Furniture/Bedroom/Bed_Apt_01_01.prefab",
            BPK+"Furniture/Bedroom/Dresser_Apt_01.prefab",
            BPK+"Props/Electronics/TV_Apt_01.prefab",
            BPK+"Props/Electronics/Monitor_Apt_01.prefab",
            BPK+"Props/Electronics/Computer_apt_01.prefab",
            BPK+"Props/Lighting/Lamp_Floor_Apt_01.prefab",
            BPK+"Props/Art/Rug_Apt_01.prefab",
            BPK+"Props/Kitchen/CoffeeMaker_Apt_01.prefab",
            BPK+"Props/Kitchen/Blender_Apt_01.prefab",
            BPK+"Props/Kitchen/Bowl_Apt_01.prefab",
        };
        foreach (var p in bps) MeasurePath(p, sb);

        string path = "Assets/kit_sizes.txt";
        System.IO.File.WriteAllText(path, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("✅ Probe done -> " + path);
    }

    static void MeasurePath(string fullPath, StringBuilder sb)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        string name = System.IO.Path.GetFileNameWithoutExtension(fullPath);
        if (model == null) { sb.AppendLine(name + ": MISSING (" + fullPath + ")"); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        go.transform.position = Vector3.zero; go.transform.rotation = Quaternion.identity;
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) { sb.AppendLine(name + ": (no renderer)"); Object.DestroyImmediate(go); return; }
        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        sb.AppendLine($"{name}: size=({b.size.x:0.00},{b.size.y:0.00},{b.size.z:0.00})  yMin={b.min.y:0.00} yMax={b.max.y:0.00}");
        Object.DestroyImmediate(go);
    }

    static void Measure(string folder, string name, StringBuilder sb)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(folder + name + ".fbx");
        if (model == null) { sb.AppendLine(name + ": MISSING"); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        // NEPREPISUJEM localScale – meriam natívnu veľkosť (akú má v hre pri S=1)
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) { sb.AppendLine(name + ": (no renderer)"); Object.DestroyImmediate(go); return; }
        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        sb.AppendLine($"{name}: size=({b.size.x:0.00},{b.size.y:0.00},{b.size.z:0.00})  yMin={b.min.y:0.00} yMax={b.max.y:0.00}");
        Object.DestroyImmediate(go);
    }
}
#endif
