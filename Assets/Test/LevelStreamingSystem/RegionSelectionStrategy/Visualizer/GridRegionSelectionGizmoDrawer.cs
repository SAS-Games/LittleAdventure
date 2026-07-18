#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace LevelStreaming.Editor
{
    public static class GridRegionSelectionGizmoDrawer
    {
        private const int MaxPreviewCells = 10000;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmoForRegionManager(RegionManager manager, GizmoType gizmoType)
        {
            if (manager.ActiveRegionSelectionStrategy is not GridRegionSelection strategy)
                return;

            if (!strategy.DebugDraw)
                return;

            var cellSize = strategy.CellSize;
            IEnumerable<Vector3Int> cells = Application.isPlaying
                ? strategy.Grid.Keys
                : GetPreviewCells(manager, cellSize);

            foreach (var cell in cells)
            {
                var cellCenter = new Vector3(
                    (cell.x + 0.5f) * cellSize.x,
                    (cell.y + 0.5f) * cellSize.y,
                    (cell.z + 0.5f) * cellSize.z);
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(cellCenter, cellSize);
                Gizmos.color = new Color(0, 0, 1, 0.15f);
            }
        }

        private static HashSet<Vector3Int> GetPreviewCells(RegionManager manager, Vector3Int cellSize)
        {
            cellSize = new Vector3Int(
                SafeSize(cellSize.x),
                SafeSize(cellSize.y),
                SafeSize(cellSize.z));
            var cells = new HashSet<Vector3Int>();
            foreach (var region in manager.Regions)
            {
                if (region == null)
                    continue;

                Vector3Int min = WorldToCell(region.CachedBounds.min, cellSize);
                Vector3Int max = WorldToCell(region.CachedBounds.max, cellSize);
                for (long x = min.x; x <= max.x; x++)
                for (long y = min.y; y <= max.y; y++)
                for (long z = min.z; z <= max.z; z++)
                {
                    cells.Add(new Vector3Int((int)x, (int)y, (int)z));
                    if (cells.Count >= MaxPreviewCells)
                        return cells;
                }
            }

            return cells;
        }

        private static Vector3Int WorldToCell(Vector3 position, Vector3Int size)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / size.x),
                Mathf.FloorToInt(position.y / size.y),
                Mathf.FloorToInt(position.z / size.z));
        }

        private static int SafeSize(int value)
        {
            return value == int.MinValue ? int.MaxValue : Mathf.Max(1, Mathf.Abs(value));
        }
    }
}
#endif
