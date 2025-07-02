using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerConfigurationUI : UIScreenView
{
    [SerializeField] private PlayerSetupMenuController m_PlayerConfigScreen;
    [SerializeField] private RectTransform m_Content;

    public override void OnButtonClick(UIButton button, BaseEventData eventData)
    {
    }

    public void OnPlayerJoin(PlayerInput playerInput)
    {
        PlayerSetupController.Instance.HandlePlayerJoin(playerInput);
        var menu = Instantiate(m_PlayerConfigScreen, m_Content.transform);
        playerInput.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
        menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(playerInput.playerIndex);
    }
}