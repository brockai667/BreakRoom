using System.Linq;
using UnityEditor;
using UnityEngine;

/// Vygeneruje nove mapy skopirovanim funkcnej sceny (Obyvacka) a prida ich
/// do Build Settings. Spusti cez menu: Break Room -> Generate New Maps.
/// Re-theme (vzhlad) rieci runtime RoomTheme podla mena sceny.
public class MapGenerator
{
    [MenuItem("Break Room/Generate New Maps")]
    static void Generate()
    {
        MakeMap("Garage");
        MakeMap("Kitchen");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MapGenerator] Hotovo: Garage + Kitchen vygenerovane a pridane do Build Settings.");
    }

    static void MakeMap(string name)
    {
        const string src = "Assets/Scenes/Obyvacka.unity";
        string dst = "Assets/Scenes/" + name + ".unity";

        if (!System.IO.File.Exists(dst))
        {
            if (!AssetDatabase.CopyAsset(src, dst))
            {
                Debug.LogError("[MapGenerator] Kopirovanie zlyhalo: " + dst);
                return;
            }
        }

        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == dst))
        {
            list.Add(new EditorBuildSettingsScene(dst, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
