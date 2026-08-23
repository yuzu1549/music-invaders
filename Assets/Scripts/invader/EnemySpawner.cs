using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemySpawner : MonoBehaviour
{
    private const int EnemiesPerGroup = 3;

    [Header("敵のプールのキー")]
    [SerializeField] private string enemyPoolKey = "Invader";

    [Space(15)]
    [Header("グリッド左端の基準位置")]
    [SerializeField] private Transform gridLeftPoint;
    [Header("グリッド右端の基準位置")]
    [SerializeField] private Transform gridRightPoint;
    [Header("横方向のグリッド列数")]
    [Min(EnemiesPerGroup)]
    [SerializeField] private int gridColumnCount = 24;
    [Header("グループ内の敵同士の列間隔")]
    [Tooltip("1で隣接、2で敵の間に1列の空きを作る")]
    [Min(1)]
    [SerializeField] private int groupEnemyColumnStep = 2;
    [Header("グループ間に確保する最低空き列数")]
    [Min(0)]
    [SerializeField] private int minimumGroupGapColumns = 1;
    [Header("離れた生成位置を優先する強さ（0で均等）")]
    [Range(0f, 8f)]
    [SerializeField] private float spawnDistanceBiasPower = 2f;

    [Space(15)]
    [Header("進入フェーズ開始位置")]
    [SerializeField] private Transform entryStartPoint;
    [Header("通常フェーズ開始位置")]
    [SerializeField] private Transform normalStartPoint;
    [Header("突入フェーズ開始位置")]
    [SerializeField] private Transform diveStartPoint;
    [Header("突入フェーズ終了位置")]
    [SerializeField] private Transform diveEndPoint;
    [Header("進入フェーズの小節数")]
    [Min(1)]
    [SerializeField] private int entryPhaseMeasureCount = 1;
    [Header("通常フェーズの小節数")]
    [Min(1)]
    [SerializeField] private int normalPhaseMeasureCount = 14;
    [Header("突入フェーズでチャージする拍数")]
    [Min(1)]
    [SerializeField] private int diveChargeBeatCount = 3;
    [Header("突入フェーズで等速移動する拍数")]
    [Min(1)]
    [SerializeField] private int diveMovementBeatCount = 1;
    [Header("進入フェーズの減速指数")]
    [Min(1f)]
    [SerializeField] private float entryDecelerationPower = 3f;
    [Header("突入チャージ完了時の横方向倍率")]
    [Min(0.01f)]
    [SerializeField] private float diveChargeScaleX = 1.1f;
    [Header("突入チャージ完了時の縦方向倍率")]
    [Min(0.01f)]
    [SerializeField] private float diveChargeScaleY = 0.8f;

    [Space(15)]
    [Header("敵グループの拍同期に使うクロック")]
    [SerializeField] private MusicBeatClock musicBeatClock;
    [Header("拍先頭で適用する敵の横方向倍率")]
    [Min(0.01f)]
    [SerializeField] private float beatSquashScaleX = 1.05f;
    [Header("拍先頭で適用する敵の縦方向倍率")]
    [Min(0.01f)]
    [SerializeField] private float beatSquashScaleY = 0.8f;
    [Header("元の大きさへ戻り終える拍内進行度")]
    [Range(0.01f, 1f)]
    [SerializeField] private float beatScaleRestoreProgress = 0.5f;

    [Space(15)]
    [Header("初期配置する敵グループ数")]
    [Min(0)]
    [SerializeField] private int initialGroupCount = 3;
    [Header("最初のグループが経過済みとする小節数")]
    [Min(0f)]
    [SerializeField] private float firstGroupElapsedMeasures = 1f;
    [Header("初期グループ間の小節間隔")]
    [Min(0f)]
    [SerializeField] private float initialGroupMeasureInterval = 2f;
    [Header("グループ内の撃破順に対応する得点")]
    [SerializeField] private int[] groupDefeatScores = { 100, 200, 300 };
    [Header("撃破得点のポップアップを表示する制御クラス")]
    [SerializeField] private EnemyScorePopupController scorePopupController;

    [Space(15)]
    [Header("盤面上の最大グループ数（0で上限なし）")]
    [Min(0)]
    [SerializeField] private int maxActiveGroupCount = 6;
    [Header("生存グループ数ごとの追加生成設定")]
    [SerializeField] private EnemyGroupSpawnSettings spawnSettings;
    [Header("同じ列へ配置するグループ間の最低Y距離")]
    [Tooltip("敵の高さの半分程度を目安に調整します")]
    [Min(0f)]
    [SerializeField] private float minimumGroupVerticalSpacing = 0.5f;

    [Space(15)]
    [Header("Sceneビューに敵配置ガイドを表示する")]
    [SerializeField] private bool showSpawnGuide = true;
    [Header("グリッドガイドの色")]
    [SerializeField] private Color gridGuideColor =
        new Color(0.2f, 0.8f, 1f, 0.6f);
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

    private readonly List<EnemyGroupController> activeGroups = new();
    private int nextGroupNumber;
    private int lastSuccessfulSpawnMeasureIndex;
    private bool hasStartedMeasureScheduling;
    private bool isReady;
    private bool isMeasureSubscribed;

    private void Start()
    {
        isReady = CanSpawnInitialGroups();
        if (!isReady)
        {
            return;
        }

        SpawnInitialGroups();
        TrySubscribeToMeasure();
    }

    private void OnEnable()
    {
        TrySubscribeToMeasure();
    }

    private void OnDisable()
    {
        UnsubscribeFromMeasure();
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnGuide)
        {
            return;
        }

        DrawGuideMarker(gridLeftPoint, Color.yellow, "GridLeftPoint");
        DrawGuideMarker(gridRightPoint, Color.yellow, "GridRightPoint");
        DrawGuideMarker(
            entryStartPoint,
            entryStartGuideColor,
            "EntryStartPoint"
        );
        DrawGuideMarker(
            normalStartPoint,
            normalStartGuideColor,
            "NormalStartPoint"
        );
        DrawGuideMarker(
            diveStartPoint,
            diveStartGuideColor,
            "DiveStartPoint"
        );
        DrawGuideMarker(
            diveEndPoint,
            diveEndGuideColor,
            "DiveEndPoint"
        );

        if (gridLeftPoint == null || gridRightPoint == null)
        {
            return;
        }

        DrawHorizontalGuide(entryStartPoint, entryStartGuideColor);
        DrawHorizontalGuide(normalStartPoint, normalStartGuideColor);
        DrawHorizontalGuide(diveStartPoint, diveStartGuideColor);
        DrawHorizontalGuide(diveEndPoint, diveEndGuideColor);
        DrawGridColumns();
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
            gridLeftPoint.position.x,
            point.position.y,
            guideZ
        );
        Vector3 rightPosition = new Vector3(
            gridRightPoint.position.x,
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
        if (normalStartPoint == null || diveStartPoint == null ||
            gridColumnCount < 2)
        {
            return;
        }

        float guideZ = GetGuideZ();
        Gizmos.color = gridGuideColor;

        for (int column = 0; column < gridColumnCount; column++)
        {
            float columnX = GetColumnX(column);
            Vector3 startPosition = new Vector3(
                columnX,
                normalStartPoint.position.y,
                guideZ
            );
            Vector3 endPosition = new Vector3(
                columnX,
                diveStartPoint.position.y,
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
        return (gridLeftPoint.position.z + gridRightPoint.position.z) * 0.5f;
    }

    /// <summary>
    /// 初期グループの生成に必要な設定を確認する。
    /// </summary>
    /// <returns>初期グループを生成できる場合は true</returns>
    private bool CanSpawnInitialGroups()
    {
        if (gridLeftPoint == null || gridRightPoint == null ||
            entryStartPoint == null || normalStartPoint == null ||
            diveStartPoint == null || diveEndPoint == null)
        {
            Debug.LogError($"{name}: 敵配置用の基準位置が設定されていません。");
            return false;
        }

        if (musicBeatClock == null || musicBeatClock.BeatsPerMeasure <= 0)
        {
            Debug.LogError($"{name}: 敵グループ用のMusicBeatClockが設定されていません。");
            return false;
        }

        if (groupEnemyColumnStep < 1)
        {
            Debug.LogError($"{name}: グループ内の敵の列間隔は1以上にしてください。");
            return false;
        }

        if (minimumGroupGapColumns < 0)
        {
            Debug.LogError($"{name}: グループ間の最低空き列数は0以上にしてください。");
            return false;
        }

        if (minimumGroupVerticalSpacing < 0f)
        {
            Debug.LogError($"{name}: グループ間の最低Y距離は0以上にしてください。");
            return false;
        }

        if (spawnSettings == null ||
            !spawnSettings.TryGetRule(0, out _))
        {
            Debug.LogError($"{name}: 追加生成設定が正しく設定されていません。");
            return false;
        }

        if (maxActiveGroupCount > 0 &&
            initialGroupCount > maxActiveGroupCount)
        {
            Debug.LogError($"{name}: 初期グループ数が最大数を超えています。");
            return false;
        }

        int groupColumnSpan = GetGroupColumnSpan();
        if (gridColumnCount < groupColumnSpan)
        {
            Debug.LogError($"{name}: 1グループを配置できるグリッド列数がありません。");
            return false;
        }

        int requiredColumnCount = initialGroupCount == 0
            ? 0
            : initialGroupCount * groupColumnSpan +
                (initialGroupCount - 1) * minimumGroupGapColumns;
        if (requiredColumnCount > gridColumnCount)
        {
            Debug.LogError($"{name}: 初期グループを重なりなく配置できる列数がありません。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 通常フェーズ序盤の異なる進行位置に初期グループを配置する。
    /// </summary>
    private void SpawnInitialGroups()
    {
        if (!TryChooseGroupStartColumns(out List<int> startColumns))
        {
            Debug.LogError($"{name}: 初期グループの配置を決定できませんでした。");
            return;
        }

        for (int groupIndex = 0; groupIndex < startColumns.Count; groupIndex++)
        {
            float elapsedMeasures = GetInitialGroupElapsedMeasures(groupIndex);
            float groupY = GetInitialGroupY(groupIndex);

            SpawnGroup(
                startColumns[groupIndex],
                groupY,
                elapsedMeasures,
                false
            );
        }
    }

    /// <summary>
    /// 初期グループが通常フェーズで経過済みとする小節数を返す。
    /// </summary>
    /// <param name="groupIndex">0始まりの初期グループ番号</param>
    /// <returns>通常フェーズで経過済みとする小節数</returns>
    private float GetInitialGroupElapsedMeasures(int groupIndex)
    {
        return firstGroupElapsedMeasures +
            initialGroupMeasureInterval * groupIndex;
    }

    /// <summary>
    /// 初期グループを配置する予定Y座標を返す。
    /// </summary>
    /// <param name="groupIndex">0始まりの初期グループ番号</param>
    /// <returns>初期グループの予定Y座標</returns>
    private float GetInitialGroupY(int groupIndex)
    {
        float elapsedMeasures = GetInitialGroupElapsedMeasures(groupIndex);
        float normalPhaseProgress = Mathf.Clamp01(
            elapsedMeasures / normalPhaseMeasureCount
        );

        return Mathf.Lerp(
            normalStartPoint.position.y,
            diveStartPoint.position.y,
            normalPhaseProgress
        );
    }

    /// <summary>
    /// 小節の先頭で生存グループ数に応じた追加生成を判定する。
    /// </summary>
    /// <param name="measureIndex">曲全体での小節番号</param>
    private void HandleMeasure(int measureIndex)
    {
        if (!isReady)
        {
            return;
        }

        RemoveDestroyedGroups();

        if (!hasStartedMeasureScheduling)
        {
            hasStartedMeasureScheduling = true;
            lastSuccessfulSpawnMeasureIndex = measureIndex;
            return;
        }

        if (maxActiveGroupCount > 0 &&
            activeGroups.Count >= maxActiveGroupCount)
        {
            return;
        }

        if (!spawnSettings.TryGetRule(
            activeGroups.Count,
            out EnemyGroupSpawnRule spawnRule))
        {
            return;
        }

        if (measureIndex - lastSuccessfulSpawnMeasureIndex <
            spawnRule.SpawnIntervalMeasures)
        {
            return;
        }

        if (TrySpawnAdditionalGroups(spawnRule.SpawnGroupCount) > 0)
        {
            lastSuccessfulSpawnMeasureIndex = measureIndex;
        }
    }

    /// <summary>
    /// 設定数まで、空いている列へ追加グループを生成する。
    /// </summary>
    /// <param name="requestedGroupCount">生成を試みるグループ数</param>
    /// <returns>実際に生成できたグループ数</returns>
    private int TrySpawnAdditionalGroups(int requestedGroupCount)
    {
        int availableGroupCount = requestedGroupCount;
        if (maxActiveGroupCount > 0)
        {
            availableGroupCount = Mathf.Min(
                availableGroupCount,
                maxActiveGroupCount - activeGroups.Count
            );
        }

        int spawnedGroupCount = 0;
        for (int groupIndex = 0;
            groupIndex < availableGroupCount;
            groupIndex++)
        {
            if (!TrySpawnAdditionalGroup())
            {
                break;
            }

            spawnedGroupCount++;
        }

        return spawnedGroupCount;
    }

    /// <summary>
    /// 空いている列を選び、進入フェーズからグループを生成する。
    /// </summary>
    /// <returns>生成できた場合は true</returns>
    private bool TrySpawnAdditionalGroup()
    {
        List<int> candidates =
            CreateDistanceWeightedStartColumnCandidates();
        foreach (int startColumn in candidates)
        {
            if (!CanSpawnAtEntry(startColumn))
            {
                continue;
            }

            return SpawnGroup(
                startColumn,
                entryStartPoint.position.y,
                0f,
                true
            );
        }

        return false;
    }

    /// <summary>
    /// 進入開始位置の指定列にグループを配置できるか確認する。
    /// </summary>
    /// <param name="startColumn">候補とする開始列</param>
    /// <returns>他グループと許容範囲外で重ならない場合は true</returns>
    private bool CanSpawnAtEntry(int startColumn)
    {
        int candidateFirstColumn = startColumn - minimumGroupGapColumns;
        int candidateLastColumn = startColumn + GetGroupColumnSpan() - 1 +
            minimumGroupGapColumns;

        foreach (EnemyGroupController activeGroup in activeGroups)
        {
            if (activeGroup == null)
            {
                continue;
            }

            float verticalDistance = Mathf.Abs(
                activeGroup.transform.position.y - entryStartPoint.position.y
            );
            if (verticalDistance >= minimumGroupVerticalSpacing)
            {
                continue;
            }

            int activeFirstColumn = activeGroup.StartColumn;
            int activeLastColumn = activeFirstColumn +
                activeGroup.ColumnSpan - 1;
            bool overlapsHorizontally =
                candidateFirstColumn <= activeLastColumn &&
                candidateLastColumn >= activeFirstColumn;
            if (overlapsHorizontally)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 破棄済みのグループを生存リストから取り除く。
    /// </summary>
    private void RemoveDestroyedGroups()
    {
        activeGroups.RemoveAll(group => group == null);
    }

    /// <summary>
    /// グループ同士が横方向に重ならない開始列をランダムに選ぶ。
    /// </summary>
    /// <param name="selectedColumns">選択した各グループの開始列</param>
    /// <returns>必要数分の列を選択できた場合は true</returns>
    private bool TryChooseGroupStartColumns(out List<int> selectedColumns)
    {
        selectedColumns = new List<int>();
        bool[] occupiedColumns = new bool[gridColumnCount];

        return TryChooseGroupStartColumns(
            occupiedColumns,
            selectedColumns,
            initialGroupCount
        );
    }

    /// <summary>
    /// 必要な列幅とグループ間隔を確保できる配置を組み立てる。
    /// </summary>
    /// <param name="occupiedColumns">使用中の列</param>
    /// <param name="selectedColumns">選択済みの開始列</param>
    /// <param name="remainingGroupCount">まだ配置するグループ数</param>
    /// <returns>必要数分を配置できた場合は true</returns>
    private bool TryChooseGroupStartColumns(
        bool[] occupiedColumns,
        List<int> selectedColumns,
        int remainingGroupCount)
    {
        if (remainingGroupCount == 0)
        {
            return true;
        }

        List<int> candidates =
            CreateDistanceWeightedStartColumnCandidates(selectedColumns);
        foreach (int startColumn in candidates)
        {
            if (!CanUseColumns(occupiedColumns, startColumn))
            {
                continue;
            }

            SetColumnsOccupied(occupiedColumns, startColumn, true);
            selectedColumns.Add(startColumn);

            if (TryChooseGroupStartColumns(
                occupiedColumns,
                selectedColumns,
                remainingGroupCount - 1))
            {
                return true;
            }

            selectedColumns.RemoveAt(selectedColumns.Count - 1);
            SetColumnsOccupied(occupiedColumns, startColumn, false);
        }

        return false;
    }

    /// <summary>
    /// 離れた位置を優先した重み付きランダム順で開始列を作成する。
    /// </summary>
    /// <param name="selectedStartColumns">
    /// 初期配置でこの処理より前に選択済みの開始列
    /// </param>
    /// <returns>ランダムに並べた開始列番号</returns>
    private List<int> CreateDistanceWeightedStartColumnCandidates(
        List<int> selectedStartColumns = null)
    {
        int candidateCount = gridColumnCount - GetGroupColumnSpan() + 1;
        List<int> remainingCandidates = new List<int>(candidateCount);
        List<int> orderedCandidates = new List<int>(candidateCount);

        for (int column = 0; column < candidateCount; column++)
        {
            remainingCandidates.Add(column);
        }

        while (remainingCandidates.Count > 0)
        {
            int selectedIndex = ChooseWeightedCandidateIndex(
                remainingCandidates,
                selectedStartColumns
            );
            orderedCandidates.Add(remainingCandidates[selectedIndex]);
            remainingCandidates.RemoveAt(selectedIndex);
        }

        return orderedCandidates;
    }

    /// <summary>
    /// 生存グループから離れた候補ほど高い重みで1列を選択する。
    /// </summary>
    /// <param name="candidates">選択対象の開始列</param>
    /// <param name="selectedStartColumns">
    /// 初期配置でこの処理より前に選択済みの開始列
    /// </param>
    /// <returns>選択された候補のリスト内インデックス</returns>
    private int ChooseWeightedCandidateIndex(
        List<int> candidates,
        List<int> selectedStartColumns)
    {
        float totalWeight = 0f;
        float[] candidateWeights = new float[candidates.Count];

        for (int index = 0; index < candidates.Count; index++)
        {
            float weight = GetSpawnDistanceWeight(
                candidates[index],
                selectedStartColumns
            );
            candidateWeights[index] = weight;
            totalWeight += weight;
        }

        float selectedWeight = Random.value * totalWeight;
        for (int index = 0; index < candidateWeights.Length; index++)
        {
            selectedWeight -= candidateWeights[index];
            if (selectedWeight <= 0f)
            {
                return index;
            }
        }

        return candidates.Count - 1;
    }

    /// <summary>
    /// Y位置の影響を反映した横距離から生成候補の重みを計算する。
    /// </summary>
    /// <param name="startColumn">候補とする開始列</param>
    /// <param name="selectedStartColumns">
    /// 初期配置でこの処理より前に選択済みの開始列
    /// </param>
    /// <returns>重み付きランダム選択で使用する重み</returns>
    private float GetSpawnDistanceWeight(
        int startColumn,
        List<int> selectedStartColumns)
    {
        float closestAdjustedDistance = float.PositiveInfinity;
        float candidateCenter = GetGroupCenterColumn(
            startColumn,
            GetGroupColumnSpan()
        );

        foreach (EnemyGroupController activeGroup in activeGroups)
        {
            if (activeGroup == null)
            {
                continue;
            }

            float activeGroupCenter = GetGroupCenterColumn(
                activeGroup.StartColumn,
                activeGroup.ColumnSpan
            );
            float adjustedDistance = GetInfluenceAdjustedHorizontalDistance(
                candidateCenter,
                activeGroupCenter,
                activeGroup.transform.position.y
            );
            closestAdjustedDistance = Mathf.Min(
                closestAdjustedDistance,
                adjustedDistance
            );
        }

        if (selectedStartColumns != null)
        {
            for (int groupIndex = 0;
                groupIndex < selectedStartColumns.Count;
                groupIndex++)
            {
                int selectedStartColumn = selectedStartColumns[groupIndex];
                float selectedGroupCenter = GetGroupCenterColumn(
                    selectedStartColumn,
                    GetGroupColumnSpan()
                );
                float adjustedDistance =
                    GetInfluenceAdjustedHorizontalDistance(
                        candidateCenter,
                        selectedGroupCenter,
                        GetInitialGroupY(groupIndex)
                    );
                closestAdjustedDistance = Mathf.Min(
                    closestAdjustedDistance,
                    adjustedDistance
                );
            }
        }

        if (float.IsPositiveInfinity(closestAdjustedDistance))
        {
            return 1f;
        }

        float maximumRelevantDistance = gridColumnCount + 1f;
        float weightedDistance = Mathf.Min(
            closestAdjustedDistance,
            maximumRelevantDistance
        );

        return Mathf.Pow(
            weightedDistance,
            spawnDistanceBiasPower
        );
    }

    /// <summary>
    /// Y位置による影響倍率を反映した候補との横距離を返す。
    /// </summary>
    /// <param name="candidateCenter">生成候補の中心列</param>
    /// <param name="groupCenter">比較するグループの中心列</param>
    /// <param name="groupY">比較するグループのY座標</param>
    /// <returns>影響倍率で補正した横距離</returns>
    private float GetInfluenceAdjustedHorizontalDistance(
        float candidateCenter,
        float groupCenter,
        float groupY)
    {
        float influence = GetSpawnPositionInfluence(groupY);
        if (influence <= 0f)
        {
            return float.PositiveInfinity;
        }

        float horizontalDistance = Mathf.Abs(candidateCenter - groupCenter);
        return (horizontalDistance + 1f) / influence;
    }

    /// <summary>
    /// 生成位置を1、突入開始位置を0とする配置への影響倍率を返す。
    /// </summary>
    /// <param name="groupY">グループのY座標</param>
    /// <returns>生成位置への近さを表す0から1の影響倍率</returns>
    private float GetSpawnPositionInfluence(float groupY)
    {
        float entryY = entryStartPoint.position.y;
        float verticalRange = Mathf.Abs(
            entryY - diveStartPoint.position.y
        );
        if (Mathf.Approximately(verticalRange, 0f))
        {
            return 1f;
        }

        float distanceFromEntry = Mathf.Abs(groupY - entryY);
        return 1f - Mathf.Clamp01(distanceFromEntry / verticalRange);
    }

    /// <summary>
    /// グループの開始列と列幅から中心列を返す。
    /// </summary>
    /// <param name="startColumn">グループの開始列</param>
    /// <param name="columnSpan">グループが占有する列数</param>
    /// <returns>グループの中心列</returns>
    private float GetGroupCenterColumn(int startColumn, int columnSpan)
    {
        return startColumn + (columnSpan - 1) * 0.5f;
    }

    /// <summary>
    /// 開始列からグループの列幅と最低間隔を確保できるか確認する。
    /// </summary>
    /// <param name="occupiedColumns">使用中の列</param>
    /// <param name="startColumn">確認する開始列</param>
    /// <returns>必要な列が空いている場合は true</returns>
    private bool CanUseColumns(bool[] occupiedColumns, int startColumn)
    {
        int firstCheckedColumn = Mathf.Max(
            0,
            startColumn - minimumGroupGapColumns
        );
        int lastCheckedColumn = Mathf.Min(
            gridColumnCount - 1,
            startColumn + GetGroupColumnSpan() - 1 + minimumGroupGapColumns
        );

        for (int column = firstCheckedColumn;
            column <= lastCheckedColumn;
            column++)
        {
            if (occupiedColumns[column])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 開始列からグループが占有する列幅の使用状態を更新する。
    /// </summary>
    /// <param name="occupiedColumns">使用状態を保持する配列</param>
    /// <param name="startColumn">更新する開始列</param>
    /// <param name="isOccupied">使用中にする場合は true</param>
    private void SetColumnsOccupied(
        bool[] occupiedColumns,
        int startColumn,
        bool isOccupied)
    {
        int groupColumnSpan = GetGroupColumnSpan();
        for (int offset = 0; offset < groupColumnSpan; offset++)
        {
            occupiedColumns[startColumn + offset] = isOccupied;
        }
    }

    /// <summary>
    /// 3体の敵とその間隔を含むグループ全体の列幅を返す。
    /// </summary>
    /// <returns>グループが占有する列数</returns>
    private int GetGroupColumnSpan()
    {
        return (EnemiesPerGroup - 1) * groupEnemyColumnStep + 1;
    }

    /// <summary>
    /// 指定した列とY座標に3体の敵グループを生成する。
    /// </summary>
    /// <param name="startColumn">グループの開始列</param>
    /// <param name="groupY">グループのY座標</param>
    /// <param name="elapsedMeasures">通常フェーズで経過済みとする小節数</param>
    /// <param name="startsInEntryPhase">進入フェーズから開始する場合は true</param>
    /// <returns>敵1体以上を生成できた場合は true</returns>
    private bool SpawnGroup(
        int startColumn,
        float groupY,
        float elapsedMeasures,
        bool startsInEntryPhase)
    {
        float firstEnemyX = GetColumnX(startColumn);
        float lastEnemyX = GetColumnX(
            startColumn + (EnemiesPerGroup - 1) * groupEnemyColumnStep
        );
        float groupX = (firstEnemyX + lastEnemyX) * 0.5f;

        nextGroupNumber++;
        GameObject groupObject = new GameObject(
            $"EnemyGroup_{nextGroupNumber:00}"
        );
        groupObject.transform.SetParent(transform, false);
        groupObject.transform.position = new Vector3(
            groupX,
            groupY,
            transform.position.z
        );

        EnemyGroupController enemyGroup =
            groupObject.AddComponent<EnemyGroupController>();
        enemyGroup.SetDefeatScores(groupDefeatScores);
        enemyGroup.SetScorePopupController(scorePopupController);
        enemyGroup.SetGridPlacement(startColumn, GetGroupColumnSpan());

        EnemyGroupMovement groupMovement =
            groupObject.AddComponent<EnemyGroupMovement>();
        InitializeGroupMovement(
            groupMovement,
            elapsedMeasures,
            startsInEntryPhase
        );

        int registeredEnemyCount = 0;
        for (int offset = 0; offset < EnemiesPerGroup; offset++)
        {
            Vector3 spawnPosition = new Vector3(
                GetColumnX(startColumn + offset * groupEnemyColumnStep),
                groupY,
                transform.position.z
            );
            EnemyMove enemy = GetEnemy(spawnPosition);
            if (enemy == null)
            {
                continue;
            }

            if (!enemy.TryGetComponent(out EnemyHealth enemyHealth))
            {
                Debug.LogError($"{enemy.name}: EnemyHealthが見つかりません。");
                PoolManager.Instance.Return(enemyPoolKey, enemy.gameObject);
                continue;
            }

            enemy.enabled = false;
            enemyGroup.RegisterEnemy(enemyHealth);
            groupMovement.RegisterEnemy(enemy.transform);
            registeredEnemyCount++;
        }

        if (registeredEnemyCount == 0)
        {
            Destroy(groupObject);
            return false;
        }

        activeGroups.Add(enemyGroup);
        return true;
    }

    /// <summary>
    /// 初期配置か追加生成かに応じてグループ移動を初期化する。
    /// </summary>
    /// <param name="groupMovement">初期化するグループ移動</param>
    /// <param name="elapsedMeasures">通常フェーズの経過済み小節数</param>
    /// <param name="startsInEntryPhase">進入フェーズから開始する場合は true</param>
    private void InitializeGroupMovement(
        EnemyGroupMovement groupMovement,
        float elapsedMeasures,
        bool startsInEntryPhase)
    {
        if (startsInEntryPhase)
        {
            groupMovement.InitializeEntryPhase(
                musicBeatClock,
                entryStartPoint.position.y,
                normalStartPoint.position.y,
                diveStartPoint.position.y,
                diveEndPoint.position.y,
                entryPhaseMeasureCount,
                normalPhaseMeasureCount,
                diveChargeBeatCount,
                diveMovementBeatCount,
                entryDecelerationPower,
                diveChargeScaleX,
                diveChargeScaleY,
                beatSquashScaleX,
                beatSquashScaleY,
                beatScaleRestoreProgress
            );
            return;
        }

        groupMovement.InitializeNormalPhase(
            musicBeatClock,
            entryStartPoint.position.y,
            normalStartPoint.position.y,
            diveStartPoint.position.y,
            diveEndPoint.position.y,
            entryPhaseMeasureCount,
            normalPhaseMeasureCount,
            elapsedMeasures,
            diveChargeBeatCount,
            diveMovementBeatCount,
            entryDecelerationPower,
            diveChargeScaleX,
            diveChargeScaleY,
            beatSquashScaleX,
            beatSquashScaleY,
            beatScaleRestoreProgress
        );
    }

    /// <summary>
    /// 小節通知を受け取れる場合にMusicBeatClockを購読する。
    /// </summary>
    private void TrySubscribeToMeasure()
    {
        if (!isActiveAndEnabled || !isReady || musicBeatClock == null ||
            isMeasureSubscribed)
        {
            return;
        }

        musicBeatClock.OnMeasure += HandleMeasure;
        isMeasureSubscribed = true;
    }

    /// <summary>
    /// MusicBeatClockの小節通知購読を解除する。
    /// </summary>
    private void UnsubscribeFromMeasure()
    {
        if (!isMeasureSubscribed || musicBeatClock == null)
        {
            return;
        }

        musicBeatClock.OnMeasure -= HandleMeasure;
        isMeasureSubscribed = false;
    }

    /// <summary>
    /// グリッド列に対応するワールドX座標を返す。
    /// </summary>
    /// <param name="column">左から0始まりの列番号</param>
    /// <returns>指定列のワールドX座標</returns>
    private float GetColumnX(int column)
    {
        float normalizedColumn = (float)column / (gridColumnCount - 1);
        return Mathf.Lerp(
            gridLeftPoint.position.x,
            gridRightPoint.position.x,
            normalizedColumn
        );
    }

    /// <summary>
    /// プールから敵を1体取得し、指定位置に配置する。
    /// </summary>
    /// <param name="spawnPosition">敵の生成位置</param>
    /// <returns>取得した敵。取得できない場合は null</returns>
    private EnemyMove GetEnemy(Vector3 spawnPosition)
    {
        if (PoolManager.Instance == null || string.IsNullOrEmpty(enemyPoolKey))
        {
            Debug.LogError($"{name}: PoolManagerまたは敵のプールキーが設定されていません。");
            return null;
        }

        GameObject enemyObject = PoolManager.Instance.Get(enemyPoolKey);
        if (enemyObject == null)
        {
            Debug.LogError($"{name}: プールから{enemyPoolKey}を取得できませんでした。");
            return null;
        }

        if (!enemyObject.TryGetComponent(out EnemyMove enemy))
        {
            Debug.LogError($"{name}: {enemyPoolKey}にEnemyMoveがありません。");
            PoolManager.Instance.Return(enemyPoolKey, enemyObject);
            return null;
        }

        enemy.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        return enemy;
    }
}
