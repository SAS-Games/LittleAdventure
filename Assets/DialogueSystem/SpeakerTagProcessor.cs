using System.Collections;
using System.Collections.Generic;

public class SpeakerTagProcessor : BaseTagProcessor
{
    public override IEnumerable<string> SupportedKeys => new[] { "speaker", "char" };

    public override IEnumerator Process(string tagValue, TagProcessContext context)
    {
        var parsed = context.MetaParser.Parse(tagValue);
        var speakerId = parsed["id"];

        if (!context.Model.Speakers.TryGetValue(speakerId, out var speaker))
        {
            speaker = new SpeakerState();
            context.Model.Speakers.Add(speakerId, speaker);
        }

        speaker.UpdateFromTags(parsed);
        context.Widget.UpdateSpeaker(speaker);

        yield return null;
    }
}