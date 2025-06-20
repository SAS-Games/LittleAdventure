using System.Collections;
using System.Collections.Generic;

public class TagProcessor
{
    private readonly Dictionary<string, ITagProcessor> _processors = new();

    public TagProcessor Add(ITagProcessor processor)
    {
        foreach (var key in processor.SupportedKeys)
            _processors[key] = processor;
        return this;
    }

    public IEnumerator Process(List<string> tags, TagProcessContext context)
    {
        foreach (var tag in tags)
        {
            var split = tag.Split(new[] { ':' }, 2);
            if (split.Length != 2) continue;

            var key = split[0].Trim();
            var value = split[1].Trim();

            if (_processors.TryGetValue(key, out var processor))
                yield return processor.Process(value, context);
        }
    }
}