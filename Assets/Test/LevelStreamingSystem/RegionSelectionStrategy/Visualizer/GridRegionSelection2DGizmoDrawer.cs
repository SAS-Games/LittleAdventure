#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    public static class GridRegionSelection2DGizmoDrawer
    {
        private const int MaxPreviewCells = 10000;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmoForRegionManager(RegionManager manager, GizmoType gizmoType)
        {
            if (manager.ActiveRegionSelectionStrategy is not GridRegionSelection2D strategy)
                return;

            if (!strategy.DebugDraw)
                return;

            var cellSize = strategy.CellSize;
            var occupiedCells = Application.isPlaying
                ? new HashSet<Vector2Int>(strategy.Grid.Keys)
                : GetPreviewCells(manager, cellSize);

            foreach (var cell in occupiedCells)
            {
                var cellCenter = new Vector3((cell.x + 0.5f) * cellSize.x,
                    (cell.y + 0.5f) * cellSize.y, 0f);
                var cellSizeVec = new Vector3(cellSize.x, cellSize.y, 0.1f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(cellCenter, cellSizeVec);
            }
        }

        private static HashSet<Vector2Int> GetPreviewCells(RegionManager manager, Vector2Int cellSize)
        {
            cellSize = new Vector2Int(
                SafeSize(cellSize.x),
                SafeSize(cellSize.y));
            var cells = new HashSet<Vector2Int>();
            foreach (var region in manager.Regions)
            {
                if (region == null)
                    continue;

                Vector2Int min = WorldToCell(region.CachedBounds.min, cellSize);
                Vector2Int max = WorldToCell(region.CachedBounds.max, cellSize);
                for (long x = min.x; x <= max.x; x++)
                for (long y = min.y; y <= max.y; y++)
                {
                    cells.Add(new Vector2Int((int)x, (int)y));
                    if (cells.Count >= MaxPreviewCells)
                        return cells;
                }
            }

            return cells;
        }

        private static Vector2Int WorldToCell(Vector3 position, Vector2Int size)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / size.x),
                Mathf.FloorToInt(position.y / size.y));
        }

        private static int SafeSize(int value)
        {
            return value == int.MinValue ? int.MaxValue : Mathf.Max(1, Mathf.Abs(value));
        }
    }
}
#endif
