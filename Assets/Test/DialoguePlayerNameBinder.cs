using Ink.Runtime;
using SAS.DialogueSystem;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialoguePlayerNameBinder : MonoBehaviour
{
    [Inject] private IPlayerSetupModel _playerSetupModel;
    private PlayerInput _interactedPlayerInput;

    private void Awake()
    {
        this.Initialize();
    }

    public void SetPlayerInput(PlayerInput playerInput)
    {
        this._interactedPlayerInput = playerInput;
    }

    private void SetPlayerNames(Story story)
    {
        var playerName = _playerSetupModel.GetPlayer(_interactedPlayerInput)?.DisplayName ?? "Player1";
        var otherPlayerName = _playerSetupModel.GetOtherPlayer(_interactedPlayerInput)?.DisplayName ?? "Player2";

        story.variablesState["Player1_name"] = playerName;
        story.variablesState["Player2_name"] = otherPlayerName;
        story.variablesState["isCoop"] = _playerSetupModel.Players.Count > 1;
    }

    public void OnDialogueStart(DialogueHandler dialogueHandler)
    {
        SetPlayerNames(dialogueHandler.CurrentStory);
    }

    public void OnDialogueEnd(DialogueHandler dialogueHandler)
    {
    }
}