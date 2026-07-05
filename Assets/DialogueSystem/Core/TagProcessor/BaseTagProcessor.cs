using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseTagProcessor : MonoBehaviour, ITagProcessor
{
   [field: SerializeField] public List<string> SupportedKeys { get; private set; }
    public virtual bool CanHandle(string tagKey)
    {
        return SupportedKeys != null && SupportedKeys.Any(key => KeyEquals(key, tagKey));
    }

    protected static bool KeyEquals(string lhs, string rhs)
    {
        return string.Equals(lhs, rhs, StringComparison.OrdinalIgnoreCase);
    }

    public abstract void Process(string tagValue, TagProcessContext context);

    public virtual void Reset() { }
}
