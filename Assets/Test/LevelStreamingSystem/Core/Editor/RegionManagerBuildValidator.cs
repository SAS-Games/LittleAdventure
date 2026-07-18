using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelStreaming.Editor
{
    internal sealed class RegionManagerBuildValidator : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var errors = new List<string>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (RegionManager manager in root.GetComponentsInChildren<RegionManager>(true))
                {
                    foreach (RegionValidationIssue issue in RegionManagerValidator.Validate(manager))
                    {
                        if (issue.Severity == MessageType.Error)
                            errors.Add($"{manager.name}: {issue.Message}");
                    }
                }
            }

            if (errors.Count == 0)
                return;

            throw new BuildFailedException(
                $"Invalid level-streaming configuration in scene '{scene.path}':\n- " +
                string.Join("\n- ", errors));
        }
    }
}
