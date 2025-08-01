using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DialogueTrigger))]
public class DialogueProximityActivator : ProximityInputActivator
{
    private DialogueTrigger _dialogueTrigger;

    private void Awake()
    {
        _dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    protected override void OnInputPerformed(InputAction.CallbackContext context)
    {
        _dialogueTrigger.ShowDialogue();
    }
}
