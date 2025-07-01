using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class DialogueModel
{
    public Story CurrentStory { get; private set; }
    public Dictionary<string, SpeakerState> Speakers { get; } = new();
    public DialogueVariables Variables { get; }
    public DialogueConfig Config { get; }
    public IInkMetaParser MetaParser { get; }
    public bool AutoAdvance => Config.autoAdvance;
    public float AutoAdvanceDelay =>Config.autoAdvanceDelay;

    public DialogueModel(DialogueConfig config, IInkMetaParser metaParser, TextAsset globalsJSON = null)
    {
        Config = config;
        MetaParser = metaParser;
        Variables = new DialogueVariables(globalsJSON);
    }

    public void StartStory(TextAsset inkJSON)
    {
        CurrentStory = new Story(inkJSON.text);
        // if (Variables != null)
        //     Variables.StartListening(CurrentStory);
    }

    public string ContinueStory() => CurrentStory.canContinue ? CurrentStory.Continue() : null;
}