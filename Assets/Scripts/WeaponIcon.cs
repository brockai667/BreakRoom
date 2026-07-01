using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// Vyrenderuje 3D model zbrane (WeaponPreview.BuildModel) do Sprite, ktorý sa dá
/// použiť ako pekná ikona na kartách v shope (namiesto tyče + kvádra).
/// Renderuje sa cez izolovanú kameru ďaleko od scény, výsledok sa cachuje.
/// Pri zlyhaní vráti null — volajúci si vykreslí 2D fallback.
public static class WeaponIcon
{
    const int SIZE = 256;
    static readonly Vector3 FAR = new Vector3(0f, 6000f, 0f);
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    static Camera cam;
    static Light keyLight, fillLight;

    /// Vráti (a cachuje) ikonu danej zbrane; null ak render zlyhá alebo w je null.
    public static Sprite Get(WeaponData w)
    {
        if (w == null) return null;
        if (cache.TryGetValue(w.id, out var cached)) return cached;

        Sprite sprite = null;
        GameObject model = null;
        RenderTexture rt = null;
        var prevActive = RenderTexture.active;
        try
        {
            EnsureCam();

            model = WeaponPreview.BuildModel(w);
            if (model == null) { cache[w.id] = null; return null; }
            // vypni prípadnú animáciu/rotáciu, nech je statický záber
            foreach (var mb in model.GetComponentsInChildren<MonoBehaviour>()) if (mb != null) mb.enabled = false;
            model.transform.position = FAR;
            model.transform.rotation = Quaternion.Euler(0f, 28f, 0f);

            // bounds zo všetkých rendererov
            var rends = model.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) { Object.Destroy(model); cache[w.id] = null; return null; }
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);

            // umiestni kameru pred model (mierne zhora), ortho rámovanie
            Vector3 dir = new Vector3(0f, 0.28f, -1f).normalized;
            cam.transform.position = b.center + dir * 12f;
            cam.transform.rotation = Quaternion.LookRotation(b.center - cam.transform.position, Vector3.up);
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(0.25f, Mathf.Max(b.extents.x, b.extents.y) * 1.18f);
            cam.nearClipPlane = 0.05f; cam.farClipPlane = 40f;

            rt = RenderTexture.GetTemporary(SIZE, SIZE, 16, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            // svetlá zapni len počas renderu ikony (sú smerové = globálne)
            if (keyLight != null) keyLight.enabled = true;
            if (fillLight != null) fillLight.enabled = true;

            bool rendered = false;
            var req = new UniversalRenderPipeline.SingleCameraRequest();
            if (RenderPipeline.SupportsRenderRequest(cam, req))
            {
                req.destination = rt;
                RenderPipeline.SubmitRenderRequest(cam, req);
                rendered = true;
            }
            if (!rendered) cam.Render();   // fallback (built-in / staršie URP)

            RenderTexture.active = rt;
            var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, SIZE, SIZE), 0, 0);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;

            sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), 100f);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[WeaponIcon] render zlyhal pre " + w.id + ": " + e.Message);
            sprite = null;
        }
        finally
        {
            if (keyLight != null) keyLight.enabled = false;
            if (fillLight != null) fillLight.enabled = false;
            if (cam != null) cam.targetTexture = null;
            RenderTexture.active = prevActive;
            if (rt != null) RenderTexture.ReleaseTemporary(rt);
            if (model != null) Object.Destroy(model);
        }

        cache[w.id] = sprite;
        return sprite;
    }

    static void EnsureCam()
    {
        if (cam != null) return;
        var go = new GameObject("WeaponIconCam");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.transform.position = FAR + new Vector3(0, 0, -12f);
        cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0f, 0f, 0f, 0f);   // priehľadné pozadie
        cam.cullingMask = ~0;
        cam.enabled = false;                                // renderujeme manuálne
        cam.allowMSAA = false; cam.allowHDR = false;

        // vlastné svetlo, nech model nie je tmavý (kľúč + jemný fill).
        // Sú vypnuté a zapínajú sa len počas renderu ikony (smerové = globálne).
        var lightGO = new GameObject("WeaponIconLight");
        lightGO.transform.SetParent(go.transform, false);
        lightGO.transform.rotation = Quaternion.Euler(32f, 18f, 0f);
        keyLight = lightGO.AddComponent<Light>();
        keyLight.type = LightType.Directional; keyLight.intensity = 1.7f; keyLight.color = new Color(1f, 0.97f, 0.92f);
        keyLight.enabled = false;

        var fillGO = new GameObject("WeaponIconFill");
        fillGO.transform.SetParent(go.transform, false);
        fillGO.transform.rotation = Quaternion.Euler(-15f, -150f, 0f);
        fillLight = fillGO.AddComponent<Light>();
        fillLight.type = LightType.Directional; fillLight.intensity = 0.6f; fillLight.color = new Color(0.8f, 0.85f, 1f);
        fillLight.enabled = false;
    }
}
