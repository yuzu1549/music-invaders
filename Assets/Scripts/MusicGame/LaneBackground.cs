using UnityEngine;

public class LaneBackground : MonoBehaviour
{
    [Header("ノーツの軌道設定（Pseudo3DNoteと同じ数値を入力）")]
    public float startY = -5f;
    public float endY = 0f;
    public float startSpread = 1f;
    public float endSpread = 0.2f;
    public float startScaleX = 2.0f; 
    public float endScaleX = 0.5f;   

    [Header("見た目の設定")]
    public float startLineWidth = 0.05f;
    public float endLineWidth = 0.01f;

    [Header("色の設定（フェードアウトさせず一定の濃さに！）")]
    public Color lineColor = new Color(1f, 1f, 1f, 0.4f); 

    [Header("描画順（背景より手前、ノーツより奥）")]
    public int sortingOrder = -1;

    void Start()
    {
        CreateLine("LeftLine", -1);
        CreateLine("CenterLine", 0);
        CreateLine("RightLine", 1);
    }

    void CreateLine(string lineName, int positionType)
    {
        GameObject lineObj = new GameObject(lineName);
        lineObj.transform.SetParent(this.transform);
        lineObj.transform.localPosition = Vector3.zero;
        lineObj.transform.localScale = Vector3.one;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = lineColor; 
        lr.startWidth = startLineWidth;
        lr.endWidth = endLineWidth;
        lr.positionCount = 2; // 点は2つに戻す
        lr.useWorldSpace = false;
        lr.sortingOrder = sortingOrder;

        float startX = 0;
        float endX = 0;

        if (positionType == -1) 
        {
            startX = -startSpread - (startScaleX / 2f);
            endX = -endSpread - (endScaleX / 2f);
        }
        else if (positionType == 1) 
        {
            startX = startSpread + (startScaleX / 2f);
            endX = endSpread + (endScaleX / 2f);
        }
        else if (positionType == 0)
        {
            // ★超重要ポイント★
            // Unityの丸め込み計算を回避するため、始点と終点を「X軸をまたぐように」ズラす
            // 画面上では約1〜2ピクセルのズレなので、視覚的には完全に真っ直ぐに見えます
            startX = 0.01f;  
            endX = -0.01f; 
        }

        lr.SetPosition(0, new Vector3(startX, startY, -0.1f));
        lr.SetPosition(1, new Vector3(endX, endY, -0.1f));
    }
}
