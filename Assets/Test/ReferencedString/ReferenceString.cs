using System;
using UnityEngine;

namespace SAS.StringTest
{
    [Serializable]
    public class ReferenceString : ISerializationCallbackReceiver
    {
        [SerializeField] private string guid;

#if UNITY_EDITOR
        [SerializeField] private string lastKnownName; // Editor-only fallback
#endif

        [SerializeField] private string resolvedName;
        [SerializeField] private ReferenceStringOptions sourceOptions;

        [NonSerialized] private bool _isResolved;

        public string Name
        {
            get
            {
                if (!_isResolved)
                {
                    resolvedName = ResolveName();
                    _isResolved = true;
                }

                return resolvedName;
            }
        }

        private string ResolveName()
        {
            if (string.IsNullOrEmpty(guid) || sourceOptions == null)
            {
#if UNITY_EDITOR
                return lastKnownName ?? string.Empty;
#else
                return string.Empty;
#endif
            }

            var name = sourceOptions.GetNameByGUID(guid);

#if UNITY_EDITOR
            return string.IsNullOrEmpty(name) ? lastKnownName ?? string.Empty : name;
#else
            return name ?? string.Empty;
#endif
        }

        public override string ToString()
        {
            return Name ?? string.Empty;
        }

#if UNITY_EDITOR
        public void Set(string newGuid, string newName, ReferenceStringOptions sourceSO)
        {
            guid = newGuid;
            resolvedName = newName;
            sourceOptions = sourceSO;
            _isResolved = false;

            if (!string.IsNullOrEmpty(newName))
                lastKnownName = newName;
        }

        public string GetLastKnownName() => lastKnownName;
#endif

        // Ensures cache is reset after domain reload / deserialization
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _isResolved = false;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }
}
