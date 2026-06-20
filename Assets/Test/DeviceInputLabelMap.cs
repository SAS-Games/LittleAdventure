using System;
using System.Collections.Generic;
using SAS.StringTest;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputDeviceUtils;

[CreateAssetMenu(menuName = "LittleAdventure/Device Label Map", fileName = "NewDeviceLabelMap")]
public class DeviceInputLabelMap : ScriptableObject
{
    [SerializeField] private ReferenceStringOptions m_StringOptions;

    [Serializable]
    public class LabelEntry
    {
        [SerializeField] private ReferenceString m_Key;
        [SerializeField] private ReferenceString m_DefaultLabel;
        public ReferenceString Key => m_Key;

        [Serializable]
        public class DeviceOverride
        {
            public InputDeviceType Device;
            [SerializeField] public ReferenceString Label;
        }

        [SerializeField]
        private List<DeviceOverride> m_DeviceOverrides = new();

        public string GetLabel(InputDeviceType device)
        {
            foreach (var overrideEntry in m_DeviceOverrides)
            {
                if (overrideEntry.Device == device)
                    return overrideEntry.Label.Value;
            }

            return m_DefaultLabel.Value;
        }
    }

    [SerializeField] private List<LabelEntry> m_Entries = new();
    private Dictionary<string, LabelEntry> _lookup;

    private void OnEnable()
    {
        RebuildLookup();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildLookup();
    }
#endif

    private void RebuildLookup()
    {
        _lookup = new Dictionary<string, LabelEntry>();

        foreach (var entry in m_Entries)
        {
            if (entry?.Key == null)
                continue;

            string guid = entry.Key.Guid;

            if (string.IsNullOrEmpty(guid))
                continue;

            _lookup[guid] = entry;
        }
    }

    public string GetLabel(InputDeviceType deviceType, ReferenceString key)
    {
        if (key == null)
            return null;

        if (_lookup == null)
            RebuildLookup();

        if (string.IsNullOrEmpty(key.Guid))
            return null;

        return _lookup.TryGetValue(key.Guid, out var entry)
            ? entry.GetLabel(deviceType)
            : null;
    }

    public string GetLabel(PlayerInput playerInput, ReferenceString key)
    {
        return GetLabel(playerInput.GetActiveDevice(), key);
    }
}