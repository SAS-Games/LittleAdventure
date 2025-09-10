using System.Collections.Generic;
using UnityEngine;

public interface IRegionSelectionStrategy
{
    void Initialize(List<RegionManager.Region> sceneRefs);
    List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds);
}

public abstract class RegionSelectionStrategySO : ScriptableObject, IRegionSelectionStrategy
{
    public abstract void Initialize(List<RegionManager.Region> sceneRefs);
    public abstract List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds);
}