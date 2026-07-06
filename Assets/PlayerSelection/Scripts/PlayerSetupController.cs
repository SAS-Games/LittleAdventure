using SAS.Core;
using SAS.Core.TagSystem;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSetupController : Singleton<PlayerSetupController>
{
    [SerializeField] private PlayerInput m_DefaultPlayer;
    [SerializeField] private int m_MaxPlayers = 2;
    [Inject] private IPlayerSetupModel _playerSetupModel;
    public int MaxPlayers => m_MaxPlayers;

    protected override void Awake()
    {
        base.Awake();
        this.Initialize();
    }

    public void HandlePlayerJoin(PlayerInput playerInput)
    {
        Debug.Log("player joined " + playerInput.playerIndex);
        playerInput.transform.name = playerInput.transform.name.Replace("(Clone)", $" {playerInput.playerIndex}");
        playerInput.transform.SetParent(transform);

        if (_playerSetupModel.Players.All(p => p.Index != playerInput.playerIndex))
        {
            _playerSetupModel.AddPlayer(playerInput);
        }
    }

    public void Clear()
    {
        foreach (var player in _playerSetupModel.Players)
        {
            if (player.Input && player.Input.gameObject)
                Destroy(player.Input.gameObject);
        }

        _playerSetupModel.Clear();
    }

    public void AddDefaultPlayer()
    {
        var defaultPlayer = Instantiate(m_DefaultPlayer.gameObject);
        HandlePlayerJoin(defaultPlayer.GetComponent<PlayerInput>());
    }
}