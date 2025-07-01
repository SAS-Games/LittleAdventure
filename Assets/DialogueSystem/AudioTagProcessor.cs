using System.Collections;
using System.Collections.Generic;

public class AudioTagProcessor : BaseTagProcessor
{
    public override IEnumerable<string> SupportedKeys { get; } = new string[] { "audio" };

    public override IEnumerator Process(string tagValue, TagProcessContext context)
    {
        // Access configuration
        // float volume = context.Config.defaultVolume;
        //
        // // Access services
        // var audioService = context.ServiceLocator.Get<IAudioService>();
        //
        // // Modify state
        // context.Model.CurrentAudioEffect = tagValue;
        //
        // // Update view
        // context.View.PlaySound(tagValue, volume);
        return default;
    }
}