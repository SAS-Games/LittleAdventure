using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseTagProcessor : MonoBehaviour, ITagProcessor
{
   [field: SerializeField] public List<string> SupportedKeys { get; private set; }
    public virtual bool CanHandle(string tagKey) => SupportedKeys.Contains(tagKey);
    public abstract void Process(string tagValue, TagProcessContext context);

    public virtual void Reset() { }
}