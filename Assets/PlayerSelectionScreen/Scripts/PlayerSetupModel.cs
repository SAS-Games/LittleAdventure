using System.Collections.Generic;
using SAS.Utilities.TagSystem;
using UnityEngine.InputSystem;

public interface IPlayerSetupModel : IBindable
{
    IReadOnlyList<PlayerProfile> Players { get; }
    void AddPlayer(PlayerInput playerInput);
    PlayerProfile GetPlayer(int index);
}

public class PlayerSetupModel : IPlayerSetupModel
{
    private readonly List<PlayerProfile> _players = new List<PlayerProfile>();
    public IReadOnlyList<PlayerProfile> Players => _players;

    public PlayerSetupModel(IContextBinder _)
    {
    }

    void IPlayerSetupModel.AddPlayer(PlayerInput playerInput)
    {
        if (_players.Exists(p => p.Input.playerIndex == playerInput.playerIndex))
            return;

        var playerProfile = new PlayerProfile(playerInput);
        _players.Add(playerProfile);
    }

    PlayerProfile IPlayerSetupModel.GetPlayer(int index)
    {
        return _players.Find(p => p.Index == index);
    }

    public void OnInstanceCreated()
    {
    }
}