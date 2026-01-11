using SAS.Core.TagSystem;
using UnityEngine;

public interface IDialogueHandler : IBindable
{
    void EnterDialogueMode(TextAsset inkJSON, GameObject initiator);
    bool DialogueIsPlaying { get; }
    InkExternalMethodRegistry InkExternalMethodRegistry { get; }
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