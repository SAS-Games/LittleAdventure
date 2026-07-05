using Ink.Runtime;
using SAS.DialogueSystem;
using SAS.Core.TagSystem;
using SAS.WeaponSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialoguePlayerDataBinder : MonoBehaviour
{
    [Inject] private IPlayerSetupModel _playerSetupModel;
    private PlayerInput _interactedPlayerInput;

    private void Awake()
    {
        this.Initialize();
    }

    public void SetInteractedPlayerInput(PlayerInput playerInput)
    {
        _interactedPlayerInput = playerInput;
    }

    public void BindPlayerData(DialogueHandler dialogueHandler)
    {
        BindPlayerDataToStory(dialogueHandler.CurrentStory);
    }

    private void BindPlayerDataToStory(Story story)
    {
        var player = _playerSetupModel.GetPlayer(_interactedPlayerInput);
        var otherPlayer = _playerSetupModel.GetOtherPlayer(_interactedPlayerInput);

        bool isCoop = _playerSetupModel.GetPresentEntities().Count > 1;

        story.variablesState["Player1_name"] = player?.Name ?? player?.IndexedName;
        if (otherPlayer != null)
            story.variablesState["Player2_name"] = otherPlayer.Name ?? otherPlayer?.IndexedName;
        story.variablesState["isCoop"] = isCoop;

        story.variablesState["player1_coins"] = GetCoins(player?.Character);
        story.variablesState["player2_coins"] = isCoop ? GetCoins(otherPlayer?.Character) : 0;
    }

    private int GetCoins(GameObject playerObj)
    {
        var currencyPresenter = playerObj.GetComponent<ICurrencyPresenter>();
        return currencyPresenter.GetValue();
    }

    private void ApplyHealthBonus(string playerName, int cost, int bonusHealth)
    {
        var playerObj = _playerSetupModel.GetEntity(playerName);
        var currencyPresenter = playerObj.GetComponent<ICurrencyPresenter>();
        currencyPresenter.SetValue(currencyPresenter.GetValue() - cost);
        var healthPresenter = playerObj.GetComponent<StatPresenter<IHealthModel>>();
        healthPresenter.IncreaseMax(bonusHealth);
    }
    
    private void UpgradeAttackDamage(string playerName, int cost, int bonusHealth)
    {
        var playerObj = _playerSetupModel.GetEntity(playerName);
        var currencyPresenter = playerObj.GetComponent<ICurrencyPresenter>();
        currencyPresenter.SetValue(currencyPresenter.GetValue() - cost);
        //var healthPresenter = playerObj.GetComponent<StatPresenter<IHealthModel>>();
        //healthPresenter.IncreaseMax(bonusHealth);
    }
    
    private void UnlockWeapon(string playerName, string weaponName)
    {
        var playerObj = _playerSetupModel.GetEntity(playerName);
        //playerObj.GetComponent<WeaponInventory>().TrySetWeapon(WeaponInventory.WeaponSlot.Primary, weaponName, out _);
    }

    public void RegisterGrantHealthMethod(DialogueHandler dialogueHandler)
    {
        dialogueHandler.InkExternalMethodRegistry.Register<string, int, int>("grant_health", ApplyHealthBonus);
    }

    public void UnregisterGrantHealthMethod(DialogueHandler dialogueHandler)
    {
        dialogueHandler.InkExternalMethodRegistry.Unregister("grant_health");
    }
    
    public void RegisterGrantWeaponMethod(DialogueHandler dialogueHandler)
    {
        dialogueHandler.InkExternalMethodRegistry.Register<string, string>("grant_weapon", UnlockWeapon);
    }

    public void UnregisterGrantWeaponMethod(DialogueHandler dialogueHandler)
    {
        dialogueHandler.InkExternalMethodRegistry.Unregister("grant_weapon");
    }
    
    public void RegisterUpgradeAttackDamageMethod(DialogueHandler dialogueHandler)
    {
        dialogueHandler.InkExternalMethodRegistry.Register<string, int, int>("grant_damage", UpgradeAttackDamage);
    }

    public void UnregisterUpgradeAttackDamageMethod(DialogueHandler dialogueHandler)
    {
        dialogueHandler.InkExternalMethodRegistry.Unregister("grant_damage");
    } 
    public void UpdateControlLabels(IDialogueHandler dialogueHandler)
    {
        var story = ((DialogueHandler)dialogueHandler).CurrentStory;
        InkInputLabelSetter.SetControlLabelsFromPlayerInput(_interactedPlayerInput, story);
    }
}