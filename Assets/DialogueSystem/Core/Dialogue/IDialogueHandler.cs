using Ink.Runtime;
using SAS.Core.TagSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueHandler : IBindable, IInitializable
{
    void EnterDialogueMode(TextAsset inkJSON, GameObject initiator);
    void ContinueStory();
    void MakeChoice(int choiceIndex);
    DialogueLineContext CreateLineContext(string lineText, List<string> currentTags);

    bool DialogueIsPlaying { get; }
    InkExternalMethodRegistry InkExternalMethodRegistry { get; }
    Story CurrentStory { get; }
    DialogueLineContext CurrentLineContext { get; }

    event Action<Story> OnStoryMessageShown;
    event Action<Story, DialogueLineContext> OnLineMessageShown;
    event Action<string> OnStoryContinue;
    event Action<DialogueLineContext> OnLineReady;
    event Action OnEnterDialogueMode;
    event Action OnExitDialogueMode;
    event Action OnSkipRequested;
}

public struct DialogueStartEvent : IEvent
{
    public IDialogueHandler dialogueHandler;
    public GameObject initiator;
}

public struct DialogueEndEvent : IEvent
{
    public IDialogueHandler dialogueHandler;
    public GameObject initiator;
}
