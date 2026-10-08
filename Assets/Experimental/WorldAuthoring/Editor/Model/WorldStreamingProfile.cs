using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    /// <summary>
    /// Authoring configuration for one source world and its generated output.
    /// Generated scenes are not source-of-truth assets; this profile and its
    /// database preserve the regeneration contract.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WorldStreamingProfile",
        menuName = "SAS/World Streaming/World Streaming Profile")]
    public sealed class WorldStreamingProfile : ScriptableObject
    {
        [Header("Source World")]
        [SerializeField] private SceneAsset m_SourceAuthoringScene;
        [SerializeField] private string m_OutputRootFolder = "Assets/WorldStreaming/Generated";
        [SerializeField] private string m_GeneratedSceneNamePrefix = "Region";
        [SerializeField] private bool m_CreatePersistentScene = true;
        [SerializeField] private string m_PersistentSceneName = "Persistent";
        [SerializeField] private WorldStreamingAuthoringDatabase m_AuthoringDatabase;

        [Header("World Layout")]
        [SerializeField] private StreamingRegionGenerationMode m_RegionGenerationMode =
            StreamingRegionGenerationMode.UniformGrid;
        [SerializeField] private Vector3 m_WorldOrigin;
        [SerializeField] private Vector3 m_CellSize = new(500f, 200f, 500f);
        [SerializeField] private Vector3Int m_CellCount = new(4, 1, 4);
        [SerializeField, Min(0f)] private float m_BoundsPadding;
        [SerializeField] private bool m_UseXZPlaneOnly = true;
        [SerializeField] private float m_MinimumWorldY = -100f;
        [SerializeField] private float m_MaximumWorldY = 500f;
        [SerializeField] private List<StreamingRegionDefinition> m_Regions = new();
        [SerializeField] private List<StreamingRegionDefinition> m_ManualRegions = new();
        [SerializeField] private bool m_ShowSceneViewVisualization = true;

        [Header("Persistent Classification")]
        [SerializeField] private List<WorldObjectReference> m_PersistentRootObjects = new();
        [SerializeField] private LayerMask m_PersistentLayers;
        [SerializeField] private bool m_IncludeChildrenOfPersistentLayerObjects = true;
        [SerializeField] private List<string> m_PersistentUnityTags = new();

        [Header("Shared Scenes")]
        [SerializeField] private List<SharedSceneDefinition> m_SharedScenes = new();

        [Header("Future Analysis Defaults")]
        [SerializeField] private BoundsFallbackMode m_BoundsFallbackMode =
            BoundsFallbackMode.TransformPosition;
        [SerializeField] private Vector3 m_FixedFallbackBoundsSize = Vector3.one;
        [SerializeField] private RegionAssignmentStrategy m_AssignmentStrategy =
            RegionAssignmentStrategy.MaximumBoundsOverlap;
        [SerializeField] private CrossRegionObjectPolicy m_CrossRegionPolicy =
            CrossRegionObjectPolicy.RequireManualAssignment;
        [SerializeField] private HierarchyAssignmentPolicy m_HierarchyPolicy =
            HierarchyAssignmentPolicy.PrefabInstanceRoot;
        [SerializeField] private NeighborMode m_NeighborMode = NeighborMode.CardinalAndDiagonal8;
        [SerializeField] private bool m_BlockGenerationOnUnsafeCrossSceneReferences = true;

        public SceneAsset SourceAuthoringScene => m_SourceAuthoringScene;
        public string OutputRootFolder => m_OutputRootFolder;
        public string GeneratedSceneNamePrefix => m_GeneratedSceneNamePrefix;
        public bool CreatePersistentScene => m_CreatePersistentScene;
        public string PersistentSceneName => m_PersistentSceneName;
        public WorldStreamingAuthoringDatabase AuthoringDatabase => m_AuthoringDatabase;
        public StreamingRegionGenerationMode RegionGenerationMode => m_RegionGenerationMode;
        public Vector3 WorldOrigin => m_WorldOrigin;
        public Vector3 CellSize => m_CellSize;
        public Vector3Int CellCount => m_CellCount;
        public float BoundsPadding => m_BoundsPadding;
        public bool UseXZPlaneOnly => m_UseXZPlaneOnly;
        public float MinimumWorldY => m_MinimumWorldY;
        public float MaximumWorldY => m_MaximumWorldY;
        public IReadOnlyList<StreamingRegionDefinition> Regions =>
            m_RegionGenerationMode == StreamingRegionGenerationMode.UniformGrid
                ? m_Regions
                : m_ManualRegions;
        public bool ShowSceneViewVisualization => m_ShowSceneViewVisualization;
        public IReadOnlyList<WorldObjectReference> PersistentRootObjects => m_PersistentRootObjects;
        public LayerMask PersistentLayers => m_PersistentLayers;
        public bool IncludeChildrenOfPersistentLayerObjects =>
            m_IncludeChildrenOfPersistentLayerObjects;
        public IReadOnlyList<string> PersistentUnityTags => m_PersistentUnityTags;
        public IReadOnlyList<SharedSceneDefinition> SharedScenes => m_SharedScenes;
        public BoundsFallbackMode BoundsFallbackMode => m_BoundsFallbackMode;
        public Vector3 FixedFallbackBoundsSize => m_FixedFallbackBoundsSize;
        public RegionAssignmentStrategy AssignmentStrategy => m_AssignmentStrategy;
        public CrossRegionObjectPolicy CrossRegionPolicy => m_CrossRegionPolicy;
        public HierarchyAssignmentPolicy HierarchyPolicy => m_HierarchyPolicy;
        public NeighborMode NeighborMode => m_NeighborMode;
        public bool BlockGenerationOnUnsafeCrossSceneReferences =>
            m_BlockGenerationOnUnsafeCrossSceneReferences;

        public void ConfigureUniformGrid(
            Vector3 worldOrigin,
            Vector3 cellSize,
            Vector3Int cellCount,
            bool useXZPlaneOnly,
            float minimumWorldY,
            float maximumWorldY,
            float boundsPadding = 0f)
        {
            m_RegionGenerationMode = StreamingRegionGenerationMode.UniformGrid;
            m_WorldOrigin = worldOrigin;
            m_CellSize = cellSize;
            m_CellCount = cellCount;
            m_UseXZPlaneOnly = useXZPlaneOnly;
            m_MinimumWorldY = minimumWorldY;
            m_MaximumWorldY = maximumWorldY;
            m_BoundsPadding = boundsPadding;
            SanitizeConfiguration();
        }

        public void SetWorldOrigin(Vector3 worldOrigin)
        {
            m_WorldOrigin = worldOrigin;
        }

        public void ReplaceRegions(IEnumerable<StreamingRegionDefinition> regions)
        {
            List<StreamingRegionDefinition> replacement = regions == null
                ? new List<StreamingRegionDefinition>()
                : new List<StreamingRegionDefinition>(regions);

            if (m_RegionGenerationMode == StreamingRegionGenerationMode.UniformGrid)
                m_Regions = replacement;
            else
                m_ManualRegions = replacement;

            SanitizeDefinitions();
        }

        public int AddManualRegion()
        {
            Vector3 size = m_UseXZPlaneOnly
                ? new Vector3(m_CellSize.x, m_MaximumWorldY - m_MinimumWorldY, m_CellSize.z)
                : m_CellSize;
            Vector3 center = m_UseXZPlaneOnly
                ? new Vector3(m_WorldOrigin.x, (m_MinimumWorldY + m_MaximumWorldY) * 0.5f,
                    m_WorldOrigin.z)
                : m_WorldOrigin;

            m_ManualRegions.Add(StreamingRegionDefinition.CreateManual(
                m_ManualRegions.Count,
                new Bounds(center, size)));
            return m_ManualRegions.Count - 1;
        }

        public int DuplicateManualRegion(int index)
        {
            if (index < 0 || index >= m_ManualRegions.Count ||
                m_ManualRegions[index] == null)
                return -1;

            m_ManualRegions.Insert(
                index + 1,
                m_ManualRegions[index].DuplicateAsManual());
            return index + 1;
        }

        public bool RemoveManualRegion(int index)
        {
            if (index < 0 || index >= m_ManualRegions.Count ||
                m_ManualRegions[index] == null)
                return false;

            m_ManualRegions.RemoveAt(index);
            return true;
        }

        public void ClearGeneratedRegions()
        {
            m_Regions.RemoveAll(region => region == null || region.GeneratedFromGrid);
        }

        internal void SetAuthoringDatabase(WorldStreamingAuthoringDatabase database)
        {
            m_AuthoringDatabase = database;
        }

        private void OnValidate()
        {
            SanitizeConfiguration();
            SanitizeDefinitions();
        }

        private void SanitizeConfiguration()
        {
            m_OutputRootFolder = (m_OutputRootFolder ?? string.Empty).Replace('\\', '/').TrimEnd('/');
            m_CellSize = new Vector3(
                SanitizePositive(m_CellSize.x),
                SanitizePositive(m_CellSize.y),
                SanitizePositive(m_CellSize.z));
            m_CellCount = new Vector3Int(
                SanitizeCount(m_CellCount.x),
                SanitizeCount(m_CellCount.y),
                SanitizeCount(m_CellCount.z));
            m_BoundsPadding = Mathf.Max(0f, m_BoundsPadding);

            if (!IsFinite(m_MinimumWorldY))
                m_MinimumWorldY = -100f;
            if (!IsFinite(m_MaximumWorldY))
                m_MaximumWorldY = 500f;
            if (m_MaximumWorldY <= m_MinimumWorldY)
                m_MaximumWorldY = m_MinimumWorldY + 1f;

            m_FixedFallbackBoundsSize = new Vector3(
                SanitizePositive(m_FixedFallbackBoundsSize.x),
                SanitizePositive(m_FixedFallbackBoundsSize.y),
                SanitizePositive(m_FixedFallbackBoundsSize.z));
        }

        private void SanitizeDefinitions()
        {
            m_Regions ??= new List<StreamingRegionDefinition>();
            foreach (StreamingRegionDefinition region in m_Regions)
                region?.Sanitize();

            m_ManualRegions ??= new List<StreamingRegionDefinition>();
            foreach (StreamingRegionDefinition region in m_ManualRegions)
                region?.Sanitize();

            m_SharedScenes ??= new List<SharedSceneDefinition>();
            for (int i = 0; i < m_SharedScenes.Count; i++)
                m_SharedScenes[i]?.Sanitize(i);

            m_PersistentRootObjects ??= new List<WorldObjectReference>();
            m_PersistentUnityTags ??= new List<string>();
        }

        private static int SanitizeCount(int value)
        {
            return value == int.MinValue ? int.MaxValue : Mathf.Max(1, Mathf.Abs(value));
        }

        private static float SanitizePositive(float value)
        {
            return IsFinite(value) ? Mathf.Max(0.01f, Mathf.Abs(value)) : 1f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
