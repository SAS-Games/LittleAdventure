using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    /// <summary>
    /// Builds deterministic grid coordinates while preserving an existing region
    /// ID whenever the corresponding coordinate is regenerated.
    /// </summary>
    public sealed class UniformGridRegionProvider : IWorldRegionProvider
    {
        public const int MaximumGeneratedRegionCount = 10000;

        public IReadOnlyList<StreamingRegionDefinition> Generate(WorldStreamingProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            if (profile.RegionGenerationMode != StreamingRegionGenerationMode.UniformGrid)
                throw new InvalidOperationException("The profile is not configured for UniformGrid generation.");

            Vector3Int count = profile.CellCount;
            int yCount = profile.UseXZPlaneOnly ? 1 : count.y;
            long total = (long)count.x * yCount * count.z;
            if (total > MaximumGeneratedRegionCount)
            {
                throw new InvalidOperationException(
                    $"The grid would create {total} regions. The Phase 1 safety limit is " +
                    $"{MaximumGeneratedRegionCount}.");
            }

            var stableIds = new Dictionary<Vector3Int, string>();
            foreach (StreamingRegionDefinition existing in profile.Regions)
            {
                if (existing == null || !existing.GeneratedFromGrid ||
                    stableIds.ContainsKey(existing.GridCoordinate))
                    continue;

                stableIds.Add(existing.GridCoordinate, existing.RegionId);
            }

            string prefix = SanitizeSceneName(profile.GeneratedSceneNamePrefix);
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "Region";

            var result = new List<StreamingRegionDefinition>((int)total);
            for (int z = 0; z < count.z; z++)
            for (int y = 0; y < yCount; y++)
            for (int x = 0; x < count.x; x++)
            {
                var coordinate = new Vector3Int(x, y, z);
                stableIds.TryGetValue(coordinate, out string stableId);

                Bounds bounds = CalculateBounds(profile, coordinate);
                Vector3 padding = profile.UseXZPlaneOnly
                    ? new Vector3(profile.BoundsPadding, 0f, profile.BoundsPadding)
                    : Vector3.one * profile.BoundsPadding;
                string coordinateLabel = profile.UseXZPlaneOnly
                    ? $"{x}, {z}"
                    : $"{x}, {y}, {z}";
                string sceneSuffix = profile.UseXZPlaneOnly
                    ? $"{x}_{z}"
                    : $"{x}_{y}_{z}";
                float hue = Mathf.Repeat((x * 0.173f) + (y * 0.311f) + (z * 0.071f), 1f);
                Color color = Color.HSVToRGB(hue, 0.65f, 1f);

                result.Add(StreamingRegionDefinition.CreateGridRegion(
                    stableId,
                    $"Cell [{coordinateLabel}]",
                    $"{prefix}_{sceneSuffix}",
                    bounds,
                    padding,
                    coordinate,
                    color));
            }

            return result;
        }

        private static Bounds CalculateBounds(
            WorldStreamingProfile profile,
            Vector3Int coordinate)
        {
            Vector3 origin = profile.WorldOrigin;
            Vector3 cellSize = profile.CellSize;

            if (profile.UseXZPlaneOnly)
            {
                float height = profile.MaximumWorldY - profile.MinimumWorldY;
                Vector3 size = new(cellSize.x, height, cellSize.z);
                Vector3 center = new(
                    origin.x + (coordinate.x + 0.5f) * cellSize.x,
                    (profile.MinimumWorldY + profile.MaximumWorldY) * 0.5f,
                    origin.z + (coordinate.z + 0.5f) * cellSize.z);
                return new Bounds(center, size);
            }

            Vector3 gridCenter = origin + new Vector3(
                (coordinate.x + 0.5f) * cellSize.x,
                (coordinate.y + 0.5f) * cellSize.y,
                (coordinate.z + 0.5f) * cellSize.z);
            return new Bounds(gridCenter, cellSize);
        }

        private static string SanitizeSceneName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string result = value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
                result = result.Replace(invalid, '_');

            return result.Replace('/', '_').Replace('\\', '_');
        }
    }
}
