using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlowField/Area Cost Config", fileName = "FlowFieldAreaCosts")]
public class FlowFieldAreaCostConfig : ScriptableObject
{
    public List<Entry> entries = new();

    [Serializable]
    public class Entry
    {
        public string id;
        public byte cost;
    }

    private Dictionary<string, byte> _lookup;

    public byte GetCost(string id)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<string, byte>();
            foreach (var entry in entries)
            {
                _lookup[entry.id] = entry.cost;
            }
        }

        return _lookup.TryGetValue(id, out var cost) ? cost : (byte)3;
    }
}