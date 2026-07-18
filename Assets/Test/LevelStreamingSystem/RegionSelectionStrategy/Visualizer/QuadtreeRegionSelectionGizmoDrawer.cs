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
            if (manager.ActiveRegionSelectionStrategy is not QuadtreeRegionSelection strategy)
                return;
            if (!strategy.DebugDraw)
                return;

            QuadtreeNode root = strategy.Root;
            if (!Application.isPlaying)
                root = BuildPreview(manager, strategy.MaxDepth, strategy.MaxCapacity);

            if (root != null)
                DrawNodeRecursive(root, Mathf.Max(1, strategy.MaxDepth));
        }

        private static QuadtreeNode BuildPreview(RegionManager manager, int maxDepth, int maxCapacity)
        {
            if (manager.Regions == null || manager.Regions.Count == 0)
                return null;

            bool hasRegion = false;
            Bounds worldBounds = default;
            foreach (var region in manager.Regions)
            {
                if (region == null)
                    continue;

                if (!hasRegion)
                {
                    worldBounds = region.CachedBounds;
                    hasRegion = true;
                }
                else
                    worldBounds.Encapsulate(region.CachedBounds);
            }

            if (!hasRegion)
                return null;

            var root = new QuadtreeNode(worldBounds, 0, maxDepth, maxCapacity);
            foreach (var region in manager.Regions)
                root.Insert(region);
            return root;
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
