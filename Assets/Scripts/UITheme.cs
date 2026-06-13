using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Zdielany moderny UI styl: zaoblene rohy (9-slice sprite generovany za behu),
/// paleta, hover/press efekty na tlacidlach a mäkky tien. Pouzivane vsetkymi
/// UI skriptami, aby hra vyzerala konzistentne a moderne.
public static class UITheme
{
    // Paleta
    public static readonly Color Overlay   = new Color(0.02f, 0.02f, 0.04f, 0.82f);
    public static readonly Color Panel     = new Color(0.12f, 0.13f, 0.17f, 1f);
    public static readonly Color PanelLight = new Color(0.17f, 0.18f, 0.23f, 1f);
    public static readonly Color Accent    = new Color(1f, 0.46f, 0.14f, 1f);
    public static readonly Color AccentDim = new Color(0.55f, 0.24f, 0.07f, 1f);
    public static readonly Color BtnNormal = new Color(0.20f, 0.22f, 0.28f, 1f);
    public static readonly Color BtnHover  = new Color(0.29f, 0.32f, 0.40f, 1f);
    public static readonly Color Good      = new Color(0.22f, 0.62f, 0.30f, 1f);
    public static readonly Color GoodHover = new Color(0.28f, 0.74f, 0.38f, 1f);
    public static readonly Color Danger    = new Color(0.78f, 0.26f, 0.18f, 1f);
    public static readonly Color DangerHover= new Color(0.90f, 0.33f, 0.24f, 1f);
    public static readonly Color Text       = Color.white;
    public static readonly Color SubText    = new Color(0.72f, 0.76f, 0.84f, 1f);

    static readonly Dictionary<int, Sprite> cache = new Dictionary<int, Sprite>();

    /// Biely zaobleny 9-slice sprite (tintuje sa farbou Image-u).
    public static Sprite Rounded(int radius = 16)
    {
        if (cache.TryGetValue(radius, out var sp)) return sp;
        int r = Mathf.Max(2, radius);
        int size = r * 2 + 4;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                px[y * size + x] = new Color(1f, 1f, 1f, CornerAlpha(x, y, size, r));
        tex.SetPixels(px); tex.Apply();
        var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                              100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        cache[radius] = s;
        return s;
    }

    static float CornerAlpha(int x, int y, int size, int r)
    {
        bool cx = x < r || x > size - 1 - r;
        bool cy = y < r || y > size - 1 - r;
        if (cx && cy)
        {
            float ccx = x < r ? r : size - 1 - r;
            float ccy = y < r ? r : size - 1 - r;
            float d = Mathf.Sqrt((x - ccx) * (x - ccx) + (y - ccy) * (y - ccy));
            return Mathf.Clamp01(r - d + 0.5f);
        }
        return 1f;
    }

    /// Spravi z Image zaobleny panel danej farby.
    public static Image PanelImage(GameObject go, Color col, int radius = 18)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.sprite = Rounded(radius); img.type = Image.Type.Sliced; img.color = col;
        return img;
    }

    /// Hover/press farby na tlacidlo (plynuly prechod).
    public static void Hover(Button btn, Color normal, Color hover)
    {
        btn.transition = Selectable.Transition.ColorTint;
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(hover.r / Mathf.Max(0.01f, normal.r), 1f, 1f, 1f); // fallback
        cb.fadeDuration = 0.12f;
        // Pouzijeme jednoduchu cestu: menime priamo farbu cez multiplikator
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        cb.selectedColor = Color.white;
        cb.colorMultiplier = 1f;
        btn.colors = cb;
    }

    /// Mäkky tien pod prvkom (duplikat obrazka posunuty a stmaveny).
    public static void Shadow(GameObject go, Vector2 dist, float alpha = 0.45f)
    {
        var sh = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, alpha);
        sh.effectDistance = dist;
    }
}
