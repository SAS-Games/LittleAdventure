using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelStreaming.Editor
{
    internal readonly struct StreamingEditorIssue
    {
        public StreamingEditorIssue(MessageType severity, string message, Object context = null)
        {
            Severity = severity;
            Message = message;
            Context = context;
        }

        public MessageType Severity { get; }
        public string Message { get; }
        public Object Context { get; }
    }

    internal static class StreamingEditorValidation
    {
        public static List<StreamingEditorIssue> Validate(RegionManager manager)
        {
            var issues = new List<StreamingEditorIssue>();
            if (manager == null)
            {
                issues.Add(new StreamingEditorIssue(MessageType.Error,
                    "Select a RegionManager from a loaded persistent scene."));
                return issues;
            }

            Scene scene = manager.gameObject.scene;
            int managerCount = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                managerCount += root.GetComponentsInChildren<RegionManager>(true).Length;
            if (managerCount > 1)
            {
                issues.Add(new StreamingEditorIssue(MessageType.Error,
                    $"Scene '{scene.name}' contains {managerCount} RegionManagers. Keep exactly one.", manager));
            }

            RegionStreamingController controller = manager.GetComponent<RegionStreamingController>();
            if (controller == null)
            {
                issues.Add(new StreamingEditorIssue(MessageType.Error,
                    "RegionManager has no RegionStreamingController.", manager));
            }
            else
            {
                var serializedController = new SerializedObject(controller);
                if (serializedController.FindProperty("m_StreamingLoader")?.objectReferenceValue == null)
                {
                    issues.Add(new StreamingEditorIssue(MessageType.Error,
                        "RegionStreamingController has no streaming loader.", controller));
                }
            }

            List<MonoBehaviour> providers = FindProviders(scene);
            if (providers.Count == 0)
            {
                issues.Add(new StreamingEditorIssue(MessageType.Error,
                    "The persistent scene has no IStreamingBoundsProvider."));
            }
            else if (providers.Count > 1)
            {
                issues.Add(new StreamingEditorIssue(MessageType.Warning,
                    $"The persistent scene has {providers.Count} bounds providers. Their Awake order decides which one controls streaming."));
            }

            foreach (RegionValidationIssue issue in RegionManagerValidator.Validate(manager))
                issues.Add(new StreamingEditorIssue(issue.Severity, issue.Message, manager));

            return issues;
        }

        public static List<MonoBehaviour> FindProviders(Scene scene)
        {
            var providers = new List<MonoBehaviour>();
            if (!scene.IsValid() || !scene.isLoaded)
                return providers;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour is IStreamingBoundsProvider)
                        providers.Add(behaviour);
                }
            }

            return providers;
        }
    }
}
