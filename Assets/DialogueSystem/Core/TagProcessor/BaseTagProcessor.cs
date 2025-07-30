using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseTagProcessor : MonoBehaviour, ITagProcessor
{
    public IEnumerable<string> SupportedKeys { get; } = new string[] { };

    public virtual bool CanHandle(string tagKey) => SupportedKeys.Contains(tagKey);
    public abstract void Process(string tagValue, TagProcessContext context);

    public virtual void Reset() { }
}