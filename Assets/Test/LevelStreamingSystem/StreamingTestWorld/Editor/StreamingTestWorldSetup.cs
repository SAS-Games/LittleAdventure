using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LevelStreaming.TestWorld.Editor
{
    [InitializeOnLoad]
    internal static class StreamingTestWorldSetup
    {
        private const string Root = "Assets/Test/LevelStreamingSystem/StreamingTestWorld";
        private const string PersistentScene = Root + "/StreamingTestPersistent.unity";
        private const int GridSize = 5;

        static StreamingTestWorldSetup()
        {
            EditorApplication.delayCall += EnsureBuildSettingsSilently;
        }

        [MenuItem("Tools/Streaming/Test World/Open Persistent Scene", priority = 200)]
        private static void OpenPersistentScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(PersistentScene, OpenSceneMode.Single);
        }

        [MenuItem("Tools/Streaming/Test World/Install Scenes In Build Settings", priority = 201)]
        private static void InstallBuildSettings()
        {
            int added = EnsureBuildSettings();
            EditorUtility.DisplayDialog(
                "Streaming Test World",
                added == 0
                    ? "All test scenes are already enabled in Build Settings."
                    : $"Added {added} test scene(s) to Build Settings without removing existing entries.",
                "OK");
        }

        [MenuItem("Tools/Streaming/Test World/Select Test Folder", priority = 202)]
        private static void SelectTestFolder()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(Root);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private static void EnsureBuildSettingsSilently()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PersistentScene) == null)
                return;
            int added = EnsureBuildSettings();
            if (added > 0)
                Debug.Log($"[Streaming Test World] Added {added} test scenes to Build Settings.");
        }

        private static int EnsureBuildSettings()
        {
            var paths = new List<string> { PersistentScene };
            for (int z = 0; z < GridSize; z++)
            for (int x = 0; x < GridSize; x++)
                paths.Add($"{Root}/Levels/StreamingLevel_{x:00}_{z:00}.unity");

            var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var existingPaths = new HashSet<string>(
                existing.Select(scene => scene.path),
                StringComparer.OrdinalIgnoreCase);
            int added = 0;
            foreach (string path in paths)
            {
                if (existingPaths.Contains(path) || AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                    continue;
                existing.Add(new EditorBuildSettingsScene(path, true));
                existingPaths.Add(path);
                added++;
            }

            if (added > 0)
                EditorBuildSettings.scenes = existing.ToArray();
            return added;
        }
    }
}
