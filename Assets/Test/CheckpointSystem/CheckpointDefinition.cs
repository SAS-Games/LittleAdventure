using System;
using UnityEngine;

namespace SAS.Checkpoints
{
    [Serializable]
    public sealed class CheckpointDefinition
    {
        [SerializeField] private string m_Id;
        [SerializeField] private string m_DisplayName;
        [SerializeField] private int m_Order;
        public string Id => m_Id;
        public string DisplayName => m_DisplayName;
        public int Order => m_Order;
        public bool IsValid => !string.IsNullOrWhiteSpace(m_Id);
    }
}