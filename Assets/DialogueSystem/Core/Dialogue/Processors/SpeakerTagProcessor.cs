using System.Collections.Generic;

public struct SpeakerState
{
    public string Name;
    public string Image;
    public string Animation;
}

public class SpeakerTagProcessor : BaseTagProcessor
{
    private readonly Dictionary<string, SpeakerState> _speakers = new();
    public IReadOnlyDictionary<string, SpeakerState> Speakers => _speakers;

    public override void Process(string tagValue, TagProcessContext context)
    {
        var parsed = context.MetaParser.Parse(tagValue);

        if (!parsed.TryGetValue("id", out var speakerId))
            return;

        context.CurrentSpeakerId = speakerId;

        var state = new SpeakerState
        {
            Name = parsed.GetValueOrDefault("name"),
            Image = parsed.GetValueOrDefault("image"),
            Animation = parsed.GetValueOrDefault("anim")
        };

        _speakers[speakerId] = state;
    }


    public override void Reset()
    {
        base.Reset();
        _speakers.Clear();
    }
}