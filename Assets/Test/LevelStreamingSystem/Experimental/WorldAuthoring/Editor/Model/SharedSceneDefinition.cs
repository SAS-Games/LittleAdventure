using System;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    [Serializable]
    public sealed class SharedSceneDefinition
    {
        [SerializeField, HideInInspector] private string m_Id = string.Empty;
        [SerializeField] private string m_DisplayName = "Shared";
        [SerializeField] private string m_SceneName = "Shared";
        [SerializeField] private bool m_AlwaysLoaded;

        public string Id => m_Id;
        public string DisplayName => m_DisplayName;
        public string SceneName => m_SceneName;
        public bool AlwaysLoaded => m_AlwaysLoaded;

        internal void Sanitize(int index)
        {
            if (string.IsNullOrWhiteSpace(m_Id))
                m_Id = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(m_DisplayName))
                m_DisplayName = $"Shared {index + 1}";
            if (string.IsNullOrWhiteSpace(m_SceneName))
                m_SceneName = $"Shared_{index + 1}";
        }
    }
}
