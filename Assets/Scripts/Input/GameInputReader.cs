using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲームプレイ中に使う入力値を Input Actions から読み取る。
/// </summary>
public class GameInputReader : MonoBehaviour
{
    [Header("ゲーム操作を定義した Input Actions")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap gameplayMap;
    private InputAction rhythmLeftAction;
    private InputAction rhythmRightAction;
    private InputAction invaderMoveAction;

    private void Awake()
    {
        InitializeActions();
    }

    private void OnEnable()
    {
        gameplayMap?.Enable();
    }

    private void OnDisable()
    {
        gameplayMap?.Disable();
    }

    /// <summary>
    /// リズムゲームの左入力が押された瞬間かどうかを返す。
    /// </summary>
    /// <returns>左入力が押された瞬間なら true</returns>
    public bool WasRhythmLeftPressed()
    {
        return rhythmLeftAction != null && rhythmLeftAction.WasPressedThisFrame();
    }

    /// <summary>
    /// リズムゲームの右入力が押された瞬間かどうかを返す。
    /// </summary>
    /// <returns>右入力が押された瞬間なら true</returns>
    public bool WasRhythmRightPressed()
    {
        return rhythmRightAction != null && rhythmRightAction.WasPressedThisFrame();
    }

    /// <summary>
    /// インベーダーの左右移動入力を -1 から 1 の値で返す。
    /// </summary>
    /// <returns>左入力なら負、右入力なら正、未入力なら 0</returns>
    public float GetInvaderMoveValue()
    {
        if (invaderMoveAction == null)
        {
            return 0f;
        }

        return invaderMoveAction.ReadValue<float>();
    }

    /// <summary>
    /// Input Actions からゲームプレイ用 Action を取得する。
    /// </summary>
    private void InitializeActions()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("GameInputReader に Input Actions が設定されていません。");
            return;
        }

        gameplayMap = inputActions.FindActionMap(
            GameInputBindingSettings.GameplayMapName);
        if (gameplayMap == null)
        {
            Debug.LogWarning("Gameplay Action Map が見つかりません。");
            return;
        }

        rhythmLeftAction = gameplayMap.FindAction(
            GameInputBindingSettings.RhythmLeftActionName);
        rhythmRightAction = gameplayMap.FindAction(
            GameInputBindingSettings.RhythmRightActionName);
        invaderMoveAction = gameplayMap.FindAction(
            GameInputBindingSettings.InvaderMoveActionName);

        WarnIfActionIsMissing(
            rhythmLeftAction,
            GameInputBindingSettings.RhythmLeftActionName);
        WarnIfActionIsMissing(
            rhythmRightAction,
            GameInputBindingSettings.RhythmRightActionName);
        WarnIfActionIsMissing(
            invaderMoveAction,
            GameInputBindingSettings.InvaderMoveActionName);
    }

    /// <summary>
    /// Action が見つからない場合に警告を出す。
    /// </summary>
    /// <param name="action">確認する Action</param>
    /// <param name="actionName">Action 名</param>
    private void WarnIfActionIsMissing(InputAction action, string actionName)
    {
        if (action != null)
        {
            return;
        }

        Debug.LogWarning($"{actionName} Action が見つかりません。");
    }
}
