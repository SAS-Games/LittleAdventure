using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_PlayerPrefab;
    [FieldRequiresChild] protected SpawnPoint[] _spawnPoints;
    private List<GameObject> _players = new List<GameObject>();

    private void Awake()
    {
        this.Initialize();
    }

    public GameObject SpawnPlayer(PlayerProfile playerProfile)
    {
        var player = Instantiate(m_PlayerPrefab);
        player.GetComponent<IInputHandler>().PlayerInput = playerProfile.Input;
        player.SetActive(true);
        _players.Add(player);
        player.GetComponent<FSMCharacterController>().SetPosition(_spawnPoints[_players.Count - 1].transform.position);
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
}