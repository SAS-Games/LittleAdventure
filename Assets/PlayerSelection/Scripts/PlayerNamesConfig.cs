using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "LittleAdventure/Player/Name Configuration", fileName = "PlayerNamesConfig")]
public class PlayerNamesConfig : ScriptableObject
{
    [SerializeField] private List<string> m_AvailableNames = new();
    public IReadOnlyList<string> AvailableNames => m_AvailableNames;
}