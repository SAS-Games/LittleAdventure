using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;

public interface IDialogueWidget
{
    event Action OnContinuePressed;
    void ShowDialogue();
    void HideDialogue();
    IEnumerator DisplayLine(string text);
   
    void UpdateSpeaker(SpeakerState speaker);
    public IEnumerator RunOperations(IEnumerable<IEnumerator> operations, Action onComplete);
}