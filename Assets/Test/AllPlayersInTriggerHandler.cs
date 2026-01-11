using SAS.Core.TagSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(OnTriggerHandler), typeof(IEntityPresenceProvider))]
public abstract class AllPlayersInTriggerHandler : MonoBehaviour
{
    [FieldRequiresSelf] protected IEntityPresenceProvider _presenceProvider;
    protected readonly HashSet<GameObject> _playersInside = new();
    private bool _actionTriggered = false;

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
        if (_actionTriggered) return;

        var activePlayers = GetPresentEntities();
        if (activePlayers.Count == 0) return;

        if (activePlayers.All(p => _playersInside.Contains(p)))
        {
            _actionTriggered = true;
            OnAllPlayersInside();
        }
    }

    protected abstract void OnAllPlayersInside();

    private IReadOnlyList<GameObject> GetPresentEntities()
    {
        return _presenceProvider.GetPresentEntities() ?? new List<GameObject>();
    }
}
