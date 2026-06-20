using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// Jednotny vizual vsetkych scen za behu (bez rucnej upravy scen).
/// Cielom je TEPLY, FLAT low-poly look v style One Armed Robber – aby hub,
/// factory a spol. vyzerali rovnako sucasne ako Living Room (Obyvacka).
///
/// DOLEZITE: tento skript NESMIE pridavat lesk/reflexie (to by zabilo flat
/// look a "rozbilo" Flatten Materials). Naopak drzi materialy ploche aj za behu.
public class VisualUpgrade : MonoBehaviour
{
    // MainMenu ma vlastny look – nechaj ho tak. (Pridaj sem dalsie ak treba.)
    static readonly string[] SKIP = { "MainMenu" };

    // ---- jednotna nalada (teplo, mierne stmaveny ambient) ----------------
    static readonly Color KeyColor     = new Color(1.00f, 0.92f, 0.77f); // teply directional (~#FFEAC4)
    static readonly Color AmbSky       = new Color(0.60f, 0.56f, 0.49f); // teply kremovy zhora
    static readonly Color AmbEquator   = new Color(0.45f, 0.41f, 0.36f); // teply stred
    static readonly Color AmbGround    = new Color(0.22f, 0.18f, 0.15f); // teply hnedy zdola

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

        // Tiene a kvalita – mäkke tiene su pre tento styl OK
        QualitySettings.shadowDistance   = 45f;
        QualitySettings.shadows          = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowCascades   = 4;

        // Teply ambient (Trilight) – jadro jednotnej nalady
        RenderSettings.ambientMode         = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = AmbSky;
        RenderSettings.ambientEquatorColor = AmbEquator;
        RenderSettings.ambientGroundColor  = AmbGround;

        // Flat look: ziadne odrazy prostredia
        RenderSettings.reflectionIntensity = 0.12f;

        StartCoroutine(Deferred());
    }

    IEnumerator Deferred()
    {
        yield return null;   // pockaj na objekty v scene

        // 1) SVETLO: zabezpec jeden teply directional "key" s mäkkym tienom.
        Light key = null;
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l == null || l.type != LightType.Directional) continue;
            if (key == null) key = l;                      // prvy directional = key
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.6f;
            l.shadowBias = 0.04f;
        }

        if (key == null)
        {
            // ziadny directional v scene -> vytvor ho (toto zachrani "sive" sceny)
            var go = new GameObject("KeyLight (auto)");
            key = go.AddComponent<Light>();
            key.type = LightType.Directional;
            key.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
        // jednotny teply key
        key.color = KeyColor;
        key.intensity = Mathf.Max(key.intensity, 1.15f);
        key.shadows = LightShadows.Soft;
        key.shadowStrength = 0.6f;

        // 2) MATERIALY: drz ich ploche aj za behu (instancie, nemenia asset).
        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if (r == null) continue;
            if (r.GetComponentInParent<Canvas>() != null) continue;   // nie UI
            string n = r.gameObject.name.ToLowerInvariant();
            if (n.Contains("hand") || n.Contains("forearm") || n.Contains("weapon")) continue; // nie viewmodel

            var mats = r.materials;   // instancie (nemenia .mat asset)
            foreach (var m in mats)
            {
                if (m == null) continue;
                if (m.IsKeywordEnabled("_EMISSION")) continue;   // svietiace veci necha tak (lampy/obrazovky)

                if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0f);
                if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0f); // Standard
                if (m.HasProperty("_Metallic"))   m.SetFloat("_Metallic", 0f);

                if (m.HasProperty("_SpecularHighlights"))    m.SetFloat("_SpecularHighlights", 0f);
                if (m.HasProperty("_EnvironmentReflections")) m.SetFloat("_EnvironmentReflections", 0f);
                m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                m.EnableKeyword("_GLOSSYREFLECTIONS_OFF");      // Standard
            }
        }
    }
}
