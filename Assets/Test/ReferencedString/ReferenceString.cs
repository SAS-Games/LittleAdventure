using System;
using UnityEngine;

namespace SAS.StringTest
{
    [Serializable]
    public class ReferenceString
    {
        [SerializeField] private string guid;
#if UNITY_EDITOR
        [SerializeField] private string lastKnownName; // For missing keys in editor
#endif
        [SerializeField] private string resolvedName; // Baked string used at runtime
        [SerializeField] private ReferenceStringOptions sourceOptions; // Optional custom SO reference
        private bool _isResolved = false;
        public string GUID => guid;
        public string Name => _isResolved? resolvedName:sourceOptions.GetNameByGUID(guid); // No runtime lookup needed

#if UNITY_EDITOR
        public void Set(string newGuid, string newName, ReferenceStringOptions sourceSO)
        {
            guid = newGuid;
            resolvedName = newName;
            sourceOptions = sourceSO;

            if (!string.IsNullOrEmpty(newName))
                lastKnownName = newName;
        }
#endif

#if UNITY_EDITOR
        public string GetLastKnownName() => lastKnownName;
#endif
    }
}
