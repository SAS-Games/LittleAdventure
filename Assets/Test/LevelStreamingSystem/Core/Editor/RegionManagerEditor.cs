using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomEditor(typeof(RegionManager))]
    public class RegionManagerEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle _regionHandle = new();
        private readonly BoxBoundsHandle _portalHandle = new();
        private int _selectedRegionIndex;
        private bool _editSelectedRegion;

        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            var manager = (RegionManager)target;
            DrawSceneAuthoringControls(manager);
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

        private void OnSceneGUI()
        {
            var manager = (RegionManager)target;
            if (!_editSelectedRegion || manager?.Regions == null || manager.Regions.Count == 0)
                return;

            _selectedRegionIndex = Mathf.Clamp(_selectedRegionIndex, 0, manager.Regions.Count - 1);
            RegionManager.Region region = manager.Regions[_selectedRegionIndex];
            if (region == null)
                return;

            DrawRegionBounds(manager, region);
            DrawRegionPortals(manager, region);
        }

        private void DrawSceneAuthoringControls(RegionManager manager)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Scene Authoring", EditorStyles.boldLabel);

            if (manager.Regions.Count == 0)
            {
                EditorGUILayout.HelpBox("Add a region to enable Scene view handles.", MessageType.Info);
                _editSelectedRegion = false;
                return;
            }

            string[] labels = new string[manager.Regions.Count];
            for (int i = 0; i < labels.Length; i++)
            {
                RegionManager.Region region = manager.Regions[i];
                labels[i] = region == null
                    ? $"{i}: <null>"
                    : $"{i}: {(string.IsNullOrWhiteSpace(region.RegionName) ? region.Type.ToString() : region.RegionName)}";
            }

            _selectedRegionIndex = Mathf.Clamp(_selectedRegionIndex, 0, labels.Length - 1);
            _selectedRegionIndex = EditorGUILayout.Popup("Selected Region", _selectedRegionIndex, labels);
            _editSelectedRegion = EditorGUILayout.Toggle(
                new GUIContent("Edit Handles", "Only the selected region and its portals receive interactive handles."),
                _editSelectedRegion);
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

        private void DrawRegionBounds(RegionManager manager, RegionManager.Region region)
        {
            Bounds bounds = region.CachedBounds;
            _regionHandle.center = bounds.center;
            _regionHandle.size = bounds.size;

            EditorGUI.BeginChangeCheck();
            _regionHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(manager, "Modify Region Bound");
                region.CachedBounds = new Bounds(_regionHandle.center, _regionHandle.size);
                region.RebuildPortalWorldBounds();
                EditorUtility.SetDirty(manager);
                bounds = region.CachedBounds;
            }

            string label = string.IsNullOrWhiteSpace(region.RegionName)
                ? region.Type.ToString()
                : region.RegionName;
            Handles.Label(bounds.center + Vector3.up * bounds.extents.y, label, EditorStyles.boldLabel);
        }

        private void DrawRegionPortals(RegionManager manager, RegionManager.Region region)
        {
            if (region.Portals == null)
                return;

            for (int i = 0; i < region.Portals.Count; i++)
            {
                RegionManager.Portal portal = region.Portals[i];
                if (portal == null)
                    continue;

                Vector3 worldCenter = region.CachedBounds.center + portal.LocalBounds.center;
                _portalHandle.center = worldCenter;
                _portalHandle.size = portal.LocalBounds.size;

                EditorGUI.BeginChangeCheck();
                _portalHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(manager, "Modify Portal Bound");
                    Vector3 localCenter = _portalHandle.center - region.CachedBounds.center;
                    portal.LocalBounds = new Bounds(localCenter, _portalHandle.size);
                    region.RebuildPortalWorldBounds();
                    EditorUtility.SetDirty(manager);
                    worldCenter = region.CachedBounds.center + portal.LocalBounds.center;
                }

                string targetLabel = string.IsNullOrWhiteSpace(portal.TargetRegionName)
                    ? "No target"
                    : portal.TargetRegionName;
                Handles.Label(worldCenter + Vector3.up * portal.LocalBounds.extents.y,
                    $"Portal {i} -> {targetLabel}", EditorStyles.miniBoldLabel);
            }
        }
    }
}
