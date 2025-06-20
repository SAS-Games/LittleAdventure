using System.Collections;
using System.Collections.Generic;

public interface ITagProcessor
{
    IEnumerable<string> SupportedKeys { get; }
    bool CanHandle(string tagKey);
    IEnumerator Process(string tagValue, TagProcessContext context);
}