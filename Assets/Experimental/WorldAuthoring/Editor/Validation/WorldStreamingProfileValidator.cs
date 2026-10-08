using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    internal static class WorldStreamingProfileValidator
    {
        public static List<WorldStreamingValidationIssue> Validate(WorldStreamingProfile profile)
        {
            var issues = new List<WorldStreamingValidationIssue>();
            if (profile == null)
            {
                issues.Add(Error("PROFILE_MISSING", "No World Streaming Profile is selected."));
                return issues;
            }

            ValidateSourceAndOutput(profile, issues);
            ValidateGrid(profile, issues);
            ValidateRegions(profile, issues);
            return issues;
        }

        private static void ValidateSourceAndOutput(
            WorldStreamingProfile profile,
            List<WorldStreamingValidationIssue> issues)
        {
            if (profile.SourceAuthoringScene == null)
            {
                issues.Add(Error(
                    "SOURCE_SCENE_MISSING",
                    "Assign the large source authoring scene.",
                    profile));
            }

            string output = profile.OutputRootFolder;
            if (string.IsNullOrWhiteSpace(output) ||
                (!string.Equals(output, "Assets", StringComparison.Ordinal) &&
                 !output.StartsWith("Assets/", StringComparison.Ordinal)))
            {
                issues.Add(Error(
                    "OUTPUT_FOLDER_INVALID",
                    "The output root must be a project-relative folder below Assets.",
                    profile));
            }

            string sourcePath = profile.SourceAuthoringScene == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(profile.SourceAuthoringScene);
            if (!string.IsNullOrWhiteSpace(sourcePath) &&
                !string.IsNullOrWhiteSpace(output) &&
                (string.Equals(sourcePath, output, StringComparison.OrdinalIgnoreCase) ||
                 sourcePath.StartsWith(output.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(Error(
                    "SOURCE_INSIDE_OUTPUT",
                    "The source authoring scene cannot be inside the generated output folder.",
                    profile.SourceAuthoringScene));
            }

            if (profile.CreatePersistentScene && !IsValidSceneName(profile.PersistentSceneName))
            {
                issues.Add(Error(
                    "PERSISTENT_SCENE_NAME_INVALID",
                    "Persistent Scene Name is empty or contains invalid file-name characters.",
                    profile));
            }
        }

        private static void ValidateGrid(
            WorldStreamingProfile profile,
            List<WorldStreamingValidationIssue> issues)
        {
            if (profile.RegionGenerationMode != StreamingRegionGenerationMode.UniformGrid)
                return;

            int yCount = profile.UseXZPlaneOnly ? 1 : profile.CellCount.y;
            long total = (long)profile.CellCount.x * yCount * profile.CellCount.z;
            if (total > UniformGridRegionProvider.MaximumGeneratedRegionCount)
            {
                issues.Add(Error(
                    "GRID_REGION_LIMIT",
                    $"The configured grid would create {total} regions; the current safety limit is " +
                    $"{UniformGridRegionProvider.MaximumGeneratedRegionCount}.",
                    profile));
            }

            if (profile.UseXZPlaneOnly && profile.MaximumWorldY <= profile.MinimumWorldY)
            {
                issues.Add(Error(
                    "GRID_HEIGHT_INVALID",
                    "Maximum World Y must be greater than Minimum World Y.",
                    profile));
            }

            if (profile.Regions.Count == 0)
            {
                issues.Add(new WorldStreamingValidationIssue(
                    ValidationSeverity.Warning,
                    "GRID_PREVIEW_MISSING",
                    "Generate the grid preview before scanning the source world.",
                    profile));
            }
        }

        private static void ValidateRegions(
            WorldStreamingProfile profile,
            List<WorldStreamingValidationIssue> issues)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var sceneNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < profile.Regions.Count; i++)
            {
                StreamingRegionDefinition region = profile.Regions[i];
                if (region == null)
                {
                    issues.Add(Error("REGION_NULL", $"Region entry {i} is null.", profile));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(region.RegionId))
                {
                    issues.Add(Error(
                        "REGION_ID_MISSING",
                        $"Region {i} has no stable ID.",
                        profile));
                }
                else if (!ids.Add(region.RegionId))
                {
                    issues.Add(Error(
                        "REGION_ID_DUPLICATE",
                        $"Region ID '{region.RegionId}' is duplicated.",
                        profile,
                        region.RegionId));
                }

                if (!IsValidSceneName(region.SceneName))
                {
                    issues.Add(Error(
                        "REGION_SCENE_NAME_INVALID",
                        $"Region '{region.DisplayName}' has an invalid scene name.",
                        profile,
                        region.RegionId));
                }
                else if (!sceneNames.Add(region.SceneName))
                {
                    issues.Add(Error(
                        "REGION_SCENE_NAME_DUPLICATE",
                        $"Generated scene name '{region.SceneName}' is duplicated.",
                        profile,
                        region.RegionId));
                }

                Vector3 size = region.Bounds.size;
                if (!IsFinite(region.Bounds.center) || !IsFinite(size) ||
                    size.x <= 0f || size.y <= 0f || size.z <= 0f)
                {
                    issues.Add(Error(
                        "REGION_BOUNDS_INVALID",
                        $"Region '{region.DisplayName}' must have finite, positive bounds.",
                        profile,
                        region.RegionId));
                }

                Vector3 load = region.LoadPadding;
                Vector3 unload = region.UnloadPadding;
                if (unload.x < load.x || unload.y < load.y || unload.z < load.z)
                {
                    issues.Add(Error(
                        "REGION_UNLOAD_PADDING_INVALID",
                        $"Region '{region.DisplayName}' unload padding cannot be smaller than load padding.",
                        profile,
                        region.RegionId));
                }
            }
        }

        private static bool IsValidSceneName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                   value.IndexOf('/') < 0 &&
                   value.IndexOf('\\') < 0;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static WorldStreamingValidationIssue Error(
            string code,
            string message,
            UnityEngine.Object context = null,
            string regionId = null)
        {
            return new WorldStreamingValidationIssue(
                ValidationSeverity.Error,
                code,
                message,
                context,
                regionId);
        }
    }
}
