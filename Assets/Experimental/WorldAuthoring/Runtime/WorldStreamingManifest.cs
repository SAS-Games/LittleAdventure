using System.Collections.Generic;
using UnityEngine;

namespace SAS.WorldStreaming
{
    /// <summary>
    /// Generated, runtime-friendly description of an authored streaming world.
    ///
    /// The current LevelStreaming.RegionManager remains the runtime authority.
    /// A later generation phase will populate this asset and adapt its region
    /// records into the existing RegionManager configuration.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WorldStreamingManifest",
        menuName = "SAS/World Streaming/World Streaming Manifest")]
    public sealed class WorldStreamingManifest : ScriptableObject
    {
        [SerializeField] private string m_WorldId = string.Empty;
        [SerializeField] private string m_PersistentScenePath = string.Empty;
        [SerializeField] private List<StreamingSceneRecord> m_StreamingScenes = new();
        [SerializeField] private List<SharedSceneRecord> m_SharedScenes = new();

        public string WorldId => m_WorldId;
        public string PersistentScenePath => m_PersistentScenePath;
        public IReadOnlyList<StreamingSceneRecord> StreamingScenes => m_StreamingScenes;
        public IReadOnlyList<SharedSceneRecord> SharedScenes => m_SharedScenes;

        internal void Replace(
            string worldId,
            string persistentScenePath,
            IEnumerable<StreamingSceneRecord> streamingScenes,
            IEnumerable<SharedSceneRecord> sharedScenes)
        {
            m_WorldId = worldId ?? string.Empty;
            m_PersistentScenePath = persistentScenePath ?? string.Empty;
            m_StreamingScenes = streamingScenes == null
                ? new List<StreamingSceneRecord>()
                : new List<StreamingSceneRecord>(streamingScenes);
            m_SharedScenes = sharedScenes == null
                ? new List<SharedSceneRecord>()
                : new List<SharedSceneRecord>(sharedScenes);
        }
    }
}
