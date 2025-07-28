using SAS.Utilities.TagSystem;
using UnityEngine;

public interface IDialogueHandler : IBindable
{
    bool DialogueIsPlaying { get; }
    void EnterDialogueMode(TextAsset inkJSON, Animator emoteAnimator);
}

public struct DialogueStartEvent : IEvent
{
    public IDialogueHandler dialogueHandler;
}
public struct DialogueEndEvent : IEvent
{
    public IDialogueHandler dialogueHandler;
}
