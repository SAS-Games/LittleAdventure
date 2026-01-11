using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SAS/String List")]
public class StringOptions : ScriptableObject
{
    [field: SerializeField] public List<string> Values { get; private set; } = new();
}
