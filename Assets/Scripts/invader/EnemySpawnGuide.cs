using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(EnemyGridPlacement))]
[RequireComponent(typeof(EnemyGroupMovementSettings))]
public class EnemySpawnGuide : MonoBehaviour
{
    [Header("Sceneビューに敵配置ガイドを表示する")]
    [SerializeField] private bool showSpawnGuide = true;
    [Header("グリッドガイドの色")]
    [SerializeField] private Color gridGuideColor =
        new Color(0.2f, 0.8f, 1f, 0.4f);
    [Header("進入フェーズ開始ガイドの色")]
    [SerializeField] private Color entryStartGuideColor =
        new Color(0.2f, 0.8f, 1f, 1f);
    [Header("通常フェーズ開始ガイドの色")]
    [SerializeField] private Color normalStartGuideColor = Color.green;
    [Header("突入フェーズ開始ガイドの色")]
    [SerializeField] private Color diveStartGuideColor =
        new Color(1f, 0.5f, 0f, 1f);
    [Header("突入フェーズ終了ガイドの色")]
    [SerializeField] private Color diveEndGuideColor = Color.red;
    [Header("基準位置マーカーの半径")]
    [Min(0.01f)]
    [SerializeField] private float guideMarkerRadius = 0.08f;

    private EnemyGridPlacement gridPlacement;
    private EnemyGroupMovementSettings movementSettings;

    private void OnDrawGizmos()
    {
        CacheComponents();
        if (!showSpawnGuide || gridPlacement == null ||
            movementSettings == null)
        {
            return;
        }

        DrawGuideMarker(
            gridPlacement.GridLeftPoint,
            Color.yellow,
            "GridLeftPoint"
        );
        DrawGuideMarker(
            gridPlacement.GridRightPoint,
            Color.yellow,
            "GridRightPoint"
        );
        DrawGuideMarker(
            movementSettings.EntryStartPoint,
            entryStartGuideColor,
            "EntryStartPoint"
        );
        DrawGuideMarker(
            movementSettings.NormalStartPoint,
            normalStartGuideColor,
            "NormalStartPoint"
        );
        DrawGuideMarker(
            movementSettings.DiveStartPoint,
            diveStartGuideColor,
            "DiveStartPoint"
        );
        DrawGuideMarker(
            movementSettings.DiveEndPoint,
            diveEndGuideColor,
            "DiveEndPoint"
        );

        if (gridPlacement.GridLeftPoint == null ||
            gridPlacement.GridRightPoint == null)
        {
            return;
        }

        DrawHorizontalGuide(
            movementSettings.EntryStartPoint,
            entryStartGuideColor
        );
        DrawHorizontalGuide(
            movementSettings.NormalStartPoint,
            normalStartGuideColor
        );
        DrawHorizontalGuide(
            movementSettings.DiveStartPoint,
            diveStartGuideColor
        );
        DrawHorizontalGuide(
            movementSettings.DiveEndPoint,
            diveEndGuideColor
        );
        DrawGridColumns();
    }

    /// <summary>
    /// 同じGameObjectにあるガイド描画用コンポーネントを取得する。
    /// </summary>
    private void CacheComponents()
    {
        if (gridPlacement == null)
        {
            gridPlacement = GetComponent<EnemyGridPlacement>();
        }

        if (movementSettings == null)
        {
            movementSettings = GetComponent<EnemyGroupMovementSettings>();
        }
    }

    /// <summary>
    /// Sceneビューに基準位置のマーカーと名前を描画する。
    /// </summary>
    /// <param name="point">表示する基準位置</param>
    /// <param name="color">マーカーの色</param>
    /// <param name="label">基準位置の表示名</param>
    private void DrawGuideMarker(Transform point, Color color, string label)
    {
        if (point == null)
        {
            return;
        }

        Gizmos.color = color;
        Gizmos.DrawWireSphere(point.position, guideMarkerRadius);

#if UNITY_EDITOR
        Handles.color = color;
        Handles.Label(
            point.position + Vector3.up * guideMarkerRadius * 1.5f,
            label
        );
#endif
    }

    /// <summary>
    /// Sceneビューにグリッド左端から右端までの水平線を描画する。
    /// </summary>
    /// <param name="point">水平線のY座標に使う基準位置</param>
    /// <param name="color">水平線の色</param>
    private void DrawHorizontalGuide(Transform point, Color color)
    {
        if (point == null)
        {
            return;
        }

        float guideZ = GetGuideZ();
        Vector3 leftPosition = new Vector3(
            gridPlacement.GridLeftPoint.position.x,
            point.position.y,
            guideZ
        );
        Vector3 rightPosition = new Vector3(
            gridPlacement.GridRightPoint.position.x,
            point.position.y,
            guideZ
        );

        Gizmos.color = color;
        Gizmos.DrawLine(leftPosition, rightPosition);
    }

    /// <summary>
    /// Sceneビューに通常フェーズ範囲のグリッド列を描画する。
    /// </summary>
    private void DrawGridColumns()
    {
        if (movementSettings.NormalStartPoint == null ||
            movementSettings.DiveStartPoint == null ||
            gridPlacement.GridColumnCount < 2)
        {
            return;
        }

        float guideZ = GetGuideZ();
        Gizmos.color = gridGuideColor;

        for (int column = 0;
            column < gridPlacement.GridColumnCount;
            column++)
        {
            float columnX = gridPlacement.GetColumnX(column);
            Vector3 startPosition = new Vector3(
                columnX,
                movementSettings.NormalStartPoint.position.y,
                guideZ
            );
            Vector3 endPosition = new Vector3(
                columnX,
                movementSettings.DiveStartPoint.position.y,
                guideZ
            );

            Gizmos.DrawLine(startPosition, endPosition);
        }
    }

    /// <summary>
    /// ガイドを描画するワールドZ座標を返す。
    /// </summary>
    /// <returns>左右のグリッド基準位置の中間Z座標</returns>
    private float GetGuideZ()
    {
        return (gridPlacement.GridLeftPoint.position.z +
            gridPlacement.GridRightPoint.position.z) * 0.5f;
    }
}
