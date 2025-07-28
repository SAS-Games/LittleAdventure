using System.Collections;
using System.Collections.Generic;
using System.Linq;using UnityEngine;

public abstract class BaseTagProcessor : MonoBehaviour, ITagProcessor
{
    public abstract IEnumerable<string> SupportedKeys { get; }
    public virtual bool CanHandle(string tagKey) => SupportedKeys.Contains(tagKey);
    public abstract void Process(string tagValue, TagProcessContext context);
}