using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

/// Pridá profi osvetlenie a post-processing (URP) do scén:
/// mäkké tiene, bloom, tonemapping (ACES), color grading, vignette, teplé ambient.
public class ProLighting
{
    static readonly string[] SCENES =
    {
        "Assets/Scenes/Obyvacka.unity",
        "Assets/Scenes/Office.unity",
        "Assets/Scenes/Hub.unity",
        "Assets/Scenes/MainMenu.unity",
    };

    [MenuItem("BreakRoom/Apply Pro Lighting")]
    public static void ApplyAll()
    {
        var profile = GetOrCreateProfile();
        foreach (var path in SCENES)
        {
            var scene = EditorSceneManager.OpenScene(path);
            ApplyToScene(profile, path.Contains("MainMenu"));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("✅ Pro lighting + post-processing aplikované na všetky scény.");
    }

    static void ApplyToScene(VolumeProfile profile, bool isMenu)
    {
        // Global Volume s post-processing profilom
        var volGO = GameObject.Find("PostFX Volume");
        if (volGO == null) volGO = new GameObject("PostFX Volume");
        var vol = volGO.GetComponent<Volume>(); if (vol == null) vol = volGO.AddComponent<Volume>();
        vol.isGlobal = true; vol.priority = 1; vol.sharedProfile = profile;

        // Smerové svetlo: mäkké tiene + teplý tón
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional)
            {
                l.shadows = LightShadows.Soft;
                l.shadowStrength = 0.75f;
                l.intensity = Mathf.Clamp(l.intensity, 1.0f, 1.3f);
                l.color = new Color(1f, 0.96f, 0.88f);
                if (!isMenu) l.transform.rotation = Quaternion.Euler(48f, 35f, 0f);
            }
        }

        // Kamery: zapni post-processing + FXAA
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            }
        }

        // Teplé ambientné svetlo (gradient) - aby tiene a kontrast vynikli
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.50f, 0.50f, 0.55f);
        RenderSettings.ambientEquatorColor = new Color(0.40f, 0.38f, 0.36f);
        RenderSettings.ambientGroundColor  = new Color(0.18f, 0.16f, 0.15f);
    }

    static VolumeProfile GetOrCreateProfile()
    {
        const string path = "Assets/Settings/BreakRoomPostFX.asset";
        var prof = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if (prof != null) return prof;

        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            AssetDatabase.CreateFolder("Assets", "Settings");
        prof = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(prof, path);

        var tm = prof.Add<Tonemapping>(true);
        tm.mode.overrideState = true; tm.mode.value = TonemappingMode.ACES;

        var bloom = prof.Add<Bloom>(true);
        bloom.intensity.overrideState = true; bloom.intensity.value = 0.85f;
        bloom.threshold.overrideState = true; bloom.threshold.value = 0.95f;
        bloom.scatter.overrideState  = true; bloom.scatter.value  = 0.65f;
        bloom.tint.overrideState     = true; bloom.tint.value     = new Color(1f, 0.96f, 0.88f);

        var ca = prof.Add<ColorAdjustments>(true);
        ca.postExposure.overrideState = true; ca.postExposure.value = 0.15f;
        ca.contrast.overrideState     = true; ca.contrast.value     = 14f;
        ca.saturation.overrideState   = true; ca.saturation.value   = 10f;

        var wb = prof.Add<WhiteBalance>(true);
        wb.temperature.overrideState = true; wb.temperature.value = 10f;   // mierne teplé

        var vig = prof.Add<Vignette>(true);
        vig.intensity.overrideState  = true; vig.intensity.value  = 0.30f;
        vig.smoothness.overrideState = true; vig.smoothness.value = 0.45f;

        AssetDatabase.SaveAssets();
        return prof;
    }
}
