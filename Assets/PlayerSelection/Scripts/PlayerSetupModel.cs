using System;
using System.Collections.Generic;
using System.Linq;
using SAS.Core.TagSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerSetupModel : IBindable
{
    IReadOnlyList<PlayerProfile> Players { get; }
    void AddPlayer(PlayerInput playerInput);
    void AddPlayer(PlayerProfile playerProfile);
    void Clear();
    PlayerProfile GetPlayer(int index);
    PlayerProfile GetPlayer(PlayerInput playerInput);
    PlayerProfile GetOtherPlayer(PlayerInput playerInput);
    IReadOnlyList<GameObject> GetPresentEntities();
    GameObject GetEntity(string entityName);
}

public class PlayerSetupModel : IPlayerSetupModel
{
    private readonly List<PlayerProfile> _players = new List<PlayerProfile>();
    public IReadOnlyList<PlayerProfile> Players => _players;

    public PlayerSetupModel(IContextBinder _)
    {
    }

    public void OnInstanceCreated()
    {
    }

    void IPlayerSetupModel.AddPlayer(PlayerInput playerInput)
    {
        if (_players.Exists(p => p.Input?.playerIndex == playerInput?.playerIndex))
            return;

        var playerProfile = new PlayerProfile(playerInput);
        _players.Add(playerProfile);
    }

    void IPlayerSetupModel.AddPlayer(PlayerProfile playerProfile)
    {
        if (playerProfile == null)
            return;

        // If it's a default player with null input, just check by Index
        if (playerProfile.Input == null)
        {
            if (_players.Exists(p => p.Index == playerProfile.Index))
                return;

            _players.Add(playerProfile);
            return;
        }

        // If input exists, defer to playerInput overload
        (this as IPlayerSetupModel).AddPlayer(playerProfile.Input);
    }

    PlayerProfile IPlayerSetupModel.GetPlayer(int index)
    {
        return _players.Find(p => p.Index == index);
    }

    PlayerProfile IPlayerSetupModel.GetPlayer(PlayerInput playerInput)
    {
        if (playerInput == null) return null;
        return _players.Find(p => p.Input?.playerIndex == playerInput.playerIndex);
    }

    PlayerProfile IPlayerSetupModel.GetOtherPlayer(PlayerInput playerInput)
    {
        if (playerInput == null) return null;
        return _players.Find(p => p.Input?.playerIndex != playerInput.playerIndex);
    }

    public IReadOnlyList<GameObject> GetPresentEntities()
    {
        return Players
            .Where(p => p.Character != null && p.Character.activeSelf)
            .Select(p => p.Character)
            .ToList();
    }

    public GameObject GetEntity(string entityName)
    {
        return Players
            .Where(p => p.Character != null && p.Character.activeSelf)
            .FirstOrDefault(c => c.Name.Equals(entityName))?.Character;
    }


    void IPlayerSetupModel.Clear()
    {
        _players.Clear();
    }
}