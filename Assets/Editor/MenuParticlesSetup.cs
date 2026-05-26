// Assets/Editor/MenuParticlesSetup.cs

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class MenuParticlesSetup : Editor
{
    // ── 1. NAJPRV SPUSTI TOTO ────────────────────────────────────────
    [MenuItem("Break Room/1. Clean Up (spusti prvy)")]
    static void CleanUp()
    {
        // Zmaz vsetky DustParticles
        foreach (var go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (go.name == "DustParticles") DestroyImmediate(go);

        // Zmaz vsetky BackgroundCanvas — Background presunieme spat
        foreach (var go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name == "BackgroundCanvas")
            {
                Transform bg = go.transform.Find("Background");
                if (bg != null) bg.SetParent(null); // docasne do root
                DestroyImmediate(go);
            }
        }

        // Najdi Canvas s buttonmi (PlayButton)
        Canvas realCanvas = null;
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.transform.Find("PlayButton") != null ||
                c.GetComponentInChildren<Button>() != null)
            {
                realCanvas = c;
                break;
            }
        }

        // Zmaz vsetky ostatne UICanvas / Canvas duplikaty
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c != realCanvas)
                DestroyImmediate(c.gameObject);
        }

        // Premenuj realCanvas na "Canvas"
        if (realCanvas != null)
        {
            realCanvas.gameObject.name = "Canvas";

            // Ak Background lezi v roote, presun ho do Canvas
            GameObject bgInRoot = GameObject.Find("Background");
            if (bgInRoot != null && bgInRoot.transform.parent == null)
            {
                bgInRoot.transform.SetParent(realCanvas.transform, false);
                bgInRoot.transform.SetAsFirstSibling();
                RectTransform rt = bgInRoot.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
            }
            Debug.Log("✅ Cleanup hotovy! Hierarchy je cista. Teraz spusti: Break Room/2. Setup Menu Particles");
        }
        else
        {
            Debug.LogError("Nenasiel som Canvas s buttonmi!");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // ── 2. PO CLEANUP SPUSTI TOTO ────────────────────────────────────
    [MenuItem("Break Room/2. Setup Menu Particles")]
    static void SetupMenuParticles()
    {
        // Over ze existuje Canvas
        GameObject canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null)
        {
            Debug.LogError("Najprv spusti: Break Room/1. Clean Up");
            return;
        }

        Canvas mainCanvas = canvasGO.GetComponent<Canvas>();

        // Najdi Background
        GameObject backgroundGO = null;
        foreach (Transform child in canvasGO.transform)
        {
            if (child.name == "Background")
            {
                backgroundGO = child.gameObject;
                break;
            }
        }
        if (backgroundGO == null)
        {
            Image[] imgs = canvasGO.GetComponentsInChildren<Image>();
            if (imgs.Length > 0) backgroundGO = imgs[0].gameObject;
        }

        // BackgroundCanvas (Sort Order 0)
        GameObject bgCanvasGO = new GameObject("BackgroundCanvas");
        Canvas bgCanvas = bgCanvasGO.AddComponent<Canvas>();
        bgCanvas.renderMode   = RenderMode.ScreenSpaceCamera;
        bgCanvas.worldCamera  = Camera.main;
        bgCanvas.sortingOrder = 0;
        var bgScaler = bgCanvasGO.AddComponent<CanvasScaler>();
        bgScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        bgScaler.referenceResolution = new Vector2(1920, 1080);
        bgCanvasGO.AddComponent<GraphicRaycaster>();

        if (backgroundGO != null)
        {
            backgroundGO.transform.SetParent(bgCanvasGO.transform, false);
            RectTransform rt = backgroundGO.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        // UICanvas (Sort Order 2)
        mainCanvas.renderMode   = RenderMode.ScreenSpaceCamera;
        mainCanvas.worldCamera  = Camera.main;
        mainCanvas.sortingOrder = 2;
        canvasGO.name           = "UICanvas";
        var uiScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (uiScaler == null) uiScaler = canvasGO.AddComponent<CanvasScaler>();
        uiScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        uiScaler.referenceResolution = new Vector2(1920, 1080);

        // DustParticles (Sort Order 1)
        GameObject dustGO = new GameObject("DustParticles");
        dustGO.transform.position = Vector3.zero;
        ParticleSystem ps = dustGO.AddComponent<ParticleSystem>();

        var mn = ps.main;
        mn.loop            = true;
        mn.simulationSpace = ParticleSystemSimulationSpace.World;
        mn.startLifetime   = new ParticleSystem.MinMaxCurve(7f, 13f);
        mn.startSpeed      = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        mn.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
        mn.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.75f, 0.2f, 1f),
            new Color(0.95f, 0.9f, 1.0f, 1f));
        mn.maxParticles    = 120;
        mn.gravityModifier = -0.01f;

        var emission = ps.emission;
        emission.rateOverTime = 15f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale     = new Vector3(22f, 0.5f, 0.1f);
        shape.position  = new Vector3(0f, -5.5f, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
        vel.y       = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        vel.z       = new ParticleSystem.MinMaxCurve(0f, 0f);

        var noise = ps.noise;
        noise.enabled     = true;
        noise.strength    = new ParticleSystem.MinMaxCurve(0.3f);
        noise.frequency   = 0.35f;
        noise.scrollSpeed = 0.05f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.75f, 0.2f), 0.0f),
                new GradientColorKey(Color.white,                 0.35f),
                new GradientColorKey(new Color(0.9f, 0.85f, 1f), 0.7f),
                new GradientColorKey(new Color(1f, 0.75f, 0.2f), 1.0f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.0f,  0.00f),
                new GradientAlphaKey(0.85f, 0.10f),
                new GradientAlphaKey(0.75f, 0.60f),
                new GradientAlphaKey(0.3f,  0.85f),
                new GradientAlphaKey(0.0f,  1.00f),
            });
        col.color = grad;

        var sz = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0.00f, 0.0f),
            new Keyframe(0.10f, 1.0f),
            new Keyframe(0.80f, 0.9f),
            new Keyframe(1.00f, 0.0f)));

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode   = ParticleSystemRenderMode.Billboard;
        rend.sortingOrder = 1;
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                     ?? Shader.Find("Particles/Standard Unlit");
        if (shader != null)
            rend.material = new Material(shader);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Particles setup hotovy! Ctrl+S a Play.");
    }
}
#endif
