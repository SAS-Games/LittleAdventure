#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    public static class GridRegionSelection2DGizmoDrawer
    {
        private static readonly GUIStyle _labelStyle;

        static GridRegionSelection2DGizmoDrawer()
        {
            _labelStyle = new GUIStyle
            {
                normal = { textColor = Color.white },
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmoForRegionManager(RegionManager manager, GizmoType gizmoType)
        {
            if (manager.RegionSelectionStrategy is not GridRegionSelection2D strategy)
                return;

            if (!strategy.DebugDraw)
                return;

            if (!Application.isPlaying && manager.Regions is { Count: > 0 })
                strategy.Initialize(manager.Regions);

            var grid = strategy.Grid;
            var cellSize = strategy.CellSize;

            if (grid == null || grid.Count == 0)
                return;

            // Find min/max occupied cell indices
            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

            foreach (var cell in grid.Keys)
            {
                if (cell.x < min.x) min.x = cell.x;
                if (cell.y < min.y) min.y = cell.y;
                if (cell.x > max.x) max.x = cell.x;
                if (cell.y > max.y) max.y = cell.y;
            }

            // Iterate all cells in bounding range
            for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
            {
                var cell = new Vector2Int(x, y);
                var cellCenter = new Vector3((x + 0.5f) * cellSize.x,
                    (y + 0.5f) * cellSize.y, 0f);
                var cellSizeVec = new Vector3(cellSize.x, cellSize.y, 0.1f);

                // Occupied vs Empty colors
                if (grid.ContainsKey(cell))
                    Gizmos.color = Color.green; // Occupied cell
                else
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Empty cell

                Gizmos.DrawWireCube(cellCenter, cellSizeVec);
                //Handles.Label(cellCenter, $"({x}, {y})", _labelStyle);
            }
        }
    }
}
#endif