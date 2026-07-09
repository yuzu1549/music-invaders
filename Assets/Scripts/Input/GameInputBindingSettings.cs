using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// ゲーム操作用 Input Actions のキー割り当てを保存、読み込み、検証する。
/// </summary>
public class GameInputBindingSettings : MonoBehaviour
{
    public const string GameplayMapName = "Gameplay";
    public const string RhythmLeftActionName = "RhythmLeft";
    public const string RhythmRightActionName = "RhythmRight";
    public const string InvaderMoveActionName = "InvaderMove";
    public const string InvaderMoveLeftBindingName = "negative";
    public const string InvaderMoveRightBindingName = "positive";

    private const string DefaultPlayerPrefsKey = "GameInputBindingOverrides";

    [Header("キー割り当てを管理する Input Actions")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("PlayerPrefs に保存するときのキー")]
    [SerializeField] private string playerPrefsKey = DefaultPlayerPrefsKey;

    [Header("割り当てを禁止するキー")]
    [SerializeField] private Key[] blockedKeys =
    {
        Key.Escape,
        Key.Enter,
        Key.NumpadEnter,
    };

    private void Awake()
    {
        LoadBindingOverrides();
    }

    /// <summary>
    /// 指定した操作にキーボードのキーを割り当てる。
    /// </summary>
    /// <param name="actionName">Action 名</param>
    /// <param name="bindingName">Composite 内の Binding 名。通常 Binding の場合は空文字</param>
    /// <param name="key">割り当てるキー</param>
    /// <param name="errorMessage">割り当てに失敗した理由</param>
    /// <returns>割り当てできた場合は true</returns>
    public bool TrySetKeyboardBinding(
        string actionName,
        string bindingName,
        Key key,
        out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!TryGetKeyboardPath(key, out string keyPath))
        {
            errorMessage = "キーボード入力を取得できません。";
            return false;
        }

        if (IsBlockedKey(key))
        {
            errorMessage = "このキーは割り当てできません。";
            return false;
        }

        if (IsDuplicateBinding(actionName, bindingName, keyPath))
        {
            errorMessage = "このキーはすでに別の操作に割り当てられています。";
            return false;
        }

        if (!TryFindBindingIndex(actionName, bindingName, out InputAction action,
            out int bindingIndex))
        {
            errorMessage = "対象の操作が見つかりません。";
            return false;
        }

        action.ApplyBindingOverride(bindingIndex, keyPath);
        SaveBindingOverrides();
        return true;
    }

    /// <summary>
    /// 指定した操作の表示用キー名を取得する。
    /// </summary>
    /// <param name="actionName">Action 名</param>
    /// <param name="bindingName">Composite 内の Binding 名。通常 Binding の場合は空文字</param>
    /// <returns>表示用キー名</returns>
    public string GetBindingDisplayName(string actionName, string bindingName)
    {
        if (!TryFindBindingIndex(actionName, bindingName, out InputAction action,
            out int bindingIndex))
        {
            return "-";
        }

        return action.GetBindingDisplayString(bindingIndex);
    }

    /// <summary>
    /// PlayerPrefs からキー割り当ての変更内容を読み込む。
    /// </summary>
    public void LoadBindingOverrides()
    {
        if (inputActions == null || !PlayerPrefs.HasKey(playerPrefsKey))
        {
            return;
        }

        string overridesJson = PlayerPrefs.GetString(playerPrefsKey);
        inputActions.LoadBindingOverridesFromJson(overridesJson);
    }

    /// <summary>
    /// 現在のキー割り当て変更内容を PlayerPrefs に保存する。
    /// </summary>
    public void SaveBindingOverrides()
    {
        if (inputActions == null)
        {
            return;
        }

        string overridesJson = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(playerPrefsKey, overridesJson);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 保存済みのキー割り当て変更を削除し、Input Actions の初期設定へ戻す。
    /// </summary>
    public void ResetBindingOverrides()
    {
        if (inputActions == null)
        {
            return;
        }

        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 指定したキーが割り当て禁止キーかどうかを返す。
    /// </summary>
    /// <param name="key">確認するキー</param>
    /// <returns>禁止キーの場合は true</returns>
    public bool IsBlockedKey(Key key)
    {
        foreach (Key blockedKey in blockedKeys)
        {
            if (blockedKey == key)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 指定した操作と Binding 名から Binding の位置を探す。
    /// </summary>
    /// <param name="actionName">Action 名</param>
    /// <param name="bindingName">Composite 内の Binding 名。通常 Binding の場合は空文字</param>
    /// <param name="action">見つかった Action</param>
    /// <param name="bindingIndex">見つかった Binding の位置</param>
    /// <returns>Binding が見つかった場合は true</returns>
    private bool TryFindBindingIndex(
        string actionName,
        string bindingName,
        out InputAction action,
        out int bindingIndex)
    {
        action = null;
        bindingIndex = -1;

        if (inputActions == null)
        {
            return false;
        }

        action = inputActions.FindAction($"{GameplayMapName}/{actionName}");
        if (action == null)
        {
            return false;
        }

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (binding.isComposite)
            {
                continue;
            }

            if (string.IsNullOrEmpty(bindingName) ||
                string.Equals(binding.name, bindingName, StringComparison.Ordinal))
            {
                bindingIndex = i;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 指定したキーが他の操作に割り当て済みかどうかを返す。
    /// </summary>
    /// <param name="currentActionName">変更対象の Action 名</param>
    /// <param name="currentBindingName">変更対象の Binding 名</param>
    /// <param name="keyPath">割り当てようとしているキーのパス</param>
    /// <returns>重複している場合は true</returns>
    private bool IsDuplicateBinding(
        string currentActionName,
        string currentBindingName,
        string keyPath)
    {
        if (inputActions == null)
        {
            return false;
        }

        string normalizedKeyPath = NormalizeKeyboardPath(keyPath);

        foreach (InputActionMap actionMap in inputActions.actionMaps)
        {
            foreach (InputAction action in actionMap.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];

                    if (binding.isComposite)
                    {
                        continue;
                    }

                    bool isCurrentBinding =
                        string.Equals(action.name, currentActionName,
                            StringComparison.Ordinal) &&
                        (string.IsNullOrEmpty(currentBindingName) ||
                         string.Equals(binding.name, currentBindingName,
                             StringComparison.Ordinal));

                    if (isCurrentBinding)
                    {
                        continue;
                    }

                    string normalizedBindingPath =
                        NormalizeKeyboardPath(binding.effectivePath);

                    if (string.Equals(normalizedBindingPath, normalizedKeyPath,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Input System の Key から Binding Override に使うパスを取得する。
    /// </summary>
    /// <param name="key">取得対象のキー</param>
    /// <param name="keyPath">取得したキーのパス</param>
    /// <returns>取得できた場合は true</returns>
    private bool TryGetKeyboardPath(Key key, out string keyPath)
    {
        keyPath = string.Empty;

        if (Keyboard.current == null)
        {
            return false;
        }

        KeyControl keyControl = Keyboard.current[key];
        if (keyControl == null)
        {
            return false;
        }

        keyPath = keyControl.path;
        return true;
    }

    /// <summary>
    /// 同じキーを表すパスを比較しやすい形へそろえる。
    /// </summary>
    /// <param name="path">Input System のキー入力パス</param>
    /// <returns>比較用に正規化したパス</returns>
    private string NormalizeKeyboardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return path
            .Replace("/Keyboard/", "<Keyboard>/")
            .Replace("<keyboard>/", "<Keyboard>/");
    }
}
