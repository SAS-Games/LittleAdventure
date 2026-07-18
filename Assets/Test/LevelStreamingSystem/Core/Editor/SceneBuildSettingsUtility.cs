using System;
using System.Collections.Generic;
using UnityEditor;

namespace LevelStreaming.Editor
{
    internal static class SceneBuildSettingsUtility
    {
        public static bool EnsureEnabled(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
                throw new ArgumentException("A scene path is required.", nameof(scenePath));

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (!string.Equals(scenes[i].path, scenePath, StringComparison.Ordinal))
                    continue;

                if (scenes[i].enabled)
                    return false;

                scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = scenes.ToArray();
                return true;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            return true;
        }

        public static bool IsEnabled(string scenePath)
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && string.Equals(scene.path, scenePath, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
