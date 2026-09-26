using UnityEngine;

/// <summary>残りライフ1の警告枠をフェードインし、ゆっくり明滅させる。</summary>
[RequireComponent(typeof(ScreenEdgeWarningGraphic))]
[DisallowMultipleComponent]
public class LowLifeWarningController : MonoBehaviour
{
    [Header("ライフを参照する自機")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("表示開始時のフェード秒数")]
    [Min(0f)]
    [SerializeField] private float fadeInSeconds = 0.3f;

    [Header("濃淡が一巡する秒数")]
    [Min(0.1f)]
    [SerializeField] private float pulsePeriodSeconds = 2f;

    [Header("明滅の最小不透明度倍率")]
    [Range(0f, 1f)]
    [SerializeField] private float minimumOpacity = 0.4f;

    [Header("明滅の最大不透明度倍率")]
    [Range(0f, 1f)]
    [SerializeField] private float maximumOpacity = 0.7f;

    private ScreenEdgeWarningGraphic warningGraphic;
    private float fadeElapsed;
    private float pulseElapsed;

    private void Awake()
    {
        warningGraphic = GetComponent<ScreenEdgeWarningGraphic>();
        warningGraphic.raycastTarget = false;
        warningGraphic.canvasRenderer.SetAlpha(0f);
    }

    private void OnEnable()
    {
        HideWarning();
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("LowLifeWarningControllerにPlayerHealthを設定してください。", this);
        }
    }

    private void LateUpdate()
    {
        bool finished = GameManager.Instance != null &&
            (GameManager.Instance.isGameOver || GameManager.Instance.isGameCleared);
        if (playerHealth == null || playerHealth.currentHealth != 1 || finished)
        {
            HideWarning();
            return;
        }

        // 終了時の非表示は、timeScaleが0でも優先して反映する。
        if (Time.deltaTime <= 0f) return;

        fadeElapsed += Time.deltaTime;
        float period = Mathf.Max(0.1f, pulsePeriodSeconds);
        pulseElapsed = Mathf.Repeat(pulseElapsed + Time.deltaTime, period);
        float pulse = 0.5f - 0.5f * Mathf.Cos(pulseElapsed / period * Mathf.PI * 2f);
        float opacity = Mathf.Lerp(Mathf.Min(minimumOpacity, maximumOpacity),
            Mathf.Max(minimumOpacity, maximumOpacity), pulse);
        float fade = fadeInSeconds > 0f
            ? Mathf.Clamp01(fadeElapsed / fadeInSeconds) : 1f;

        // 頂点の再生成を避け、描画時の透明度だけを更新する。
        warningGraphic.canvasRenderer.SetAlpha(opacity * fade);
    }

    private void OnDisable()
    {
        HideWarning();
    }

    /// <summary>警告を即座に隠し、次回表示用の時間を初期化する。</summary>
    private void HideWarning()
    {
        fadeElapsed = 0f;
        pulseElapsed = 0f;
        if (warningGraphic != null)
        {
            warningGraphic.canvasRenderer.SetAlpha(0f);
        }
    }
}
