using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomEditor(typeof(RegionManager))]
    public class RegionManagerEditor : UnityEditor.Editor
    {
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Level Streaming Editor", GUILayout.Height(28f)))
                LevelStreamingEditorWindow.Open((RegionManager)target);

            EditorGUILayout.Space(4f);
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            var manager = (RegionManager)target;
            if (Application.isPlaying)
                DrawRuntimeState(manager);

            var issues = RegionManagerValidator.Validate(manager);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Streaming configuration is valid.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            foreach (var issue in issues)
                EditorGUILayout.HelpBox(issue.Message, issue.Severity);
        }

        private static void DrawRuntimeState(RegionManager manager)
        {
            var controller = manager.GetComponent<RegionStreamingController>();
            if (controller == null)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Runtime Streaming State", EditorStyles.boldLabel);
            foreach (var region in controller.GetDebugSnapshot())
            {
                string desire = region.DesireReason == RegionStreamingController.RegionDesireReason.None
                    ? "undesired"
                    : region.DesireReason.ToString();
                string registry = string.IsNullOrWhiteSpace(region.RegistryKey)
                    ? "no registry ref"
                    : $"refs={region.RegistryReferenceCount}";
                EditorGUILayout.LabelField(
                    region.Name,
                    $"{region.State} | {desire} | active={region.IsActive} | {registry}");

                if (!string.IsNullOrWhiteSpace(region.LastError))
                {
                    EditorGUILayout.HelpBox(
                        $"{region.Name}: {region.LastError} (failures: {region.ConsecutiveFailures})",
                        MessageType.Error);
                }
            }

            if (controller.IsShuttingDown)
                EditorGUILayout.HelpBox("Streaming controller is shutting down.", MessageType.Warning);
        }

    }
}
