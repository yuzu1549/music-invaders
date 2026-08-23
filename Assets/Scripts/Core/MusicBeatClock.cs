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
    private bool isClockInitialized;

    private void Update()
    {
        if (noteManager == null || !noteManager.IsMusicPlaying)
        {
            return;
        }

        float secondsPerBeat = noteManager.SecondsPerBeat;
        if (secondsPerBeat <= 0f || noteManager.BeatsPerMeasure <= 0)
        {
            return;
        }

        float musicTime = noteManager.CurrentMusicTimeSeconds;
        if (musicTime < 0f)
        {
            return;
        }

        int beatIndex = Mathf.FloorToInt(musicTime / secondsPerBeat);
        BeatProgress = Mathf.Repeat(musicTime, secondsPerBeat) /
            secondsPerBeat;

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
