using System.Collections.Generic;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    /// <summary>
    /// Persistent analysis database used to regenerate output without moving or
    /// deleting source-scene objects.
    /// </summary>
    public sealed class WorldStreamingAuthoringDatabase : ScriptableObject
    {
        [SerializeField] private WorldStreamingProfile m_Profile;
        [SerializeField] private string m_SourceScenePath = string.Empty;
        [SerializeField] private string m_LastAnalysisUtc = string.Empty;
        [SerializeField] private string m_LastGenerationId = string.Empty;
        [SerializeField] private int m_GenerationVersion;
        [SerializeField] private List<WorldObjectAuthoringRecord> m_Records = new();

        public WorldStreamingProfile Profile => m_Profile;
        public string SourceScenePath => m_SourceScenePath;
        public string LastAnalysisUtc => m_LastAnalysisUtc;
        public string LastGenerationId => m_LastGenerationId;
        public int GenerationVersion => m_GenerationVersion;
        public IReadOnlyList<WorldObjectAuthoringRecord> Records => m_Records;

        public int GetAssignedObjectCount(string regionId)
        {
            if (string.IsNullOrWhiteSpace(regionId))
                return 0;

            int count = 0;
            foreach (WorldObjectAuthoringRecord record in m_Records)
            {
                if (record != null && record.AssignedRegionId == regionId)
                    count++;
            }

            return count;
        }

        internal void Initialize(WorldStreamingProfile profile, string sourceScenePath)
        {
            m_Profile = profile;
            m_SourceScenePath = sourceScenePath ?? string.Empty;
        }
    }
}
