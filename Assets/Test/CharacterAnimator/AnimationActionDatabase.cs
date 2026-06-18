using System.Collections.Generic;
using SAS.StringTest;
using UnityEngine;

[CreateAssetMenu(menuName = "Animation/Action Database")]
public class AnimationActionDatabase : ScriptableObject
{
    public List<AnimationActionConfig> actions;

    Dictionary<string, AnimationActionConfig> _lookup;

    public void Initialize()
    {
        _lookup = new();
        foreach (var a in actions)
            _lookup[a.actionId.Value] = a;
    }

    public AnimationActionConfig Get(string id) => _lookup[id];
}