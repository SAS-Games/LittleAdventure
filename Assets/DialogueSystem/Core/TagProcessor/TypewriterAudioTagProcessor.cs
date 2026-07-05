using UnityEngine;

public class TypewriterAudioTagProcessor : BaseTagProcessor
{
    public override bool CanHandle(string tagKey)
    {
        return KeyEquals(tagKey, "audio") || base.CanHandle(tagKey);
    }

    public override void Process(string tagValue, TagProcessContext context)
    {
        context.CurrentLine.SetAudioInfo(tagValue);
    }
}
