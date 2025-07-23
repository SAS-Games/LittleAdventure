using SAS.Utilities.TagSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IEntityPresenceProvider
{
    IReadOnlyList<GameObject> GetPresentEntities();
}


public class PlayerEntityProvider : MonoBehaviour, IEntityPresenceProvider
{
    [Inject] private IPlayerSetupModel _playerSetupModel;

    void Awake()
    {
        this.InjectFieldBindings();
    }
    public IReadOnlyList<GameObject> GetPresentEntities()
    {
        return _playerSetupModel.Players
            .Where(p => p.Character != null && p.Character.activeSelf)
            .Select(p => p.Character)
            .ToList();
    }
}
