using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TriangleGraphic : Graphic
{
    public enum TriangleDirection
    {
        Up,
        Down
    }

    [SerializeField]
    private TriangleDirection direction = TriangleDirection.Up;

    public TriangleDirection Direction
    {
        get => direction;
        set
        {
            direction = value;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = rectTransform.rect;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        if (direction == TriangleDirection.Up)
        {
            AddVertex(
                vertexHelper,
                vertex,
                new Vector2(rect.center.x, rect.yMax)
            );

            AddVertex(
                vertexHelper,
                vertex,
                new Vector2(rect.xMin, rect.yMin)
            );

            AddVertex(
                vertexHelper,
                vertex,
                new Vector2(rect.xMax, rect.yMin)
            );
        }
        else
        {
            AddVertex(
                vertexHelper,
                vertex,
                new Vector2(rect.xMin, rect.yMax)
            );

            AddVertex(
                vertexHelper,
                vertex,
                new Vector2(rect.xMax, rect.yMax)
            );

            AddVertex(
                vertexHelper,
                vertex,
                new Vector2(rect.center.x, rect.yMin)
            );
        }

        vertexHelper.AddTriangle(0, 1, 2);
    }

    public override bool Raycast(
        Vector2 screenPoint,
        Camera eventCamera
    )
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPoint,
                eventCamera,
                out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = rectTransform.rect;

        Vector2 pointA;
        Vector2 pointB;
        Vector2 pointC;

        if (direction == TriangleDirection.Up)
        {
            pointA = new Vector2(rect.center.x, rect.yMax);
            pointB = new Vector2(rect.xMin, rect.yMin);
            pointC = new Vector2(rect.xMax, rect.yMin);
        }
        else
        {
            pointA = new Vector2(rect.xMin, rect.yMax);
            pointB = new Vector2(rect.xMax, rect.yMax);
            pointC = new Vector2(rect.center.x, rect.yMin);
        }

        return IsPointInsideTriangle(
            localPoint,
            pointA,
            pointB,
            pointC
        );
    }

    private static void AddVertex(
        VertexHelper vertexHelper,
        UIVertex vertex,
        Vector2 position
    )
    {
        vertex.position = position;
        vertexHelper.AddVert(vertex);
    }

    private static bool IsPointInsideTriangle(
        Vector2 point,
        Vector2 pointA,
        Vector2 pointB,
        Vector2 pointC
    )
    {
        float sign1 = Sign(point, pointA, pointB);
        float sign2 = Sign(point, pointB, pointC);
        float sign3 = Sign(point, pointC, pointA);

        bool hasNegative =
            sign1 < 0f ||
            sign2 < 0f ||
            sign3 < 0f;

        bool hasPositive =
            sign1 > 0f ||
            sign2 > 0f ||
            sign3 > 0f;

        return !(hasNegative && hasPositive);
    }

    private static float Sign(
        Vector2 point1,
        Vector2 point2,
        Vector2 point3
    )
    {
        return
            (point1.x - point3.x) *
            (point2.y - point3.y) -
            (point2.x - point3.x) *
            (point1.y - point3.y);
    }
}