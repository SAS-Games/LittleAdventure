using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "LittleAdventure/Player/Color Configuration", fileName = "ColorConfig")]
public class ColorConfig : ScriptableObject
{
    [System.Serializable]
    public struct NamedColor
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Color Color { get; private set; }
    }

    [SerializeField] private List<NamedColor> m_NamedColors = new();

    private Dictionary<string, Color> _colorLookup;
    public IReadOnlyList<NamedColor> NamedColors => m_NamedColors;

    public IReadOnlyList<string> ColorNames
    {
        get
        {
            if (m_NamedColors == null) return new List<string>();
            return m_NamedColors.ConvertAll(nc => nc.Name);
        }
    }

    public bool TryGetColor(string name, out Color color)
    {
        EnsureLookup();
        return _colorLookup.TryGetValue(name, out color);
    }

    public Color GetColor(string name, Color fallback = default)
    {
        EnsureLookup();
        return _colorLookup.GetValueOrDefault(name, fallback);
    }

    private void EnsureLookup()
    {
        if (_colorLookup != null) return;

        _colorLookup = new Dictionary<string, Color>();
        foreach (var nc in m_NamedColors)
        {
            if (!string.IsNullOrEmpty(nc.Name))
                _colorLookup[nc.Name] = nc.Color;
        }
    }
}