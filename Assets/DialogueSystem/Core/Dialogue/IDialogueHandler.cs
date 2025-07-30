using SAS.Utilities.TagSystem;
using UnityEngine;

public interface IDialogueHandler : IBindable
{
    void EnterDialogueMode(TextAsset inkJSON, Animator emoteAnimator);
    bool DialogueIsPlaying { get; }
}

public struct DialogueStartEvent : IEvent
{
    public IDialogueHandler dialogueHandler;
}
public struct DialogueEndEvent : IEvent
{
    public IDialogueHandler dialogueHandler;
}
