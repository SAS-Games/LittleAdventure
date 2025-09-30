#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    public static class QuadtreeRegionSelectionGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmoForRegionManager(RegionManager manager, GizmoType gizmoType)
        {
            if (manager.RegionSelectionStrategy is not QuadtreeRegionSelection strategy)
                return;
            if (!strategy.DebugDraw)
                return;

            if (!Application.isPlaying && manager.Regions is { Count: > 0 })
                strategy.Initialize(manager.Regions);

            if (strategy.Root != null)
                DrawNodeRecursive(strategy.Root, strategy.MaxDepth);
        }

        private static void DrawNodeRecursive(QuadtreeNode node, int maxDepth)
        {
            if (node == null) return;

            float t = node.Depth / (float)maxDepth;
            Gizmos.color = Color.Lerp(Color.green, Color.red, t);

            var b = node.Bounds;
            Gizmos.DrawWireCube(b.center, b.size);

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                    DrawNodeRecursive(child, maxDepth);
            }
        }
    }
}
#endif
