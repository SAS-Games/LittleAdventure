using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerConfigurationUI : UIScreenView
{
    [SerializeField] private PlayerSetupMenu m_PlayerConfigScreen;
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private SceneGroupLoader m_SceneGroupLoader;
    private HashSet<int> _readyPlayerIndices = new HashSet<int>();

    protected override void Awake()
    {
        base.Awake();
        PlayerSetupController.Instance.Clear();
        _readyPlayerIndices.Clear();
    }

    public void OnPlayerJoin(PlayerInput playerInput)
    {
        PlayerSetupController.Instance.HandlePlayerJoin(playerInput);
        var menu = Instantiate(m_PlayerConfigScreen, m_Content.transform);
        playerInput.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
        menu.GetComponent<PlayerSetupMenu>().SetPlayerIndex(playerInput.playerIndex);
    }

    public void OnPlayerReady(int playerIndex)
    {
        _readyPlayerIndices.Add(playerIndex);
        if (_readyPlayerIndices.Count == PlayerSetupController.Instance.MaxPlayers)
        {
            m_SceneGroupLoader.Load();
        }
    }
}