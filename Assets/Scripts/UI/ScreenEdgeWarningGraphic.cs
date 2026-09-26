using UnityEngine;
using UnityEngine.UI;

/// <summary>中央が透明な、段階的な濃淡の四辺枠を描画する。</summary>
[AddComponentMenu("UI/Screen Edge Warning Graphic")]
[RequireComponent(typeof(CanvasRenderer))]
public class ScreenEdgeWarningGraphic : MaskableGraphic
{
    [Header("枠全体の幅（Canvas上の単位）")]
    [Min(0f)]
    [SerializeField] private float borderWidth = 90f;

    [Header("濃淡の段階数")]
    [Range(1, 16)]
    [SerializeField] private int bandCount = 5;

    [Header("外側の帯の不透明度")]
    [Range(0f, 1f)]
    [SerializeField] private float outerOpacity = 0.95f;

    [Header("内側の帯の不透明度")]
    [Range(0f, 1f)]
    [SerializeField] private float innerOpacity = 0.07f;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        color = new Color(1f, 35f / 255f, 52f / 255f, 1f);
        raycastTarget = false;
    }
#endif

    /// <summary>隣り合う帯や四隅を重ねず、四辺の矩形を構築する。</summary>
    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();
        Rect bounds = GetPixelAdjustedRect();
        float width = Mathf.Clamp(borderWidth, 0f,
            Mathf.Min(bounds.width, bounds.height) * 0.49f);
        if (width <= 0f) return;

        int count = Mathf.Clamp(bandCount, 1, 16);
        float step = width / count;
        for (int i = 0; i < count; i++)
        {
            float inset = i * step;
            float left = bounds.xMin + inset;
            float right = bounds.xMax - inset;
            float bottom = bounds.yMin + inset;
            float top = bounds.yMax - inset;
            float progress = count > 1 ? (float)i / (count - 1) : 0f;
            Color tint = color;
            tint.a *= Mathf.Lerp(innerOpacity, outerOpacity,
                Mathf.Pow(1f - progress, 1.7f));

            AddRectangle(vertices, left, top - step, right, top, tint);
            AddRectangle(vertices, left, bottom, right, bottom + step, tint);
            AddRectangle(vertices, left, bottom + step,
                left + step, top - step, tint);
            AddRectangle(vertices, right - step, bottom + step,
                right, top - step, tint);
        }
    }

    /// <summary>単色の矩形をメッシュへ追加する。</summary>
    private static void AddRectangle(VertexHelper vertices,
        float left, float bottom, float right, float top, Color tint)
    {
        int first = vertices.currentVertCount;
        vertices.AddVert(new Vector3(left, bottom), tint, Vector2.zero);
        vertices.AddVert(new Vector3(left, top), tint, Vector2.zero);
        vertices.AddVert(new Vector3(right, top), tint, Vector2.zero);
        vertices.AddVert(new Vector3(right, bottom), tint, Vector2.zero);
        vertices.AddTriangle(first, first + 1, first + 2);
        vertices.AddTriangle(first + 2, first + 3, first);
    }
}
