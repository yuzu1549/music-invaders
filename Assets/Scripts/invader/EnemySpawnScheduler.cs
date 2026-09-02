using UnityEngine;

public class EnemySpawnScheduler : MonoBehaviour
{
    [Header("敵グループの拍同期に使うクロック")]
    [SerializeField] private MusicBeatClock musicBeatClock;

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

    [Space(15)]
    [Header("盤面上の最大グループ数（0で上限なし）")]
    [Min(0)]
    [SerializeField] private int maxActiveGroupCount = 6;
    [Header("生存グループ数ごとの追加生成設定")]
    [SerializeField] private EnemyGroupSpawnSettings spawnSettings;

    private EnemySpawner enemySpawner;
    private int lastSuccessfulSpawnMeasureIndex;
    private bool hasStartedMeasureScheduling;
    private bool isReady;
    private bool isMeasureSubscribed;

    public MusicBeatClock MusicBeatClock => musicBeatClock;

    private void Awake()
    {
        enemySpawner = GetComponent<EnemySpawner>();
    }

    private void Start()
    {
        TryBeginScheduling();
    }

    private void OnEnable()
    {
        TrySubscribeToMeasure();
    }

    private void OnDisable()
    {
        UnsubscribeFromMeasure();
    }

    /// <summary>
    /// 初期グループを生成し、小節単位の追加生成を開始する。
    /// </summary>
    private void TryBeginScheduling()
    {
        if (!TryValidateSettings(out string errorMessage))
        {
            Debug.LogError($"{name}: {errorMessage}");
            return;
        }

        if (!enemySpawner.TrySpawnInitialGroups(
            initialGroupCount,
            firstGroupElapsedMeasures,
            initialGroupMeasureInterval))
        {
            return;
        }

        isReady = true;
        TrySubscribeToMeasure();
    }

    /// <summary>
    /// 敵生成スケジュールに必要な設定を確認する。
    /// </summary>
    /// <param name="errorMessage">設定に問題がある場合の説明</param>
    /// <returns>生成を開始できる設定の場合はtrue</returns>
    private bool TryValidateSettings(out string errorMessage)
    {
        if (enemySpawner == null)
        {
            errorMessage = "EnemySpawnerが設定されていません。";
            return false;
        }

        if (musicBeatClock == null || musicBeatClock.BeatsPerMeasure <= 0)
        {
            errorMessage = "MusicBeatClockが正しく設定されていません。";
            return false;
        }

        if (initialGroupCount < 0 || firstGroupElapsedMeasures < 0f ||
            initialGroupMeasureInterval < 0f)
        {
            errorMessage = "初期グループ設定に負の値は使用できません。";
            return false;
        }

        if (spawnSettings == null ||
            !spawnSettings.TryGetRule(0, out _))
        {
            errorMessage = "追加生成設定が正しく設定されていません。";
            return false;
        }

        if (maxActiveGroupCount > 0 &&
            initialGroupCount > maxActiveGroupCount)
        {
            errorMessage = "初期グループ数が最大数を超えています。";
            return false;
        }

        errorMessage = string.Empty;
        return true;
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

        int activeGroupCount = enemySpawner.ActiveGroupCount;
        if (!hasStartedMeasureScheduling)
        {
            hasStartedMeasureScheduling = true;
            lastSuccessfulSpawnMeasureIndex = measureIndex;
            return;
        }

        if (maxActiveGroupCount > 0 &&
            activeGroupCount >= maxActiveGroupCount)
        {
            return;
        }

        if (!spawnSettings.TryGetRule(
            activeGroupCount,
            out EnemyGroupSpawnRule spawnRule))
        {
            return;
        }

        if (measureIndex - lastSuccessfulSpawnMeasureIndex <
            spawnRule.SpawnIntervalMeasures)
        {
            return;
        }

        int spawnedGroupCount = enemySpawner.TrySpawnAdditionalGroups(
            spawnRule.SpawnGroupCount,
            maxActiveGroupCount
        );
        if (spawnedGroupCount > 0)
        {
            lastSuccessfulSpawnMeasureIndex = measureIndex;
        }
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
}
