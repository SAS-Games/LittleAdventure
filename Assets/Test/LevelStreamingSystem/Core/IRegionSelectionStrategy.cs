using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelStreaming
{
    public interface IRegionSelectionStrategy
    {
        void Initialize(List<RegionManager.Region> regionRefs);
        List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds);
    }

    public abstract class RegionSelectionStrategySO : ScriptableObject, IRegionSelectionStrategy
    {
        [field: SerializeField] public bool DebugDraw { get; private set; } = false;
        public abstract void Initialize(List<RegionManager.Region> regionRefs);
        public abstract List<RegionManager.Region> GetNearbyRegions(Bounds queryBounds);
    }
}