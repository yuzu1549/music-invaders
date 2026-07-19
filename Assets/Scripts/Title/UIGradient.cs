using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIGradient : BaseMeshEffect
{
    [Header("上側の色")]
    [SerializeField]
    private Color topColor = new Color(0.01f, 0.02f, 0.12f, 1f);

    [Header("下側の色")]
    [SerializeField]
    private Color bottomColor = new Color(0.08f, 0.10f, 0.35f, 1f);

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive())
        {
            return;
        }

        int vertexCount = vertexHelper.currentVertCount;

        if (vertexCount == 0)
        {
            return;
        }

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        UIVertex vertex = new UIVertex();

        for (int i = 0; i < vertexCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);

            minY = Mathf.Min(minY, vertex.position.y);
            maxY = Mathf.Max(maxY, vertex.position.y);
        }

        float height = maxY - minY;

        if (Mathf.Approximately(height, 0f))
        {
            return;
        }

        for (int i = 0; i < vertexCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);

            float rate = Mathf.InverseLerp(minY, maxY, vertex.position.y);
            vertex.color = Color.Lerp(bottomColor, topColor, rate);

            vertexHelper.SetUIVertex(vertex, i);
        }
    }
}