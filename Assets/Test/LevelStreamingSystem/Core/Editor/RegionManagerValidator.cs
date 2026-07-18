using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    internal readonly struct RegionValidationIssue
    {
        public RegionValidationIssue(MessageType severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public MessageType Severity { get; }
        public string Message { get; }
    }

    internal static class RegionManagerValidator
    {
        public static List<RegionValidationIssue> Validate(RegionManager manager)
        {
            var issues = new List<RegionValidationIssue>();
            if (manager == null)
                return issues;

            if (manager.RegionSelectionStrategy == null)
                issues.Add(Error("No region selection strategy is assigned."));

            var names = new HashSet<string>(StringComparer.Ordinal);
            var validNames = new HashSet<string>(StringComparer.Ordinal);
            var sourceKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var region in manager.Regions)
            {
                if (region != null && !string.IsNullOrWhiteSpace(region.RegionName))
                    validNames.Add(region.RegionName);
            }

            for (int i = 0; i < manager.Regions.Count; i++)
            {
                RegionManager.Region region = manager.Regions[i];
                string prefix = $"Region {i}";
                if (region == null)
                {
                    issues.Add(Error($"{prefix} is null."));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(region.RegionName))
                    issues.Add(Error($"{prefix} has no stable name."));
                else if (!names.Add(region.RegionName))
                    issues.Add(Error($"Region name '{region.RegionName}' is duplicated."));

                ValidateSource(region, prefix, issues);
                string sourceKey = RegionBoundsAuthoringService.GetSourceKey(region);
                if (sourceKey != null && !sourceKeys.Add(sourceKey))
                {
                    issues.Add(Warning(
                        $"{prefix} shares its source asset with another region. Runtime sharing is supported, " +
                        "but Apply Bounds must be performed individually."));
                }

                ValidateBounds(region.CachedBounds, $"{prefix} bounds", issues);

                if (region.UnloadStrategy == null)
                    issues.Add(Warning($"{prefix} has no unload strategy and will remain loaded."));

                if (region.Portals == null)
                    continue;

                for (int portalIndex = 0; portalIndex < region.Portals.Count; portalIndex++)
                {
                    RegionManager.Portal portal = region.Portals[portalIndex];
                    string portalPrefix = $"{prefix}, portal {portalIndex}";
                    if (portal == null)
                    {
                        issues.Add(Error($"{portalPrefix} is null."));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(portal.TargetRegionName))
                        issues.Add(Warning($"{portalPrefix} has no target."));
                    else if (!validNames.Contains(portal.TargetRegionName))
                        issues.Add(Error($"{portalPrefix} targets missing region '{portal.TargetRegionName}'."));

                    ValidateBounds(portal.LocalBounds, $"{portalPrefix} bounds", issues);
                }
            }

            return issues;
        }

        private static void ValidateSource(RegionManager.Region region, string prefix,
            List<RegionValidationIssue> issues)
        {
            switch (region.Type)
            {
                case RegionManager.RegionType.Scene:
                {
                    if (region.SceneRef?.SceneAsset == null)
                    {
                        issues.Add(Error($"{prefix} has no scene reference."));
                        break;
                    }

                    string canonicalPath = AssetDatabase.GetAssetPath(region.SceneRef.SceneAsset);
                    if (!string.Equals(region.SceneRef.ScenePath, canonicalPath, StringComparison.Ordinal))
                    {
                        issues.Add(Error(
                            $"{prefix} stores stale scene path '{region.SceneRef.ScenePath}'. " +
                            $"Reassign or validate the scene reference to use '{canonicalPath}'."));
                    }

                    if (!SceneBuildSettingsUtility.IsEnabled(canonicalPath))
                    {
                        issues.Add(Error(
                            $"{prefix} scene '{canonicalPath}' is not enabled in Build Settings."));
                    }
                    break;
                }

                case RegionManager.RegionType.Prefab:
                    ValidateAddressable<GameObject>(region.PrefabRef, prefix, "prefab", issues);
                    break;

                case RegionManager.RegionType.AddressableScene:
                    ValidateAddressable<SceneAsset>(region.AddressableSceneRef, prefix, "scene", issues);
                    break;

                default:
                    issues.Add(Error($"{prefix} has unsupported region type '{region.Type}'."));
                    break;
            }
        }

        private static void ValidateAddressable<T>(StreamingAssetReference reference, string prefix,
            string label, List<RegionValidationIssue> issues) where T : UnityEngine.Object
        {
            if (reference == null || !reference.RuntimeKeyIsValid() ||
                string.IsNullOrWhiteSpace(reference.AssetGUID))
            {
                issues.Add(Error($"{prefix} has no valid Addressable {label}."));
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(reference.AssetGUID);
            if (string.IsNullOrWhiteSpace(path) || AssetDatabase.LoadAssetAtPath<T>(path) == null)
            {
                issues.Add(Error(
                    $"{prefix} Addressable {label} GUID does not resolve to a {typeof(T).Name} asset."));
                return;
            }

            if (!AddressablesEditorValidation.IsAvailable)
                issues.Add(Error($"{prefix} requires Addressables, but its optional integration is not installed."));
            else if (!AddressablesEditorValidation.TryValidate(reference.AssetGUID, out string validationError))
                issues.Add(Error($"{prefix} {validationError}"));
        }

        private static void ValidateBounds(Bounds bounds, string label, List<RegionValidationIssue> issues)
        {
            if (!IsFinite(bounds.center) || !IsFinite(bounds.size))
                issues.Add(Error($"{label} contain non-finite values."));
            else if (bounds.size.x <= 0f || bounds.size.y <= 0f || bounds.size.z <= 0f)
                issues.Add(Error($"{label} must have a positive size."));
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static RegionValidationIssue Error(string message) => new(MessageType.Error, message);
        private static RegionValidationIssue Warning(string message) => new(MessageType.Warning, message);
    }
}
