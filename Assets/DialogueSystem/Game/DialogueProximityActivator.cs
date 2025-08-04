using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(DialogueTrigger))]
public class DialogueProximityActivator : ProximityInputActivator
{
    [SerializeField] private UnityEvent<PlayerInput> m_OnInteract;
    [SerializeField] private UnityEvent m_OnDialogueEnd;
    private DialogueTrigger _dialogueTrigger;

    private void Awake()
    {
        _dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    protected override void OnInputPerformed(InputAction.CallbackContext context, PlayerInput playerInput)
    {
        _dialogueTrigger.ShowDialogue();
        m_OnInteract?.Invoke(playerInput);
    }
}
