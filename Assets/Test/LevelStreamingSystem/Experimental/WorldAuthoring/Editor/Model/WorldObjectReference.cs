using System;
using UnityEditor;
using UnityEngine;

namespace SAS.WorldStreaming.Editor
{
    /// <summary>
    /// Stable editor reference to an object in the source authoring scene.
    /// </summary>
    [Serializable]
    public sealed class WorldObjectReference
    {
        [SerializeField] private string m_GlobalObjectId = string.Empty;
        [SerializeField] private string m_HierarchyPath = string.Empty;

        public string GlobalObjectId => m_GlobalObjectId;
        public string HierarchyPath => m_HierarchyPath;

        public GameObject Resolve()
        {
            if (!UnityEditor.GlobalObjectId.TryParse(
                    m_GlobalObjectId,
                    out UnityEditor.GlobalObjectId id))
                return null;

            UnityEngine.Object resolved =
                UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            if (resolved is GameObject gameObject)
                return gameObject;
            return resolved is Component component ? component.gameObject : null;
        }

        public void SetTarget(GameObject target)
        {
            if (target == null)
            {
                m_GlobalObjectId = string.Empty;
                m_HierarchyPath = string.Empty;
                return;
            }

            m_GlobalObjectId =
                UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
            m_HierarchyPath = BuildHierarchyPath(target.transform);
        }

        private static string BuildHierarchyPath(Transform target)
        {
            string path = target.name;
            while (target.parent != null)
            {
                target = target.parent;
                path = $"{target.name}/{path}";
            }

            return path;
        }
    }
}
