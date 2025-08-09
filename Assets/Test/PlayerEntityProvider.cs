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
    
    IReadOnlyList<GameObject> IEntityPresenceProvider.GetPresentEntities()
    {
        return _playerSetupModel.GetPresentEntities();
    }
}
