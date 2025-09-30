#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    public static class GridRegionSelectionGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmoForRegionManager(RegionManager manager, GizmoType gizmoType)
        {
            if (manager.RegionSelectionStrategy is not GridRegionSelection strategy)
                return;

            if (!strategy.DebugDraw)
                return;

            if (!Application.isPlaying && manager.Regions is { Count: > 0 })
                strategy.Initialize(manager.Regions);
            
            var grid = strategy.Grid;
            var cellSize = strategy.CellSize;

            foreach (var kvp in grid)
            {
                var cell = kvp.Key;
                var cellCenter = new Vector3(
                    (cell.x + 0.5f) * cellSize.x,
                    (cell.y + 0.5f) * cellSize.y,
                    (cell.z + 0.5f) * cellSize.z);
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(cellCenter, cellSize);
                Gizmos.color = new Color(0, 0, 1, 0.15f);
            }
        }
    }
}
#endif