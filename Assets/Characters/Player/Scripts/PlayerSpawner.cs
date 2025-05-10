using System.Collections.Generic;
using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UniRx;

public class PlayerSpawner : MonoBase
{
    [Inject] private ITargetRegistry _targetRegistry;
    [SerializeField] private GameObject m_PlayerPrefab;
    private List<GameObject> _players = new List<GameObject>();

    public GameObject SpawnPlayer()
    {
        var player = m_PlayerPrefab; //Instantiate(m_PlayerPrefab);
        _players.Add(player);
        _targetRegistry.RegisterTarget(player.GetComponent<ITarget>());

        player.GetComponent<IThreatLevel>().Value.Subscribe(val =>
        {
            EventBus<PlayerThreatLevelEvent>.Raise(new PlayerThreatLevelEvent
            {
                character = player,
                value = val
            });

            UpdateGlobalThreatLevel();
        }).AddTo(player);

        return player;
    }

    private void UpdateGlobalThreatLevel()
    {
        if (_players.Count == 0)
            return;

        float totalThreat = 0f;

        foreach (var player in _players)
            totalThreat += player.GetComponent<IThreatLevel>().Value.Value;

        float averageThreat = totalThreat / _players.Count;

        EventBus<GlobalThreatLevelEvent>.Raise(new GlobalThreatLevelEvent
        {
            averageThreatLevel = averageThreat
        });
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _targetRegistry.Targets.Clear();
    }
}