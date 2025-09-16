using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Streaming/RegionSelection/QuadtreeStrategy")]
public class QuadtreeRegionSelection : RegionSelectionStrategySO
{
    private QuadtreeNode root;
    [SerializeField] private int maxDepth = 6;
    [SerializeField] private int maxCapacity = 4;

    public override void Initialize(List<RegionManager.Region> regionRefs)
    {
        // Find world bounds from scenes
        Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one);
        foreach (var scene in regionRefs)
            worldBounds.Encapsulate(scene.CachedBounds);

        root = new QuadtreeNode(worldBounds, 0, maxDepth, maxCapacity);

        foreach (var scene in regionRefs)
            root.Insert(scene);
    }

    public override List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds)
    {
        var results = new List<RegionManager.Region>();
        root.Query(queryBounds, results);
        return results;
    }
}