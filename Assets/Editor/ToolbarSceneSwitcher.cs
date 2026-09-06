#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Proto.Editor
{
    public static class SceneDropdownToolbar
    {
        /// <summary>
        /// シーンを検索するルート
        /// </summary>
        private const string ScenesRoot = "Assets/Scenes";

        private const string ToolbarElementId =
            "Proto/Scene Dropdown";

        /// <summary>
        /// Unity上部のツールバーにボタンを追加
        /// </summary>
        [MainToolbarElement(
            ToolbarElementId,
            defaultDockPosition = MainToolbarDockPosition.Middle
        )]
        private static MainToolbarElement CreateSceneDropdown()
        {
            var icon =
                EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;

            var content = new MainToolbarContent(
                icon,
                "シーンを切り替える"
            );

            return new MainToolbarDropdown(
                content,
                ShowSceneMenu
            );
        }

        /// <summary>
        /// シーン一覧を表示
        /// </summary>
        private static void ShowSceneMenu(Rect dropdownRect)
        {
            string[] scenePaths = LoadScenes();

            var menu = new GenericMenu();

            Scene currentScene =
                SceneManager.GetActiveScene();

            // 現在のシーン
            if (currentScene.IsValid())
            {
                menu.AddDisabledItem(
                    new GUIContent(
                        $"● {currentScene.name} (現在)"
                    )
                );
            }

            menu.AddSeparator("");

            // Main
            AddSceneFolder(
                menu,
                scenePaths,
                currentScene,
                "Main"
            );

            // Prototype
            AddSceneFolder(
                menu,
                scenePaths,
                currentScene,
                "Prototype"
            );

            // Test
            AddSceneFolder(
                menu,
                scenePaths,
                currentScene,
                "Test"
            );

            if (scenePaths.Length == 0)
            {
                menu.AddDisabledItem(
                    new GUIContent(
                        "シーンが見つかりません"
                    )
                );

                menu.AddSeparator("");
            }

            menu.AddItem(
                new GUIContent("Build Profiles..."),
                false,
                OpenBuildProfiles
            );

            menu.DropDown(dropdownRect);
        }

        /// <summary>
        /// フォルダ単位でシーンをメニューに追加
        /// </summary>
        private static void AddSceneFolder(
            GenericMenu menu,
            string[] scenePaths,
            Scene currentScene,
            string folderName)
        {
            string folderPath =
                $"{ScenesRoot}/{folderName}/";

            string[] scenes =
                scenePaths
                    .Where(path =>
                        path.StartsWith(
                            folderPath,
                            StringComparison.Ordinal
                        ))
                    .ToArray();

            if (scenes.Length == 0)
                return;

            menu.AddDisabledItem(
                new GUIContent(
                    $"── {folderName} ──"
                )
            );

            foreach (string scenePath in scenes)
            {
                string path = scenePath;

                // Assets/Scenes/Main/Hoge.unity
                // → Hoge
                string relativePath =
                    path.Substring(folderPath.Length);

                string label =
                    Path.ChangeExtension(
                        relativePath,
                        null
                    )
                    .Replace('\\', '/');

                bool isCurrent =
                    string.Equals(
                        currentScene.path,
                        path,
                        StringComparison.Ordinal
                    );

                if (isCurrent)
                {
                    menu.AddDisabledItem(
                        new GUIContent(
                            $"  {label} ✓"
                        )
                    );
                }
                else
                {
                    menu.AddItem(
                        new GUIContent(
                            $"  {label}"
                        ),
                        false,
                        () => OpenScene(path)
                    );
                }
            }

            menu.AddSeparator("");
        }

        /// <summary>
        /// シーンを開く
        /// </summary>
        private static void OpenScene(string path)
        {
            if (Application.isPlaying)
            {
                OpenSceneInPlayMode(path);
            }
            else
            {
                OpenSceneInEditMode(path);
            }
        }

        /// <summary>
        /// Edit Modeでシーンを開く
        /// </summary>
        private static void OpenSceneInEditMode(
            string path)
        {
            bool saved =
                EditorSceneManager
                    .SaveCurrentModifiedScenesIfUserWantsTo();

            if (!saved)
                return;

            EditorSceneManager.OpenScene(
                path,
                OpenSceneMode.Single
            );
        }

        /// <summary>
        /// Play Modeでシーンを開く
        /// </summary>
        private static void OpenSceneInPlayMode(
            string path)
        {
            bool isInBuild =
                EditorBuildSettings.scenes.Any(
                    scene =>
                        scene.enabled &&
                        string.Equals(
                            scene.path,
                            path,
                            StringComparison.Ordinal
                        )
                );

            if (!isInBuild)
            {
                Debug.LogWarning(
                    $"[SceneDropdownToolbar] " +
                    $"Play中はBuild Profileのシーンリストに登録されているシーンのみ開けます。\n" +
                    $"Scene: {path}"
                );

                return;
            }

            SceneManager.LoadScene(path);
        }

        /// <summary>
        /// Assets/Scenes 以下の全シーンを取得
        /// </summary>
        private static string[] LoadScenes()
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    "t:SceneAsset",
                    new[]
                    {
                        ScenesRoot
                    }
                );

            return guids
                .Select(
                    AssetDatabase.GUIDToAssetPath
                )
                .Where(path =>
                    !string.IsNullOrEmpty(path) &&
                    path.EndsWith(
                        ".unity",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .OrderBy(path => path)
                .ToArray();
        }

        /// <summary>
        /// Build Profilesを開く
        /// </summary>
        private static void OpenBuildProfiles()
        {
            EditorApplication.ExecuteMenuItem(
                "File/Build Profiles"
            );
        }
    }
}

#endif