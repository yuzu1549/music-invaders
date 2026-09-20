using UnityEngine;

// 拍通知より先に終了を確定し、終了拍での追加生成を防ぐ。
[DefaultExecutionOrder(-10)]
public class GameEndingController : MonoBehaviour
{
    [Header("楽曲とノーツの管理")]
    [SerializeField] private NoteManager noteManager;

    [Header("敵の拍クロック")]
    [SerializeField] private MusicBeatClock musicBeatClock;

    [Header("クリア表示とリザルト遷移")]
    [SerializeField] private GameFinish gameFinish;

    [Header("クリア表示までの共通待機秒数")]
    [Min(0f)]
    [SerializeField] private float clearDelaySeconds = 2f;

    private bool isWaiting;
    private bool isCompleted;
    private float elapsedWaitSeconds;

    private void OnValidate()
    {
        if (float.IsNaN(clearDelaySeconds) || float.IsInfinity(clearDelaySeconds))
        {
            clearDelaySeconds = 2f;
        }

        clearDelaySeconds = Mathf.Max(0f, clearDelaySeconds);
    }

    private void Awake()
    {
        if (noteManager == null || musicBeatClock == null || gameFinish == null)
        {
            Debug.LogError("GameEndingControllerの参照をすべて設定してください。", this);
            enabled = false;
            return;
        }

        noteManager.RegisterEndingController();
    }

    private void Update()
    {
        if (isCompleted || noteManager.IsPlaybackPaused || Time.timeScale == 0f)
        {
            return;
        }

        if (GameManager.Instance != null &&
            (GameManager.Instance.isGameOver || GameManager.Instance.isGameCleared))
        {
            isCompleted = true;
            musicBeatClock.StopCombat();
            return;
        }

        if (!isWaiting)
        {
            if (!noteManager.HasReachedEnding)
            {
                return;
            }

            isWaiting = true;
            musicBeatClock.StopCombat();
        }
        else
        {
            elapsedWaitSeconds += Time.deltaTime;
        }

        float duration = Mathf.Max(0f, clearDelaySeconds);
        noteManager.SetEndingFade(duration > 0f
            ? 1f - elapsedWaitSeconds / duration
            : 0f);
    }

    private void LateUpdate()
    {
        // このフレームの被弾を優先し、ライフ0とクリアの競合を防ぐ。
        if (!isWaiting || isCompleted || noteManager.IsPlaybackPaused ||
            Time.timeScale == 0f || elapsedWaitSeconds < clearDelaySeconds)
        {
            return;
        }

        isCompleted = true;
        gameFinish.GameClear();
    }
}
