using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    /// <summary>
    /// X/Z quadtree for typical Unity world-space regions. Regions that straddle child
    /// boundaries remain in their parent, preventing duplicate results and exponential
    /// insertion into multiple branches.
    /// </summary>
    public sealed class QuadtreeNode
    {
        private readonly Bounds bounds;
        private readonly int depth;
        private readonly int maxDepth;
        private readonly int maxCapacity;
        private readonly List<RegionManager.Region> regions;
        private QuadtreeNode[] children;

        public QuadtreeNode(Bounds bounds, int depth, int maxDepth, int maxCapacity)
        {
            this.bounds = bounds;
            this.depth = depth;
            this.maxDepth = Mathf.Max(0, maxDepth);
            this.maxCapacity = Mathf.Max(1, maxCapacity);
            regions = new List<RegionManager.Region>();
        }

        public void Insert(RegionManager.Region region)
        {
            if (region == null || !bounds.Intersects(region.CachedBounds))
                return;

            if (children != null)
            {
                int childIndex = GetContainingChild(region.CachedBounds);
                if (childIndex >= 0)
                {
                    children[childIndex].Insert(region);
                    return;
                }
            }

            regions.Add(region);
            if (children == null && regions.Count > maxCapacity && depth < maxDepth)
                Subdivide();
        }

        public void Query(Bounds range, HashSet<RegionManager.Region> results)
        {
            if (!bounds.Intersects(range))
                return;

            foreach (var region in regions)
            {
                if (region.CachedBounds.Intersects(range))
                    results.Add(region);
            }

            if (children == null)
                return;

            foreach (var child in children)
                child.Query(range, results);
        }

        private void Subdivide()
        {
            children = new QuadtreeNode[4];

            Vector3 childSize = new(bounds.size.x * 0.5f, bounds.size.y, bounds.size.z * 0.5f);
            Vector3 center = bounds.center;
            float offsetX = childSize.x * 0.5f;
            float offsetZ = childSize.z * 0.5f;

            children[0] = CreateChild(center + new Vector3(-offsetX, 0f, -offsetZ), childSize);
            children[1] = CreateChild(center + new Vector3(offsetX, 0f, -offsetZ), childSize);
            children[2] = CreateChild(center + new Vector3(-offsetX, 0f, offsetZ), childSize);
            children[3] = CreateChild(center + new Vector3(offsetX, 0f, offsetZ), childSize);

            for (int i = regions.Count - 1; i >= 0; i--)
            {
                int childIndex = GetContainingChild(regions[i].CachedBounds);
                if (childIndex < 0)
                    continue;

                RegionManager.Region region = regions[i];
                regions.RemoveAt(i);
                children[childIndex].Insert(region);
            }
        }

        private QuadtreeNode CreateChild(Vector3 center, Vector3 size)
        {
            return new QuadtreeNode(new Bounds(center, size), depth + 1, maxDepth, maxCapacity);
        }

        private int GetContainingChild(Bounds regionBounds)
        {
            if (children == null)
                return -1;

            for (int i = 0; i < children.Length; i++)
            {
                if (Contains(children[i].bounds, regionBounds))
                    return i;
            }

            return -1;
        }

        private static bool Contains(Bounds outer, Bounds inner)
        {
            Vector3 outerMin = outer.min;
            Vector3 outerMax = outer.max;
            Vector3 innerMin = inner.min;
            Vector3 innerMax = inner.max;
            return innerMin.x >= outerMin.x && innerMax.x <= outerMax.x &&
                   innerMin.y >= outerMin.y && innerMax.y <= outerMax.y &&
                   innerMin.z >= outerMin.z && innerMax.z <= outerMax.z;
        }

        public Bounds Bounds => bounds;
        public int Depth => depth;
        public QuadtreeNode[] Children => children;
        public IReadOnlyList<RegionManager.Region> Regions => regions;
    }
}
