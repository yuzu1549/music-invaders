using UnityEditor;
using UnityEngine;

public static class CopyHierarchyPath
{
    [MenuItem("GameObject/階層パスをコピー", false, 49)]
    private static void CopyPath(MenuCommand command)
    {
        // 右クリックしたGameObjectを取得
        GameObject target =
            command.context as GameObject ??
            Selection.activeGameObject;

        if (target == null)
        {
            return;
        }

        string path = GetHierarchyPath(target.transform);

        // クリップボードへコピー
        EditorGUIUtility.systemCopyBuffer = path;

        Debug.Log($"階層パスをコピーしました: {path}", target);
    }

    [MenuItem("GameObject/階層パスをコピー", true)]
    private static bool ValidateCopyPath()
    {
        return Selection.activeGameObject != null;
    }

    private static string GetHierarchyPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = $"{target.name}/{path}";
        }

        return path;
    }
}
