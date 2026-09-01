using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ChartBeatShifterWindow : EditorWindow
{
    private const int PreviewLineCount = 20;

    private static readonly Regex ChartLinePattern = new Regex(
        @"^(?<leading>\s*)(?<beat>-?\d+(?:\.\d+)?)" +
        @"(?<separator>\s*,\s*)(?<lane>-?\d+)(?<trailing>\s*)$",
        RegexOptions.Compiled
    );

    private TextAsset chartFile;
    private TextAsset previousChartFile;
    private Vector2 previewScrollPosition;
    private string shiftedChartText;
    private string previewText;
    private string validationMessage;
    private string beatShiftText = "0";
    private string selectedShiftText;

    [MenuItem("Tools/Music Game/Chart Beat Shifter")]
    private static void OpenWindow()
    {
        GetWindow<ChartBeatShifterWindow>("Chart Beat Shifter");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("譜面の拍を全体移動", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        chartFile = (TextAsset)EditorGUILayout.ObjectField(
            "譜面ファイル",
            chartFile,
            typeof(TextAsset),
            false
        );

        if (chartFile != previousChartFile)
        {
            previousChartFile = chartFile;
            ResetShift();
        }

        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        string updatedBeatShiftText = EditorGUILayout.TextField(
            "移動量（拍）",
            beatShiftText
        );
        if (EditorGUI.EndChangeCheck())
        {
            beatShiftText = updatedBeatShiftText;
            InvalidatePreview();
        }

        using (new EditorGUI.DisabledScope(chartFile == null))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-1拍"))
            {
                AddBeatShift(-1m);
            }

            if (GUILayout.Button("+1拍"))
            {
                AddBeatShift(1m);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("プレビュー"))
            {
                BuildPreviewFromInput();
            }
        }

        DrawValidationMessage();
        DrawPreview();

        using (new EditorGUI.DisabledScope(
            string.IsNullOrEmpty(shiftedChartText)))
        {
            if (GUILayout.Button("確認して保存"))
            {
                SaveShiftedChart();
            }
        }
    }

    /// <summary>
    /// 現在の移動量へ指定拍数を加算する。
    /// </summary>
    /// <param name="beatShiftDelta">追加する拍数</param>
    private void AddBeatShift(decimal beatShiftDelta)
    {
        if (!TryParseBeatShift(
            beatShiftText,
            out decimal currentShift,
            out int decimalPlaceCount))
        {
            InvalidatePreview();
            validationMessage = "移動量を数値で入力してください。";
            return;
        }

        decimal updatedShift = currentShift + beatShiftDelta;
        if (updatedShift == 0)
        {
            beatShiftText = "0";
            InvalidatePreview();
            return;
        }

        beatShiftText = FormatBeat(updatedShift, decimalPlaceCount);
        BuildPreview(updatedShift, decimalPlaceCount);
    }

    /// <summary>
    /// 手入力された移動量から変換プレビューを作成する。
    /// </summary>
    private void BuildPreviewFromInput()
    {
        if (!TryParseBeatShift(
            beatShiftText,
            out decimal beatShift,
            out int decimalPlaceCount))
        {
            InvalidatePreview();
            validationMessage = "移動量を数値で入力してください。";
            return;
        }

        if (beatShift == 0m)
        {
            InvalidatePreview();
            validationMessage = "0以外の移動量を入力してください。";
            return;
        }

        BuildPreview(beatShift, decimalPlaceCount);
    }

    /// <summary>
    /// 選択された移動量で変換結果とプレビューを作成する。
    /// </summary>
    /// <param name="beatShift">全体へ加算する拍数</param>
    /// <param name="shiftDecimalPlaceCount">移動量の小数桁数</param>
    private void BuildPreview(
        decimal beatShift,
        int shiftDecimalPlaceCount)
    {
        InvalidatePreview();
        selectedShiftText = GetSignedBeatText(
            beatShift,
            shiftDecimalPlaceCount
        );

        string assetPath = AssetDatabase.GetAssetPath(chartFile);
        if (string.IsNullOrEmpty(assetPath) ||
            !assetPath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            validationMessage = "Assets内のtxt譜面を選択してください。";
            return;
        }

        string sourceText = File.ReadAllText(assetPath, Encoding.UTF8);
        if (!TryShiftChart(
            sourceText,
            beatShift,
            shiftDecimalPlaceCount,
            out string resultText,
            out string resultPreview,
            out string errorMessage))
        {
            validationMessage = errorMessage;
            previewText = resultPreview;
            return;
        }

        shiftedChartText = resultText;
        previewText = resultPreview;
    }

    /// <summary>
    /// 譜面の各拍へ指定値を加算する。
    /// </summary>
    /// <param name="sourceText">変換前の譜面テキスト</param>
    /// <param name="beatShift">全体へ加算する拍数</param>
    /// <param name="shiftDecimalPlaceCount">移動量の小数桁数</param>
    /// <param name="resultText">変換後の譜面テキスト</param>
    /// <param name="resultPreview">変換内容のプレビュー</param>
    /// <param name="errorMessage">変換できない理由</param>
    /// <returns>すべての行を変換できた場合はtrue</returns>
    private static bool TryShiftChart(
        string sourceText,
        decimal beatShift,
        int shiftDecimalPlaceCount,
        out string resultText,
        out string resultPreview,
        out string errorMessage)
    {
        string[] textParts = Regex.Split(sourceText, "(\r\n|\n|\r)");
        StringBuilder resultBuilder = new StringBuilder(sourceText.Length);
        StringBuilder previewBuilder = new StringBuilder();
        int chartLineNumber = 0;
        int previewedLineCount = 0;

        for (int partIndex = 0; partIndex < textParts.Length; partIndex++)
        {
            string textPart = textParts[partIndex];
            if (IsLineBreak(textPart))
            {
                resultBuilder.Append(textPart);
                continue;
            }

            chartLineNumber++;
            if (string.IsNullOrWhiteSpace(textPart))
            {
                resultBuilder.Append(textPart);
                continue;
            }

            Match lineMatch = ChartLinePattern.Match(textPart);
            if (!lineMatch.Success)
            {
                resultText = null;
                resultPreview = previewBuilder.ToString();
                errorMessage =
                    $"{chartLineNumber}行目の形式が正しくありません。";
                return false;
            }

            string beatText = lineMatch.Groups["beat"].Value;
            if (!decimal.TryParse(
                beatText,
                NumberStyles.AllowLeadingSign |
                    NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out decimal beat))
            {
                resultText = null;
                resultPreview = previewBuilder.ToString();
                errorMessage =
                    $"{chartLineNumber}行目の拍を読み取れません。";
                return false;
            }

            decimal shiftedBeat = beat + beatShift;
            if (shiftedBeat < 0m)
            {
                resultText = null;
                resultPreview = previewBuilder.ToString();
                errorMessage =
                    $"{chartLineNumber}行目が0拍未満になるため保存できません。";
                return false;
            }

            string shiftedBeatText = FormatBeat(
                shiftedBeat,
                Mathf.Max(
                    GetDecimalPlaceCount(beatText),
                    shiftDecimalPlaceCount
                )
            );
            string shiftedLine =
                lineMatch.Groups["leading"].Value +
                shiftedBeatText +
                lineMatch.Groups["separator"].Value +
                lineMatch.Groups["lane"].Value +
                lineMatch.Groups["trailing"].Value;

            resultBuilder.Append(shiftedLine);

            if (previewedLineCount < PreviewLineCount)
            {
                previewBuilder.AppendLine(
                    $"{chartLineNumber}: {textPart}  ->  {shiftedLine}"
                );
                previewedLineCount++;
            }
        }

        if (PreviewLineCount < chartLineNumber)
        {
            previewBuilder.AppendLine("...");
        }

        resultText = resultBuilder.ToString();
        resultPreview = previewBuilder.ToString();
        errorMessage = null;
        return true;
    }

    /// <summary>
    /// 元の小数桁数を維持して拍を文字列へ変換する。
    /// </summary>
    /// <param name="beat">変換する拍</param>
    /// <param name="decimalPlaceCount">維持する小数桁数</param>
    /// <returns>譜面へ書き込む拍文字列</returns>
    private static string FormatBeat(decimal beat, int decimalPlaceCount)
    {
        string format = decimalPlaceCount == 0
            ? "0"
            : $"F{decimalPlaceCount}";
        return beat.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 拍文字列の小数桁数を返す。
    /// </summary>
    /// <param name="beatText">譜面に記載された拍文字列</param>
    /// <returns>小数点以下の桁数</returns>
    private static int GetDecimalPlaceCount(string beatText)
    {
        int decimalPointIndex = beatText.IndexOf('.');
        return decimalPointIndex < 0
            ? 0
            : beatText.Length - decimalPointIndex - 1;
    }

    /// <summary>
    /// 手入力された移動量を小数として読み取る。
    /// </summary>
    /// <param name="shiftText">入力された移動量</param>
    /// <param name="beatShift">読み取った移動量</param>
    /// <param name="decimalPlaceCount">入力値の小数桁数</param>
    /// <returns>移動量を読み取れた場合はtrue</returns>
    private static bool TryParseBeatShift(
        string shiftText,
        out decimal beatShift,
        out int decimalPlaceCount)
    {
        string trimmedShiftText = shiftText.Trim();
        bool couldParse = decimal.TryParse(
            trimmedShiftText,
            NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out beatShift
        );
        decimalPlaceCount = couldParse
            ? GetDecimalPlaceCount(trimmedShiftText)
            : 0;
        return couldParse;
    }

    /// <summary>
    /// 移動方向が分かる符号付きの拍文字列を返す。
    /// </summary>
    /// <param name="beatShift">表示する移動量</param>
    /// <param name="decimalPlaceCount">表示する小数桁数</param>
    /// <returns>符号付きの移動量</returns>
    private static string GetSignedBeatText(
        decimal beatShift,
        int decimalPlaceCount)
    {
        string sign = beatShift > 0m ? "+" : string.Empty;
        return sign + FormatBeat(beatShift, decimalPlaceCount);
    }

    /// <summary>
    /// 文字列が改行コードか確認する。
    /// </summary>
    /// <param name="text">確認する文字列</param>
    /// <returns>改行コードの場合はtrue</returns>
    private static bool IsLineBreak(string text)
    {
        return text == "\r\n" || text == "\n" || text == "\r";
    }

    /// <summary>
    /// 変換結果を確認後、選択中の譜面へ保存する。
    /// </summary>
    private void SaveShiftedChart()
    {
        string assetPath = AssetDatabase.GetAssetPath(chartFile);
        bool shouldSave = EditorUtility.DisplayDialog(
            "譜面の拍を変更",
            $"{chartFile.name} の全拍を " +
                $"{selectedShiftText}拍移動します。保存しますか？",
            "保存",
            "キャンセル"
        );
        if (!shouldSave)
        {
            return;
        }

        File.WriteAllText(
            assetPath,
            shiftedChartText,
            new UTF8Encoding(false)
        );
        AssetDatabase.ImportAsset(
            assetPath,
            ImportAssetOptions.ForceUpdate
        );

        ResetShift();
        validationMessage = "保存しました。";
    }

    /// <summary>
    /// 変換時のエラーまたは保存結果を表示する。
    /// </summary>
    private void DrawValidationMessage()
    {
        if (string.IsNullOrEmpty(validationMessage))
        {
            return;
        }

        MessageType messageType = validationMessage == "保存しました。"
            ? MessageType.Info
            : MessageType.Error;
        EditorGUILayout.HelpBox(validationMessage, messageType);
    }

    /// <summary>
    /// 変換前後の先頭行をスクロール表示する。
    /// </summary>
    private void DrawPreview()
    {
        if (string.IsNullOrEmpty(previewText))
        {
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("変更プレビュー", EditorStyles.boldLabel);
        previewScrollPosition = EditorGUILayout.BeginScrollView(
            previewScrollPosition,
            GUILayout.MinHeight(200f)
        );
        EditorGUILayout.TextArea(
            previewText,
            GUILayout.ExpandHeight(true)
        );
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 現在の変換結果を破棄する。
    /// </summary>
    private void InvalidatePreview()
    {
        shiftedChartText = null;
        previewText = null;
        validationMessage = null;
        selectedShiftText = null;
        previewScrollPosition = Vector2.zero;
    }

    /// <summary>
    /// 入力値と現在の変換結果を初期状態へ戻す。
    /// </summary>
    private void ResetShift()
    {
        beatShiftText = "0";
        InvalidatePreview();
    }
}
