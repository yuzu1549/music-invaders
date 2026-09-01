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
    private int selectedShift;

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
            ClearPreview();
        }

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(chartFile == null))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-1拍"))
            {
                AddBeatShift(-1);
            }

            if (GUILayout.Button("+1拍"))
            {
                AddBeatShift(1);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField(
            "現在の移動量",
            selectedShift.ToString("+#;-#;0") + "拍"
        );

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
    private void AddBeatShift(int beatShiftDelta)
    {
        int updatedShift = selectedShift + beatShiftDelta;
        if (updatedShift == 0)
        {
            ClearPreview();
            return;
        }

        BuildPreview(updatedShift);
    }

    /// <summary>
    /// 選択された移動量で変換結果とプレビューを作成する。
    /// </summary>
    /// <param name="beatShift">全体へ加算する拍数</param>
    private void BuildPreview(int beatShift)
    {
        shiftedChartText = null;
        previewText = null;
        validationMessage = null;
        previewScrollPosition = Vector2.zero;
        selectedShift = beatShift;

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
    /// <param name="resultText">変換後の譜面テキスト</param>
    /// <param name="resultPreview">変換内容のプレビュー</param>
    /// <param name="errorMessage">変換できない理由</param>
    /// <returns>すべての行を変換できた場合はtrue</returns>
    private static bool TryShiftChart(
        string sourceText,
        int beatShift,
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
                GetDecimalPlaceCount(beatText)
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
                $"{selectedShift:+#;-#;0} します。保存しますか？",
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

        ClearPreview();
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
    private void ClearPreview()
    {
        shiftedChartText = null;
        previewText = null;
        validationMessage = null;
        selectedShift = 0;
        previewScrollPosition = Vector2.zero;
    }
}
