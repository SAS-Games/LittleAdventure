using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseTagProcessor : ITagProcessor
{
    // Default implementation checks against SupportedKeys
    public abstract IEnumerable<string> SupportedKeys { get; }

    public virtual bool CanHandle(string tagKey)
        => SupportedKeys.Contains(tagKey);

    // Must be implemented by concrete processors
    public abstract IEnumerator Process(string tagValue, TagProcessContext context);
}