using System.Collections;
using System.Collections.Generic;

public interface ITagProcessor
{
    bool CanHandle(string tagKey);
    void Process(string tagValue, TagProcessContext context);
    void Reset();
}