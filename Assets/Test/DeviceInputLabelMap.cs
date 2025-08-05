using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputDeviceUtils;

[CreateAssetMenu(menuName = "LittleAdventure/Device Label Map", fileName = "NewDeviceLabelMap")]
public class DeviceInputLabelMap : ScriptableObject
{

    [SerializeField] private StringOptions m_StringOptions;

    [Serializable]
    public class LabelEntry
    {
        [field: SerializeField, StringDropdown(nameof(m_StringOptions))] public string Key { get; private set; }

        [SerializeField] private string m_DefaultLabel;

        [Serializable]
        public class DeviceOverride
        {
            public InputDeviceType Device;
            public string Label;
        }

        [SerializeField] private List<DeviceOverride> m_DeviceOverrides = new();

        public string GetLabel(InputDeviceType device)
        {
            foreach (var overrideEntry in m_DeviceOverrides)
            {
                if (overrideEntry.Device == device)
                    return overrideEntry.Label;
            }
            return m_DefaultLabel;
        }
    }


    [SerializeField] private List<LabelEntry> m_Entries = new();

    private Dictionary<string, LabelEntry> _lookup;

    private void OnEnable()
    {
        _lookup = new Dictionary<string, LabelEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in m_Entries)
        {
            if (!string.IsNullOrWhiteSpace(entry.Key))
                _lookup[entry.Key] = entry;
        }
    }

    public string GetLabel(InputDeviceType deviceType, string key)
    {
        if (_lookup == null)
            OnEnable();

        return _lookup.TryGetValue(key, out var entry)
            ? entry.GetLabel(deviceType)
            : null;
    }

    public string GetLabel(PlayerInput playerInput, string key)
    {
        if (_lookup == null)
            OnEnable();
        return GetLabel(playerInput.GetActiveDevice(), key);
    }
}
