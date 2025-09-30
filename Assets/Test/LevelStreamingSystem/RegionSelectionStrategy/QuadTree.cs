using System.Collections.Generic;
using UnityEngine;

namespace LevelStreaming
{
    public class QuadtreeNode
    {
        private readonly Bounds bounds;
        private readonly int depth;
        private readonly int maxDepth;
        private readonly int maxCapacity;
        private List<RegionManager.Region> objects;
        private QuadtreeNode[] children;

        public QuadtreeNode(Bounds bounds, int depth, int maxDepth, int maxCapacity)
        {
            this.bounds = bounds;
            this.depth = depth;
            this.maxDepth = maxDepth;
            this.maxCapacity = maxCapacity;
            objects = new List<RegionManager.Region>();
            children = null;
        }

        public void Insert(RegionManager.Region region)
        {
            if (!bounds.Intersects(region.CachedBounds))
                return;

            if (children == null)
            {
                objects.Add(region);

                if (objects.Count > maxCapacity && depth < maxDepth)
                    Subdivide();
            }
            else
            {
                foreach (var child in children)
                    child.Insert(region);
            }
        }

        public void Query(Bounds range, List<RegionManager.Region> results)
        {
            if (!bounds.Intersects(range))
                return;

            if (children == null)
            {
                foreach (var obj in objects)
                {
                    if (obj.CachedBounds.Intersects(range))
                        results.Add(obj);
                }
            }
            else
            {
                foreach (var child in children)
                    child.Query(range, results);
            }
        }

        private void Subdivide()
        {
            children = new QuadtreeNode[4];

            Vector3 size = bounds.size / 2f;
            Vector3 center = bounds.center;

            children[0] = new QuadtreeNode(new Bounds(center + new Vector3(-size.x / 2, size.y / 2, 0), size),
                depth + 1, maxDepth, maxCapacity);
            children[1] = new QuadtreeNode(new Bounds(center + new Vector3(size.x / 2, size.y / 2, 0), size),
                depth + 1, maxDepth, maxCapacity);
            children[2] = new QuadtreeNode(new Bounds(center + new Vector3(-size.x / 2, -size.y / 2, 0), size),
                depth + 1, maxDepth, maxCapacity);
            children[3] = new QuadtreeNode(new Bounds(center + new Vector3(size.x / 2, -size.y / 2, 0), size),
                depth + 1, maxDepth, maxCapacity);

            foreach (var obj in objects)
            {
                foreach (var child in children)
                    child.Insert(obj);
            }

            objects.Clear();
        }

        // 🔹 Public read-only access for visualization
        public Bounds Bounds => bounds;
        public int Depth => depth;
        public QuadtreeNode[] Children => children;
        public IReadOnlyList<RegionManager.Region> Objects => objects;
    }
}
