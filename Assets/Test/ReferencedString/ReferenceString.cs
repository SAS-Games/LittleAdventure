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
        [SerializeField] private string resolvedName;
        [SerializeField] private ReferenceStringOptions sourceOptions;
        private bool _isResolved = false;
        public string Name
        {
            get
            {
                if (!_isResolved)
                {
                    resolvedName = sourceOptions.GetNameByGUID(guid);
                    _isResolved = true;
                }
                return resolvedName;
            }
        }
        public override string ToString()
        {
            return Name;
        }

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
