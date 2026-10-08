using System;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    /// <summary>
    /// Regeneration-safe record of one source authoring object and its assignment.
    /// </summary>
    [Serializable]
    public sealed class WorldObjectAuthoringRecord
    {
        [SerializeField] private string m_GlobalObjectId = string.Empty;
        [SerializeField] private string m_HierarchyPath = string.Empty;
        [SerializeField] private string m_OriginalScenePath = string.Empty;
        [SerializeField] private string m_OriginalParentGlobalObjectId = string.Empty;
        [SerializeField] private int m_OriginalSiblingIndex;
        [SerializeField] private Vector3 m_OriginalLocalPosition;
        [SerializeField] private Quaternion m_OriginalLocalRotation = Quaternion.identity;
        [SerializeField] private Vector3 m_OriginalLocalScale = Vector3.one;
        [SerializeField] private string m_AssignedRegionId = string.Empty;
        [SerializeField] private string m_AssignedSharedSceneId = string.Empty;
        [SerializeField] private string m_GeneratedScenePath = string.Empty;
        [SerializeField] private WorldObjectClassification m_Classification;
        [SerializeField] private StreamingAssignmentMode m_AssignmentMode;
        [SerializeField] private WorldObjectAssignmentSource m_AssignmentSource;
        [SerializeField] private Bounds m_CalculatedBounds;
        [SerializeField] private bool m_HasValidBounds;
        [SerializeField] private bool m_IsManualOverride;
        [SerializeField] private bool m_IsCrossRegion;

        public string GlobalObjectId => m_GlobalObjectId;
        public string HierarchyPath => m_HierarchyPath;
        public string OriginalScenePath => m_OriginalScenePath;
        public string OriginalParentGlobalObjectId => m_OriginalParentGlobalObjectId;
        public int OriginalSiblingIndex => m_OriginalSiblingIndex;
        public Vector3 OriginalLocalPosition => m_OriginalLocalPosition;
        public Quaternion OriginalLocalRotation => m_OriginalLocalRotation;
        public Vector3 OriginalLocalScale => m_OriginalLocalScale;
        public string AssignedRegionId => m_AssignedRegionId;
        public string AssignedSharedSceneId => m_AssignedSharedSceneId;
        public string GeneratedScenePath => m_GeneratedScenePath;
        public WorldObjectClassification Classification => m_Classification;
        public StreamingAssignmentMode AssignmentMode => m_AssignmentMode;
        public WorldObjectAssignmentSource AssignmentSource => m_AssignmentSource;
        public Bounds CalculatedBounds => m_CalculatedBounds;
        public bool HasValidBounds => m_HasValidBounds;
        public bool IsManualOverride => m_IsManualOverride;
        public bool IsCrossRegion => m_IsCrossRegion;
    }
}
