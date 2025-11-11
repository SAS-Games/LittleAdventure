using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StringTest
{
    [CreateAssetMenu(menuName = "SAS/ReferenceString List")]
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

#if UNITY_EDITOR
        public void AddEntry(string name)
        {
            entries.Add(new Entry
            {
                guid = Guid.NewGuid().ToString(),
                name = name
            });
        }
#endif

        public string GetNameByGUID(string guid) =>
            entries.Find(e => e.guid == guid)?.name;

        public string GetGUIDByName(string name) =>
            entries.Find(e => e.name == name)?.guid;
    }
}