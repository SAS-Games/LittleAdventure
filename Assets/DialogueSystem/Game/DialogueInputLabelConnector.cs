using SAS.DialogueSystem;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueInputLabelConnector : MonoBehaviour
{
    [Inject] private IDialogueHandler _dialogueHandler;
    void Start()
    {
        this.Initialize();
    }

    public void UpdateControlLabels(PlayerInput playerInput)
    {
        var story = ((DialogueHandler)_dialogueHandler).CurrentStory;
        InkInputLabelSetter.SetControlLabelsFromPlayerInput(playerInput, story);
    }
}