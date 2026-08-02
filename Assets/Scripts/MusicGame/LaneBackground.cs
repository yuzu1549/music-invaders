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

	[Header("色の設定")]
	public Color lineColor = new Color(1f, 1f, 1f, 0.4f);
	public Color fillColor = new Color(0.15f, 0.15f, 0.15f, 0.7f);

	[Header("描画順の強制設定")]
	[Tooltip("背景画像などと同じ Sorting Layer 名を入力します（基本は Default）")]
	public string sortingLayerName = "Default";

	[Tooltip("塗りつぶしの描画順（背景画像より大きい数字にする）")]
	public int fillSortingOrder = 0;

	[Tooltip("線の描画順（塗りつぶしより大きい数字にする）")]
	public int lineSortingOrder = 1;

	void Start()
	{
		CreateLaneFill();
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
		lr.positionCount = 2;
		lr.useWorldSpace = false;

		// 🔥追加：レイヤー名とオーダーを明示的に指定
		lr.sortingLayerName = sortingLayerName;
		lr.sortingOrder = lineSortingOrder;

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
			startX = 0.01f;
			endX = -0.01f;
		}

		lr.SetPosition(0, new Vector3(startX, startY, -0.1f));
		lr.SetPosition(1, new Vector3(endX, endY, -0.1f));
	}

	void CreateLaneFill()
	{
		GameObject fillObj = new GameObject("LaneFillArea");
		fillObj.transform.SetParent(this.transform);
		fillObj.transform.localPosition = Vector3.zero;
		fillObj.transform.localScale = Vector3.one;

		MeshRenderer mr = fillObj.AddComponent<MeshRenderer>();
		MeshFilter mf = fillObj.AddComponent<MeshFilter>();

		Material mat = new Material(Shader.Find("Sprites/Default"));
		mat.color = fillColor;
		mr.material = mat;

		// 🔥追加：レイヤー名とオーダーを明示的に指定
		mr.sortingLayerName = sortingLayerName;
		mr.sortingOrder = fillSortingOrder;

		float leftStartX = -startSpread - (startScaleX / 2f);
		float leftEndX = -endSpread - (endScaleX / 2f);
		float rightStartX = startSpread + (startScaleX / 2f);
		float rightEndX = endSpread + (endScaleX / 2f);

		// Z軸を -0.05f にして、念のため背景よりわずかに手前に出す
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(leftStartX, startY, -0.05f),
			new Vector3(leftEndX, endY, -0.05f),
			new Vector3(rightStartX, startY, -0.05f),
			new Vector3(rightEndX, endY, -0.05f)
		};

		// 裏面判定で透明にならないよう、両面分のポリゴンを張る（念のための対策）
		int[] triangles = new int[12]
		{
			0, 1, 2,  // 表面1
            2, 1, 3,  // 表面2
            2, 1, 0,  // 裏面1
            3, 1, 2   // 裏面2
        };

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mf.mesh = mesh;
	}
}
