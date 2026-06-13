using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// Vizualny upgrade scen za behu (bez rucnej upravy scen): mäkke tiene,
/// lepsi ambient, lesklejsie materialy (povrchy reaguju na svetlo).
/// Najvacsi skok vzhladu v ramci existujuceho low-poly artu.
public class VisualUpgrade : MonoBehaviour
{
    static readonly string[] SKIP = { "MainMenu" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("VisualUpgrade");
        DontDestroyOnLoad(go);
        var vu = go.AddComponent<VisualUpgrade>();
        SceneManager.sceneLoaded += (s, m) => vu.Apply(s.name);
        vu.Apply(SceneManager.GetActiveScene().name);
    }

    void Apply(string scene)
    {
        if (System.Array.IndexOf(SKIP, scene) >= 0) return;

        // Tiene a kvalita
        QualitySettings.shadowDistance   = 45f;
        QualitySettings.shadows          = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowCascades   = 4;

        // Bohatsi ambient (mäksie, prirodzenejsie nasvietenie)
        RenderSettings.ambientMode        = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.58f, 0.60f, 0.68f);
        RenderSettings.ambientEquatorColor = new Color(0.40f, 0.39f, 0.40f);
        RenderSettings.ambientGroundColor  = new Color(0.16f, 0.15f, 0.14f);
        RenderSettings.reflectionIntensity = 0.8f;

        StartCoroutine(Deferred());
    }

    IEnumerator Deferred()
    {
        yield return null;   // pockaj na objekty v scene

        // Svetla: mäkke tiene, mierne teplé, slusna intenzita
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l == null) continue;
            if (l.type == LightType.Directional)
            {
                l.shadows = LightShadows.Soft;
                l.shadowStrength = 0.72f;
                l.shadowBias = 0.04f;
                if (l.intensity < 0.85f) l.intensity = 1.1f;
            }
        }

        // Materialy: pridaj smoothness + specular, aby povrchy odrazali svetlo
        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if (r == null) continue;
            if (r.GetComponentInParent<Canvas>() != null) continue;   // nie UI
            string n = r.gameObject.name.ToLowerInvariant();
            if (n.Contains("hand") || n.Contains("forearm") || n.Contains("weapon")) continue; // nie viewmodel

            var mats = r.materials;   // instancie (nemenia asset)
            foreach (var m in mats)
            {
                if (m == null) continue;
                bool emissive = m.IsKeywordEnabled("_EMISSION");
                if (m.HasProperty("_Smoothness"))            m.SetFloat("_Smoothness", emissive ? 0.5f : 0.32f);
                if (m.HasProperty("_Metallic"))              m.SetFloat("_Metallic", 0f);
                if (m.HasProperty("_SpecularHighlights"))    m.SetFloat("_SpecularHighlights", 1f);
                if (m.HasProperty("_EnvironmentReflections")) m.SetFloat("_EnvironmentReflections", 1f);
            }
        }
    }
}
