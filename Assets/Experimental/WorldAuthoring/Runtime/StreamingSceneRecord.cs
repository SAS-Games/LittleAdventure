using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.WorldStreaming
{
    /// <summary>
    /// Runtime metadata for one generated streaming scene.
    /// </summary>
    [Serializable]
    public sealed class StreamingSceneRecord
    {
        [SerializeField] private string m_RegionId = string.Empty;
        [SerializeField] private string m_ScenePath = string.Empty;
        [SerializeField] private Bounds m_Bounds = new(Vector3.zero, Vector3.one);
        [SerializeField] private Bounds m_LoadBounds = new(Vector3.zero, Vector3.one);
        [SerializeField] private Bounds m_UnloadBounds = new(Vector3.zero, Vector3.one);
        [SerializeField] private Vector3Int m_GridCoordinate;
        [SerializeField] private List<string> m_NeighborRegionIds = new();

        public string RegionId => m_RegionId;
        public string ScenePath => m_ScenePath;
        public Bounds Bounds => m_Bounds;
        public Bounds LoadBounds => m_LoadBounds;
        public Bounds UnloadBounds => m_UnloadBounds;
        public Vector3Int GridCoordinate => m_GridCoordinate;
        public IReadOnlyList<string> NeighborRegionIds => m_NeighborRegionIds;

        internal void Initialize(
            string regionId,
            string scenePath,
            Bounds bounds,
            Bounds loadBounds,
            Bounds unloadBounds,
            Vector3Int gridCoordinate,
            IEnumerable<string> neighborRegionIds)
        {
            m_RegionId = regionId ?? string.Empty;
            m_ScenePath = scenePath ?? string.Empty;
            m_Bounds = bounds;
            m_LoadBounds = loadBounds;
            m_UnloadBounds = unloadBounds;
            m_GridCoordinate = gridCoordinate;
            m_NeighborRegionIds = neighborRegionIds == null
                ? new List<string>()
                : new List<string>(neighborRegionIds);
        }
    }
}
