using UnityEditor;
using UnityEditor.UI;

/// <summary>標準Graphic設定に加えて、警告枠固有の調整項目を表示する。</summary>
[CustomEditor(typeof(ScreenEdgeWarningGraphic))]
[CanEditMultipleObjects]
public class ScreenEdgeWarningGraphicEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("borderWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bandCount"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outerOpacity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("innerOpacity"));
        serializedObject.ApplyModifiedProperties();
    }
}
