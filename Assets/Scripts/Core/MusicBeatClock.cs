using System;
using UnityEngine;

public class MusicBeatClock : MonoBehaviour
{
    [Header("楽曲時間を管理するNoteManager")]
    [SerializeField] private NoteManager noteManager;

    public event Action<int> OnBeat;
    public event Action<int> OnMeasure;

    public int CurrentBeatIndex { get; private set; } = -1;
    public int CurrentMeasureIndex { get; private set; } = -1;
    public float BeatProgress { get; private set; }
    public int BeatsPerMeasure => noteManager != null
        ? noteManager.BeatsPerMeasure
        : 0;

    private int lastNotifiedBeatIndex = -1;
    private int observedTimingOffsetRevision;
    private bool isClockInitialized;
    private bool shouldRebaseClock;

    private void Update()
    {
        if (noteManager == null)
        {
            return;
        }

        DetectTimingOffsetChange();

        if (!noteManager.IsMusicPlaying)
        {
            return;
        }

        float secondsPerBeat = noteManager.SecondsPerBeat;
        if (secondsPerBeat <= 0f || noteManager.BeatsPerMeasure <= 0)
        {
            return;
        }

        float enemyElapsedTime = noteManager.CurrentMusicTimeSeconds -
            noteManager.EnemyBeatStartTimeSeconds;
        if (enemyElapsedTime < 0f)
        {
            ResetClock();
            return;
        }

        int beatIndex = Mathf.FloorToInt(enemyElapsedTime / secondsPerBeat);
        BeatProgress = Mathf.Repeat(enemyElapsedTime, secondsPerBeat) /
            secondsPerBeat;

        if (shouldRebaseClock)
        {
            RebaseClock(beatIndex);
            return;
        }

        if (!isClockInitialized || beatIndex < lastNotifiedBeatIndex)
        {
            lastNotifiedBeatIndex = beatIndex - 1;
            isClockInitialized = true;
        }

        for (int index = lastNotifiedBeatIndex + 1; index <= beatIndex; index++)
        {
            NotifyBeat(index);
        }
    }

    /// <summary>
    /// タイミング調整の変更を検出し、次回再生時の時計補正を予約する。
    /// </summary>
    private void DetectTimingOffsetChange()
    {
        int timingOffsetRevision = noteManager.TimingOffsetRevision;
        if (observedTimingOffsetRevision == timingOffsetRevision)
        {
            return;
        }

        observedTimingOffsetRevision = timingOffsetRevision;
        shouldRebaseClock = isClockInitialized;
    }

    /// <summary>
    /// 調整後の現在拍を基準にし、通過した拍を再通知しないようにする。
    /// </summary>
    /// <param name="beatIndex">調整後の現在拍</param>
    private void RebaseClock(int beatIndex)
    {
        CurrentBeatIndex = beatIndex;
        CurrentMeasureIndex = beatIndex / noteManager.BeatsPerMeasure;
        lastNotifiedBeatIndex = beatIndex;
        isClockInitialized = true;
        shouldRebaseClock = false;
    }

    /// <summary>
    /// 敵の開始拍より前の初期状態へ時計を戻す。
    /// </summary>
    private void ResetClock()
    {
        CurrentBeatIndex = -1;
        CurrentMeasureIndex = -1;
        BeatProgress = 0f;
        lastNotifiedBeatIndex = -1;
        isClockInitialized = false;
        shouldRebaseClock = false;
    }

    /// <summary>
    /// 拍と、必要な場合は小節の開始を通知する。
    /// </summary>
    /// <param name="beatIndex">0から始まる拍番号</param>
    private void NotifyBeat(int beatIndex)
    {
        CurrentBeatIndex = beatIndex;
        lastNotifiedBeatIndex = beatIndex;
        OnBeat?.Invoke(beatIndex);

        int beatsPerMeasure = noteManager.BeatsPerMeasure;
        if (beatIndex % beatsPerMeasure != 0)
        {
            return;
        }

        CurrentMeasureIndex = beatIndex / beatsPerMeasure;
        OnMeasure?.Invoke(CurrentMeasureIndex);
    }
}
