using SAS.Utilities.TagSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SceneGroupLoader), typeof(OnTriggerHandler), typeof(IEntityPresenceProvider))]
public class SceneGroupTrigger : MonoBehaviour
{
    [FieldRequiresSelf] IEntityPresenceProvider _activePlayerProvider;
    private readonly HashSet<GameObject> _playersInside = new();
    private bool _sceneLoadingTriggered = false;

    private void Awake()
    {
        this.Initialize();
    }

    public void NotifyPlayerEntered(GameObject player)
    {
        if (!GetPresentEntities().Contains(player)) return;

        _playersInside.Add(player);
        CheckAllPlayersInside();
    }

    public void NotifyPlayerExited(GameObject player)
    {
        _playersInside.Remove(player);
    }

    private void CheckAllPlayersInside()
    {
        if (_sceneLoadingTriggered) return;

        var activePlayers = GetPresentEntities();
        if (activePlayers.Count == 0) return;

        if (activePlayers.All(p => _playersInside.Contains(p)))
        {
            _sceneLoadingTriggered = true;
            GetComponent<SceneGroupLoader>().Load();
        }
    }

    private IReadOnlyList<GameObject> GetPresentEntities()
    {
        return _activePlayerProvider.GetPresentEntities() ?? new List<GameObject>();
    }
}
