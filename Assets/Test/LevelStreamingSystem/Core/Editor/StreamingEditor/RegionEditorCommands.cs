using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LevelStreaming.Editor
{
    internal static class RegionEditorCommands
    {
        public static int AddRegion(RegionManager manager, RegionManager.RegionType type, Vector3 center)
        {
            if (manager == null)
                return -1;

            Undo.RecordObject(manager, "Add Streaming Region");
            var serializedManager = new SerializedObject(manager);
            serializedManager.Update();
            SerializedProperty regions = serializedManager.FindProperty("regions");
            int index = regions.arraySize;
            regions.arraySize++;

            SerializedProperty region = regions.GetArrayElementAtIndex(index);
            InitializeRegion(region, type, GenerateUniqueName(manager, type.ToString()), center);
            serializedManager.ApplyModifiedProperties();
            MarkDirty(manager);
            return index;
        }

        public static int DuplicateRegion(RegionManager manager, int index)
        {
            if (manager == null || index < 0 || index >= manager.Regions.Count)
                return -1;

            Undo.RecordObject(manager, "Duplicate Streaming Region");
            var serializedManager = new SerializedObject(manager);
            serializedManager.Update();
            SerializedProperty regions = serializedManager.FindProperty("regions");
            SerializedProperty source = regions.GetArrayElementAtIndex(index);
            if (!source.DuplicateCommand())
                return -1;

            int duplicateIndex = Mathf.Min(index + 1, regions.arraySize - 1);
            SerializedProperty duplicate = regions.GetArrayElementAtIndex(duplicateIndex);
            SerializedProperty name = duplicate.FindPropertyRelative("regionName");
            name.stringValue = GenerateUniqueName(manager,
                string.IsNullOrWhiteSpace(name.stringValue) ? "Region Copy" : $"{name.stringValue} Copy");

            Bounds bounds = duplicate.FindPropertyRelative("cachedBounds").boundsValue;
            bounds.center += Vector3.right * Mathf.Max(1f, bounds.extents.x * 0.25f);
            duplicate.FindPropertyRelative("cachedBounds").boundsValue = bounds;
            serializedManager.ApplyModifiedProperties();
            MarkDirty(manager);
            return duplicateIndex;
        }

        public static bool DeleteRegion(RegionManager manager, int index)
        {
            if (manager == null || index < 0 || index >= manager.Regions.Count)
                return false;

            string name = manager.Regions[index]?.RegionName ?? $"Region {index}";
            if (!EditorUtility.DisplayDialog(
                    "Delete Streaming Region",
                    $"Delete '{name}' from this RegionManager? The source scene or prefab is not deleted.",
                    "Delete",
                    "Cancel"))
                return false;

            Undo.RecordObject(manager, "Delete Streaming Region");
            var serializedManager = new SerializedObject(manager);
            serializedManager.Update();
            serializedManager.FindProperty("regions").DeleteArrayElementAtIndex(index);
            serializedManager.ApplyModifiedProperties();
            MarkDirty(manager);
            return true;
        }

        public static int MoveRegion(RegionManager manager, int index, int delta)
        {
            if (manager == null || index < 0 || index >= manager.Regions.Count)
                return index;

            int destination = Mathf.Clamp(index + delta, 0, manager.Regions.Count - 1);
            if (destination == index)
                return index;

            Undo.RecordObject(manager, "Reorder Streaming Region");
            var serializedManager = new SerializedObject(manager);
            serializedManager.Update();
            serializedManager.FindProperty("regions").MoveArrayElement(index, destination);
            serializedManager.ApplyModifiedProperties();
            MarkDirty(manager);
            return destination;
        }

        public static int AddPortal(RegionManager manager, int sourceRegionIndex, string targetName = null,
            Bounds? localBounds = null)
        {
            if (!TryGetRegionProperty(manager, sourceRegionIndex, out SerializedObject serializedManager,
                    out SerializedProperty region))
                return -1;

            Undo.RecordObject(manager, "Add Streaming Portal");
            SerializedProperty portals = region.FindPropertyRelative("portals");
            int portalIndex = portals.arraySize;
            portals.arraySize++;
            InitializePortal(
                portals.GetArrayElementAtIndex(portalIndex),
                targetName ?? string.Empty,
                localBounds ?? new Bounds(Vector3.zero, new Vector3(4f, 4f, 1f)));
            serializedManager.ApplyModifiedProperties();
            manager.Regions[sourceRegionIndex]?.RebuildPortalWorldBounds();
            MarkDirty(manager);
            return portalIndex;
        }

        public static bool DeletePortal(RegionManager manager, int sourceRegionIndex, int portalIndex)
        {
            if (!TryGetRegionProperty(manager, sourceRegionIndex, out SerializedObject serializedManager,
                    out SerializedProperty region))
                return false;

            SerializedProperty portals = region.FindPropertyRelative("portals");
            if (portalIndex < 0 || portalIndex >= portals.arraySize)
                return false;

            Undo.RecordObject(manager, "Delete Streaming Portal");
            portals.DeleteArrayElementAtIndex(portalIndex);
            serializedManager.ApplyModifiedProperties();
            manager.Regions[sourceRegionIndex]?.RebuildPortalWorldBounds();
            MarkDirty(manager);
            return true;
        }

        public static int CreateReciprocalPortal(RegionManager manager, int sourceRegionIndex, int portalIndex)
        {
            if (manager == null || sourceRegionIndex < 0 || sourceRegionIndex >= manager.Regions.Count)
                return -1;

            RegionManager.Region source = manager.Regions[sourceRegionIndex];
            if (source?.Portals == null || portalIndex < 0 || portalIndex >= source.Portals.Count)
                return -1;

            RegionManager.Portal portal = source.Portals[portalIndex];
            if (portal == null || string.IsNullOrWhiteSpace(portal.TargetRegionName))
                return -1;

            int targetIndex = FindRegionIndex(manager, portal.TargetRegionName);
            if (targetIndex < 0 || targetIndex == sourceRegionIndex)
                return -1;

            RegionManager.Region target = manager.Regions[targetIndex];
            foreach (RegionManager.Portal existing in target.Portals)
            {
                if (existing != null && string.Equals(existing.TargetRegionName, source.RegionName,
                        StringComparison.Ordinal))
                    return target.Portals.IndexOf(existing);
            }

            Vector3 worldCenter = source.CachedBounds.center + portal.LocalBounds.center;
            Bounds reciprocalBounds = new(
                worldCenter - target.CachedBounds.center,
                portal.LocalBounds.size);
            return AddPortal(manager, targetIndex, source.RegionName, reciprocalBounds);
        }

        public static int FindRegionIndex(RegionManager manager, string regionName)
        {
            if (manager == null || string.IsNullOrWhiteSpace(regionName))
                return -1;

            for (int i = 0; i < manager.Regions.Count; i++)
            {
                if (manager.Regions[i] != null && string.Equals(
                        manager.Regions[i].RegionName,
                        regionName,
                        StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        public static SerializedProperty FindPortalTargetProperty(SerializedProperty portal)
        {
            return portal?.FindPropertyRelative("<TargetRegionName>k__BackingField") ??
                   portal?.FindPropertyRelative("targetRegionName");
        }

        public static SerializedProperty FindPortalBoundsProperty(SerializedProperty portal)
        {
            return portal?.FindPropertyRelative("<LocalBounds>k__BackingField") ??
                   portal?.FindPropertyRelative("localBounds");
        }

        public static void MarkDirty(RegionManager manager)
        {
            if (manager == null)
                return;
            EditorUtility.SetDirty(manager);
            if (manager.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        }

        private static bool TryGetRegionProperty(RegionManager manager, int regionIndex,
            out SerializedObject serializedManager, out SerializedProperty region)
        {
            serializedManager = null;
            region = null;
            if (manager == null || regionIndex < 0 || regionIndex >= manager.Regions.Count)
                return false;

            serializedManager = new SerializedObject(manager);
            serializedManager.Update();
            region = serializedManager.FindProperty("regions").GetArrayElementAtIndex(regionIndex);
            return region != null;
        }

        private static void InitializeRegion(SerializedProperty region, RegionManager.RegionType type,
            string regionName, Vector3 center)
        {
            region.FindPropertyRelative("type").enumValueIndex = (int)type;
            region.FindPropertyRelative("regionName").stringValue = regionName;
            region.FindPropertyRelative("cachedBounds").boundsValue =
                new Bounds(center, new Vector3(100f, 50f, 100f));
            region.FindPropertyRelative("portals").arraySize = 0;
            region.FindPropertyRelative("unloadStrategy").objectReferenceValue = FindDefaultUnloadStrategy();

            SerializedProperty sceneRef = region.FindPropertyRelative("sceneRef");
            SetString(sceneRef, "scenePath", string.Empty);
            SerializedProperty sceneAsset = sceneRef?.FindPropertyRelative("sceneAsset");
            if (sceneAsset != null)
                sceneAsset.objectReferenceValue = null;

            ClearAssetReference(region.FindPropertyRelative("prefabRef"));
            ClearAssetReference(region.FindPropertyRelative("addressableSceneRef"));
        }

        private static void InitializePortal(SerializedProperty portal, string targetName, Bounds bounds)
        {
            SerializedProperty target = FindPortalTargetProperty(portal);
            SerializedProperty localBounds = FindPortalBoundsProperty(portal);
            if (target != null)
                target.stringValue = targetName;
            if (localBounds != null)
                localBounds.boundsValue = bounds;
        }

        private static string GenerateUniqueName(RegionManager manager, string baseName)
        {
            string root = string.IsNullOrWhiteSpace(baseName) ? "Region" : baseName.Trim();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (RegionManager.Region region in manager.Regions)
            {
                if (region != null && !string.IsNullOrWhiteSpace(region.RegionName))
                    names.Add(region.RegionName);
            }

            if (!names.Contains(root))
                return root;

            int suffix = 2;
            while (names.Contains($"{root}_{suffix}"))
                suffix++;
            return $"{root}_{suffix}";
        }

        private static UnloadStrategy FindDefaultUnloadStrategy()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:BoundsIntersectionUnloadStrategy"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var strategy = AssetDatabase.LoadAssetAtPath<BoundsIntersectionUnloadStrategy>(path);
                if (strategy != null)
                    return strategy;
            }

            return null;
        }

        private static void ClearAssetReference(SerializedProperty property)
        {
            if (property == null)
                return;
            SetString(property, "m_AssetGUID", string.Empty);
            SetString(property, "m_SubObjectName", string.Empty);
            SetString(property, "m_SubObjectType", string.Empty);
            SetString(property, "m_SubObjectGUID", string.Empty);
        }

        private static void SetString(SerializedProperty parent, string childName, string value)
        {
            SerializedProperty child = parent?.FindPropertyRelative(childName);
            if (child != null)
                child.stringValue = value;
        }
    }
}
