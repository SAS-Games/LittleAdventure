using SAS.Utilities;
using SAS.Utilities.TagSystem;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSetupController : Singleton<PlayerSetupController>
{
    [SerializeField] private int m_MaxPlayers = 2;
    [Inject] private IPlayerSetupModel _playerSetupModel;

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
            Destroy(player.Input.gameObject);
        }
        _playerSetupModel.Clear();
    }

    public void AddDefaultPlayer()
    {
        _playerSetupModel.AddPlayer(new PlayerProfile(null,"DefaultPlayer", 0));
    }

    // public void ReadyPlayer(int index)
    // {
    //     playerConfigs[index].IsReady = true;
    //     if (playerConfigs.Count == MaxPlayers && playerConfigs.All(p => p.IsReady == true))
    //     {
    //         SceneManager.LoadScene("SampleScene");
    //     }
    // }
}