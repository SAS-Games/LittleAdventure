using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StringTest
{
    [CreateAssetMenu(menuName = "SAS/ReferenceString List")]
    [LockedInspector]
    public class ReferenceStringOptions : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string name;
            [ReadOnly] public string guid;
        }

        [SerializeField] private List<Entry> entries = new();
        public List<Entry> Entries => entries;

        // ===============================
        // FAST LOOKUP CACHE
        // ===============================

        Dictionary<string, string> guidToName;
        Dictionary<string, string> nameToGuid;
        bool cacheValid;

        void OnEnable()
        {
            RebuildCache();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // Called when asset changes in inspector
            RebuildCache();
        }
#endif

        public void RebuildCache()
        {
            if (entries == null)
                return;

            guidToName = new Dictionary<string, string>(entries.Count);
            nameToGuid = new Dictionary<string, string>(entries.Count);

            foreach (var e in entries)
            {
                if (string.IsNullOrEmpty(e.guid))
                    continue;

                guidToName[e.guid] = e.name ?? string.Empty;

                // avoid duplicate crash
                if (!string.IsNullOrEmpty(e.name))
                    nameToGuid[e.name] = e.guid;
            }

            cacheValid = true;
        }

        void EnsureCache()
        {
            if (!cacheValid || guidToName == null)
                RebuildCache();
        }

        // ===============================
        // FAST LOOKUPS (O(1))
        // ===============================

        public string GetNameByGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return null;

            EnsureCache();

            guidToName.TryGetValue(guid, out var name);
            return name;
        }

        public string GetGUIDByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            EnsureCache();

            nameToGuid.TryGetValue(name, out var guid);
            return guid;
        }

#if UNITY_EDITOR
        public void AddEntry(string name)
        {
            entries.Add(new Entry
            {
                guid = Guid.NewGuid().ToString(),
                name = name
            });

            RebuildCache();
        }
#endif
    }
}