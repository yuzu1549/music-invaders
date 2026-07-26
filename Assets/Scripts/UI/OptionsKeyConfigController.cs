using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

/// <summary>
/// オプション画面のキーコンフィグ UI を制御する。
/// </summary>
public class OptionsKeyConfigController : MonoBehaviour
{
    private enum KeyConfigTarget
    {
        None,
        RhythmLeft,
        RhythmRight,
        InvaderLeft,
        InvaderRight,
    }

    [Header("キー割り当て設定")]
    [SerializeField] private GameInputBindingSettings bindingSettings;

    [Space(15)]
    [Header("リズム左キーの表示テキスト")]
    [SerializeField] private TMP_Text rhythmLeftKeyText;
    [Header("リズム右キーの表示テキスト")]
    [SerializeField] private TMP_Text rhythmRightKeyText;
    [Header("インベーダー左キーの表示テキスト")]
    [SerializeField] private TMP_Text invaderLeftKeyText;
    [Header("インベーダー右キーの表示テキスト")]
    [SerializeField] private TMP_Text invaderRightKeyText;

    [Space(15)]
    [Header("通常時のキー背景色")]
    [SerializeField] private Color normalButtonColor = Color.white;
    [Header("選択中のキー背景色")]
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.9f, 0.4f);

    [Space(15)]
    [Header("リズム左キーの背景画像")]
    [SerializeField] private Image rhythmLeftButtonImage;
    [Header("リズム右キーの背景画像")]
    [SerializeField] private Image rhythmRightButtonImage;
    [Header("インベーダー左キーの背景画像")]
    [SerializeField] private Image invaderLeftButtonImage;
    [Header("インベーダー右キーの背景画像")]
    [SerializeField] private Image invaderRightButtonImage;

    private KeyConfigTarget selectedTarget = KeyConfigTarget.None;

    public bool WasCancelInputConsumedThisFrame { get; private set; }

    private void Start()
    {
        RefreshKeyTexts();
        ClearSelection();
    }

    private void Update()
    {
        WasCancelInputConsumedThisFrame = false;

        if (selectedTarget == KeyConfigTarget.None || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            WasCancelInputConsumedThisFrame = true;
            ClearSelection();
            return;
        }

        TryApplyPressedKey();
    }

    /// <summary>
    /// リズム左キーを変更する入力待ち状態にする。
    /// </summary>
    public void SelectRhythmLeft()
    {
        SelectTarget(KeyConfigTarget.RhythmLeft);
    }

    /// <summary>
    /// リズム右キーを変更する入力待ち状態にする。
    /// </summary>
    public void SelectRhythmRight()
    {
        SelectTarget(KeyConfigTarget.RhythmRight);
    }

    /// <summary>
    /// インベーダー左キーを変更する入力待ち状態にする。
    /// </summary>
    public void SelectInvaderLeft()
    {
        SelectTarget(KeyConfigTarget.InvaderLeft);
    }

    /// <summary>
    /// インベーダー右キーを変更する入力待ち状態にする。
    /// </summary>
    public void SelectInvaderRight()
    {
        SelectTarget(KeyConfigTarget.InvaderRight);
    }

    /// <summary>
    /// キー割り当ての入力待ち状態を解除する。
    /// </summary>
    public void CancelKeySelection()
    {
        ClearSelection();
    }

    /// <summary>
    /// 現在のキー割り当てを画面表示に反映する。
    /// </summary>
    public void RefreshKeyTexts()
    {
        if (bindingSettings == null)
        {
            Debug.LogWarning("GameInputBindingSettings が設定されていません。");
            return;
        }

        rhythmLeftKeyText.text = bindingSettings.GetBindingDisplayName(
            GameInputBindingSettings.RhythmLeftActionName,
            string.Empty);
        rhythmRightKeyText.text = bindingSettings.GetBindingDisplayName(
            GameInputBindingSettings.RhythmRightActionName,
            string.Empty);
        invaderLeftKeyText.text = bindingSettings.GetBindingDisplayName(
            GameInputBindingSettings.InvaderMoveActionName,
            GameInputBindingSettings.InvaderMoveLeftBindingName);
        invaderRightKeyText.text = bindingSettings.GetBindingDisplayName(
            GameInputBindingSettings.InvaderMoveActionName,
            GameInputBindingSettings.InvaderMoveRightBindingName);
    }

    /// <summary>
    /// 指定した操作を入力待ち状態にする。
    /// </summary>
    /// <param name="target">入力待ちにする操作</param>
    private void SelectTarget(KeyConfigTarget target)
    {
        selectedTarget = target;
        RefreshHighlights();
    }

    /// <summary>
    /// 入力待ち状態を解除する。
    /// </summary>
    private void ClearSelection()
    {
        selectedTarget = KeyConfigTarget.None;
        RefreshHighlights();
    }

    /// <summary>
    /// 押されたキーがあれば、現在選択中の操作へ割り当てる。
    /// </summary>
    private void TryApplyPressedKey()
    {
        foreach (KeyControl keyControl in Keyboard.current.allKeys)
        {
            if (!keyControl.wasPressedThisFrame)
            {
                continue;
            }

            ApplyKeyToSelectedTarget(keyControl.keyCode);
            return;
        }
    }

    /// <summary>
    /// 現在選択中の操作へキーを割り当てる。
    /// </summary>
    /// <param name="key">割り当てるキー</param>
    private void ApplyKeyToSelectedTarget(Key key)
    {
        if (bindingSettings == null)
        {
            Debug.LogWarning("GameInputBindingSettings が設定されていません。");
            ClearSelection();
            return;
        }

        GetActionAndBindingName(selectedTarget, out string actionName,
            out string bindingName);

        bool isSucceeded = bindingSettings.TrySetKeyboardBinding(
            actionName,
            bindingName,
            key,
            out string errorMessage);

        if (!isSucceeded)
        {
            Debug.LogWarning(errorMessage);
            ClearSelection();
            return;
        }

        RefreshKeyTexts();
        ClearSelection();
    }

    /// <summary>
    /// 選択中の操作に対応する Action 名と Binding 名を取得する。
    /// </summary>
    /// <param name="target">対象の操作</param>
    /// <param name="actionName">Action 名</param>
    /// <param name="bindingName">Binding 名</param>
    private void GetActionAndBindingName(
        KeyConfigTarget target,
        out string actionName,
        out string bindingName)
    {
        actionName = string.Empty;
        bindingName = string.Empty;

        switch (target)
        {
            case KeyConfigTarget.RhythmLeft:
                actionName = GameInputBindingSettings.RhythmLeftActionName;
                break;
            case KeyConfigTarget.RhythmRight:
                actionName = GameInputBindingSettings.RhythmRightActionName;
                break;
            case KeyConfigTarget.InvaderLeft:
                actionName = GameInputBindingSettings.InvaderMoveActionName;
                bindingName = GameInputBindingSettings.InvaderMoveLeftBindingName;
                break;
            case KeyConfigTarget.InvaderRight:
                actionName = GameInputBindingSettings.InvaderMoveActionName;
                bindingName = GameInputBindingSettings.InvaderMoveRightBindingName;
                break;
        }
    }

    /// <summary>
    /// 選択中の操作に応じてハイライト表示を更新する。
    /// </summary>
    private void RefreshHighlights()
    {
        SetButtonColor(
            rhythmLeftButtonImage,
            selectedTarget == KeyConfigTarget.RhythmLeft);
        SetButtonColor(
            rhythmRightButtonImage,
            selectedTarget == KeyConfigTarget.RhythmRight);
        SetButtonColor(
            invaderLeftButtonImage,
            selectedTarget == KeyConfigTarget.InvaderLeft);
        SetButtonColor(
            invaderRightButtonImage,
            selectedTarget == KeyConfigTarget.InvaderRight);
    }

    /// <summary>
    /// Image が設定されている場合のみ背景色を変更する。
    /// </summary>
    /// <param name="buttonImage">色を変更する Image</param>
    /// <param name="isSelected">選択中の場合は true</param>
    private void SetButtonColor(Image buttonImage, bool isSelected)
    {
        if (buttonImage == null)
        {
            return;
        }

        buttonImage.color = isSelected ? selectedButtonColor : normalButtonColor;
    }
}
