using Ink.Runtime;
using SAS.Core.TagSystem;
using System;
using UnityEngine;

public interface IDialogueHandler : IBindable, IInitializable
{
    void EnterDialogueMode(
        TextAsset inkJSON,
        GameObject initiator,
        DialogueMetadataProfile metadataProfile = null);
    void ContinueStory();
    void RequestAdvance();
    void MakeChoice(int choiceIndex);
    void CompleteLinePresentation(DialogueLineContext lineContext);

    bool DialogueIsPlaying { get; }
    SAS.DialogueSystem.DialogueSessionState State { get; }
    InkExternalMethodRegistry InkExternalMethodRegistry { get; }
    Story CurrentStory { get; }
    DialogueLineContext CurrentLineContext { get; }

    event Action<DialogueLineContext> OnLinePresented;
    event Action<DialogueLineContext> OnLineReady;
    event Action<SAS.DialogueSystem.DialogueSessionState> OnStateChanged;
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
