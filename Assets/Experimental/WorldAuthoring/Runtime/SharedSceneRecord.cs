using System;
using UnityEngine;

namespace SAS.WorldStreaming
{
    /// <summary>
    /// Runtime metadata for an optional shared generated scene.
    /// </summary>
    [Serializable]
    public sealed class SharedSceneRecord
    {
        [SerializeField] private string m_Id = string.Empty;
        [SerializeField] private string m_ScenePath = string.Empty;
        [SerializeField] private bool m_AlwaysLoaded;

        public string Id => m_Id;
        public string ScenePath => m_ScenePath;
        public bool AlwaysLoaded => m_AlwaysLoaded;

        internal void Initialize(string id, string scenePath, bool alwaysLoaded)
        {
            m_Id = id ?? string.Empty;
            m_ScenePath = scenePath ?? string.Empty;
            m_AlwaysLoaded = alwaysLoaded;
        }
    }
}
