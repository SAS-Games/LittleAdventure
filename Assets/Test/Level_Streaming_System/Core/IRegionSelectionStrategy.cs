using System.Collections.Generic;
using UnityEngine;

public interface IRegionSelectionStrategy
{
    void Initialize(List<RegionManager.Region> regionRefs);
    List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds);
}

public abstract class RegionSelectionStrategySO : ScriptableObject, IRegionSelectionStrategy
{
    public abstract void Initialize(List<RegionManager.Region> regionRefs);
    public abstract List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds);
}