#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelStreaming.Editor
{
    public static class RegionBoundsAuthoringService
    {
        public static bool ApplyToSource(RegionManager.Region region)
        {
            if (region == null)
                return false;

            switch (region.Type)
            {
                case RegionManager.RegionType.Scene:
                case RegionManager.RegionType.AddressableScene:
                    return TryGetScenePath(region, out string scenePath) &&
                           ApplyToScene(region, scenePath);

                case RegionManager.RegionType.Prefab:
                    return ApplyToPrefab(region);

                default:
                    return false;
            }
        }

        public static bool RefreshFromSource(RegionManager.Region region)
        {
            if (region == null)
                return false;

            switch (region.Type)
            {
                case RegionManager.RegionType.Scene:
                case RegionManager.RegionType.AddressableScene:
                    return TryGetScenePath(region, out string scenePath) &&
                           RefreshFromScene(region, scenePath);

                case RegionManager.RegionType.Prefab:
                    return RefreshFromPrefab(region);

                default:
                    return false;
            }
        }

        public static string GetSourceKey(RegionManager.Region region)
        {
            if (region == null)
                return null;

            return region.Type switch
            {
                RegionManager.RegionType.Scene when region.SceneRef != null =>
                    NormalizeAssetKey(AssetDatabase.AssetPathToGUID(region.SceneRef.ScenePath),
                        region.SceneRef.ScenePath),
                RegionManager.RegionType.Prefab when region.PrefabRef != null =>
                    NormalizeAssetKey(region.PrefabRef.AssetGUID, null),
                RegionManager.RegionType.AddressableScene when region.AddressableSceneRef != null =>
                    NormalizeAssetKey(region.AddressableSceneRef.AssetGUID, null),
                _ => null
            };
        }

        private static string NormalizeAssetKey(string guid, string fallbackPath)
        {
            if (!string.IsNullOrWhiteSpace(guid))
                return $"asset:{guid}";
            return string.IsNullOrWhiteSpace(fallbackPath) ? null : $"asset-path:{fallbackPath}";
        }

        private static bool TryGetScenePath(RegionManager.Region region, out string path)
        {
            path = string.Empty;
            if (region.Type == RegionManager.RegionType.Scene)
            {
                if (region.SceneRef?.SceneAsset == null)
                    return false;

                region.SceneRef.SceneAsset = region.SceneRef.SceneAsset;
                path = AssetDatabase.GetAssetPath(region.SceneRef.SceneAsset);
                return path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
            }

            if (region.Type == RegionManager.RegionType.AddressableScene &&
                region.AddressableSceneRef != null)
            {
                path = AssetDatabase.GUIDToAssetPath(region.AddressableSceneRef.AssetGUID);
                return path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool ApplyToScene(RegionManager.Region region, string scenePath)
        {
            Scene targetScene = EditorSceneManager.GetSceneByPath(scenePath);
            bool wasOpen = targetScene.IsValid() && targetScene.isLoaded;
            bool closeWhenDone = !wasOpen;

            try
            {
                if (!wasOpen)
                    targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                RegionBound regionBound = FindRegionBound(targetScene);
                if (regionBound == null)
                    return false;
                if (!CanApplyInverseAabb(regionBound.transform, scenePath))
                    return false;

                Undo.RecordObject(regionBound, "Apply Streaming Bounds");
                regionBound.Bounds = BoundsTransformUtility.InverseTransform(
                    region.CachedBounds,
                    regionBound.transform.localToWorldMatrix);
                EditorUtility.SetDirty(regionBound);

                if (wasOpen)
                    EditorSceneManager.MarkSceneDirty(targetScene);
                else if (!EditorSceneManager.SaveScene(targetScene))
                {
                    // Keep the modified scene open so the user can inspect/recover it.
                    closeWhenDone = false;
                    throw new InvalidOperationException($"Could not save scene '{scenePath}'.");
                }

                return true;
            }
            finally
            {
                if (closeWhenDone && targetScene.IsValid() && targetScene.isLoaded)
                    EditorSceneManager.CloseScene(targetScene, true);
            }
        }

        private static bool RefreshFromScene(RegionManager.Region region, string scenePath)
        {
            Scene targetScene = EditorSceneManager.GetSceneByPath(scenePath);
            bool wasOpen = targetScene.IsValid() && targetScene.isLoaded;

            try
            {
                if (!wasOpen)
                    targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                RegionBound regionBound = FindRegionBound(targetScene);
                if (regionBound == null)
                    return false;

                Bounds sourceBounds = BoundsTransformUtility.Transform(
                    regionBound.Bounds,
                    regionBound.transform.localToWorldMatrix);
                if (!IsUsableBounds(sourceBounds, scenePath))
                    return false;

                region.CachedBounds = sourceBounds;
                region.RebuildPortalWorldBounds();
                return true;
            }
            finally
            {
                if (!wasOpen && targetScene.IsValid() && targetScene.isLoaded)
                    EditorSceneManager.CloseScene(targetScene, true);
            }
        }

        private static RegionBound FindRegionBound(Scene scene)
        {
            RegionBound found = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var candidate in root.GetComponentsInChildren<RegionBound>(true))
                {
                    if (found != null)
                    {
                        Debug.LogError(
                            $"Scene '{scene.path}' contains more than one RegionBound. " +
                            "Keep exactly one marker before fitting or applying bounds.");
                        return null;
                    }

                    found = candidate;
                }
            }

            if (found == null)
                Debug.LogWarning($"No RegionBound found in scene '{scene.path}'.");
            return found;
        }

        private static bool ApplyToPrefab(RegionManager.Region region)
        {
            if (!TryGetPrefabPath(region, out string prefabPath))
                return false;

            GameObject prefabRoot = null;
            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                var regionBound = FindRegionBound(prefabRoot, prefabPath);
                if (regionBound == null)
                    return false;
                if (!CanApplyInverseAabb(regionBound.transform, prefabPath))
                    return false;

                Bounds currentLocal = BoundsTransformUtility.Transform(
                    regionBound.Bounds,
                    regionBound.transform.localToWorldMatrix);
                Bounds desiredLocal = new(currentLocal.center, region.CachedBounds.size);
                regionBound.Bounds = BoundsTransformUtility.InverseTransform(
                    desiredLocal,
                    regionBound.transform.localToWorldMatrix);
                EditorUtility.SetDirty(regionBound);

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath, out bool saveSucceeded);
                if (!saveSucceeded)
                    throw new InvalidOperationException($"Could not save prefab '{prefabPath}'.");

                return true;
            }
            finally
            {
                if (prefabRoot != null)
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool RefreshFromPrefab(RegionManager.Region region)
        {
            if (!TryGetPrefabPath(region, out string prefabPath))
                return false;

            GameObject prefabRoot = null;
            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                var regionBound = FindRegionBound(prefabRoot, prefabPath);
                if (regionBound == null)
                    return false;

                Bounds contentBounds = BoundsTransformUtility.Transform(
                    regionBound.Bounds,
                    regionBound.transform.localToWorldMatrix);
                if (!IsUsableBounds(contentBounds, prefabPath))
                    return false;

                region.CachedBounds = new Bounds(region.CachedBounds.center, contentBounds.size);
                region.RebuildPortalWorldBounds();
                return true;
            }
            finally
            {
                if (prefabRoot != null)
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool TryGetPrefabPath(RegionManager.Region region, out string path)
        {
            path = string.Empty;
            if (region.PrefabRef == null || string.IsNullOrWhiteSpace(region.PrefabRef.AssetGUID))
                return false;

            path = AssetDatabase.GUIDToAssetPath(region.PrefabRef.AssetGUID);
            return !string.IsNullOrWhiteSpace(path);
        }

        private static RegionBound FindRegionBound(GameObject root, string sourcePath)
        {
            RegionBound[] candidates = root.GetComponentsInChildren<RegionBound>(true);
            if (candidates.Length == 1)
                return candidates[0];

            if (candidates.Length == 0)
                Debug.LogWarning($"No RegionBound found in prefab '{sourcePath}'.");
            else
            {
                Debug.LogError(
                    $"Prefab '{sourcePath}' contains {candidates.Length} RegionBound components. " +
                    "Keep exactly one marker before fitting or applying bounds.");
            }

            return null;
        }

        private static bool CanApplyInverseAabb(Transform marker, string sourcePath)
        {
            Matrix4x4 matrix = marker.localToWorldMatrix;
            Vector3 x = matrix.MultiplyVector(Vector3.right);
            Vector3 y = matrix.MultiplyVector(Vector3.up);
            Vector3 z = matrix.MultiplyVector(Vector3.forward);

            if (x.sqrMagnitude < 0.000001f || y.sqrMagnitude < 0.000001f || z.sqrMagnitude < 0.000001f)
            {
                Debug.LogError($"Cannot apply bounds to '{sourcePath}': RegionBound has a zero scale axis.");
                return false;
            }

            int xAxis = DominantAxis(x.normalized);
            int yAxis = DominantAxis(y.normalized);
            int zAxis = DominantAxis(z.normalized);
            bool axisAligned = xAxis >= 0 && yAxis >= 0 && zAxis >= 0 &&
                               xAxis != yAxis && xAxis != zAxis && yAxis != zAxis;
            if (axisAligned)
                return true;

            Debug.LogError(
                $"Cannot apply a world AABB back to '{sourcePath}' because its RegionBound transform is rotated " +
                "off the world axes or sheared. Fit from the source, or axis-align the marker first.");
            return false;
        }

        private static int DominantAxis(Vector3 direction)
        {
            Vector3 absolute = new(
                Mathf.Abs(direction.x),
                Mathf.Abs(direction.y),
                Mathf.Abs(direction.z));
            if (absolute.x > 0.9999f && absolute.y < 0.0001f && absolute.z < 0.0001f)
                return 0;
            if (absolute.y > 0.9999f && absolute.x < 0.0001f && absolute.z < 0.0001f)
                return 1;
            if (absolute.z > 0.9999f && absolute.x < 0.0001f && absolute.y < 0.0001f)
                return 2;
            return -1;
        }

        private static bool IsUsableBounds(Bounds bounds, string sourcePath)
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            bool finite = IsFinite(center.x) && IsFinite(center.y) && IsFinite(center.z) &&
                          IsFinite(size.x) && IsFinite(size.y) && IsFinite(size.z);
            if (finite && size.x > 0f && size.y > 0f && size.z > 0f)
                return true;

            Debug.LogError($"RegionBound in '{sourcePath}' must have finite, positive bounds.");
            return false;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
#endif
