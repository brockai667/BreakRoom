using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Re-theme novych map (kopie Obyvacky) podla mena sceny: prefarbi velke
/// plochy (podlaha/steny/strop), aby Garage/Kitchen mali iny mood.
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

    void Apply(string scene)
    {
        Color wall, floor, ceil;
        if (scene == "Garage")
        {
            wall  = new Color(0.32f, 0.34f, 0.38f);
            floor = new Color(0.27f, 0.27f, 0.29f);
            ceil  = new Color(0.22f, 0.23f, 0.25f);
        }
        else if (scene == "Kitchen")
        {
            wall  = new Color(0.86f, 0.86f, 0.82f);
            floor = new Color(0.78f, 0.80f, 0.82f);
            ceil  = new Color(0.93f, 0.93f, 0.90f);
        }
        else return;

        StartCoroutine(Tint(wall, floor, ceil));
    }

    IEnumerator Tint(Color wall, Color floor, Color ceil)
    {
        yield return null;   // pockaj na objekty v scene
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
    }
}
