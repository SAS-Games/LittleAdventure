using SAS.SceneManagement;
using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ICheckpointManager : IBindable, IInitializable, IDestroyable
{
    void RegisterGroup(SpawnPointGroup group);
    void UnregisterGroup(SpawnPointGroup group);
}

public class PlayerCheckpointManager : ICheckpointManager
{
    [Inject] private IPlayerSetupModel _playerSetupModel;
    private string m_ActiveCheckpointGroupID;
    private Dictionary<string, SpawnPointGroup> _groupsByID = new();
    private EventBinding<SceneGroupLoadedEvent> _sceneGroupLoadedEventBinding;

    void OnSceneGroupLoaded(SceneGroupLoadedEvent sceneGroupLoadedEvent)
    {
        foreach (var player in _playerSetupModel.Players)
        {
            var spawnPoint = GetSpawnPointForPlayer(player.Index);
            if (player.Character != null && player.Character.activeSelf)
            {
                if (player.Character.TryGetComponent<FSMCharacterController>(out var characterController))
                    characterController.SetPosition(spawnPoint.transform.position);
            }
        }
    }

    public void RegisterGroup(SpawnPointGroup group)
    {
        if (!string.IsNullOrEmpty(group.SpawnPointGroupID))
        {
            _groupsByID[group.SpawnPointGroupID] = group;
        }
    }

    public void UnregisterGroup(SpawnPointGroup group)
    {
        if (!string.IsNullOrEmpty(group.SpawnPointGroupID) &&
            _groupsByID.TryGetValue(group.SpawnPointGroupID, out var existing) && existing == group)
            _groupsByID.Remove(group.SpawnPointGroupID);
    }

    public void SetActiveCheckpointGroup(string groupID)
    {
        m_ActiveCheckpointGroupID = groupID;
    }

    public SpawnPoint GetSpawnPointForPlayer(int playerIndex)
    {
        SpawnPointGroup group = null;
        if (!string.IsNullOrEmpty(m_ActiveCheckpointGroupID))
        {
            if (_groupsByID.TryGetValue(m_ActiveCheckpointGroupID, out group))
                return group.GetSpawnPointByPlayerId(playerIndex);
        }

        group = _groupsByID.FirstOrDefault().Value;
        return group.GetSpawnPointByPlayerId(playerIndex);
    }

    public PlayerCheckpointManager(IContextBinder contextBinder)
    {

        (contextBinder as Component).Initialize(this);
    }

    void IInitializable.OnCreated()
    {
        _sceneGroupLoadedEventBinding = new EventBinding<SceneGroupLoadedEvent>(OnSceneGroupLoaded);
        EventBus<SceneGroupLoadedEvent>.Register(_sceneGroupLoadedEventBinding);
    }

    void IDestroyable.OnDestroyed()
    {
        EventBus<SceneGroupLoadedEvent>.Deregister(_sceneGroupLoadedEventBinding);
    }
}
