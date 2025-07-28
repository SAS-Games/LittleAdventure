using System.Collections.Generic;

public class TagProcessContext
{
    public TagProcessContext(IInkMetaParser metaParser)
    {
        MetaParser = metaParser;
    }

    // public DialogueModel Model { get; set; }
    // public IDialogueWidget Widget { get; set; }
    public IInkMetaParser MetaParser { get; set; }
    // public DialogueConfig Config { get; set; }
    //
    // // Current Processing State
    // public string CurrentSpeakerId { get; set; }
    // public string RawTagValue { get; set; }
    // public Dictionary<string, string> ParsedTags { get; set; }
}