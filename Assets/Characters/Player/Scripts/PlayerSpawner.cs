using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_PlayerPrefab;
    [FieldRequiresChild] protected SpawnPoint[] _spawnPoints;
    public List<GameObject> Players { get; } = new();
    private EventBinding<CharacterDiedEvent> _OnPlayerDiedEventBinding;
    private int _activePlayersCount;

    private void Awake()
    {
        this.Initialize();
        _OnPlayerDiedEventBinding =
            new EventBinding<CharacterDiedEvent>(playerDiedEvent => OnPlayerDied(playerDiedEvent.character));
        EventBus<CharacterDiedEvent>.Register(_OnPlayerDiedEventBinding);
    }

    public GameObject SpawnPlayer(PlayerProfile playerProfile)
    {
        var player = Instantiate(m_PlayerPrefab);
        player.GetComponent<IInputHandler>().PlayerInput = playerProfile.Input;
        player.SetActive(true);
        Players.Add(player);
        player.GetComponent<FSMCharacterController>().SetPosition(_spawnPoints[Players.Count - 1].transform.position);
        player.GetComponent<IThreatLevel>().Value.Subscribe(val =>
        {
            EventBus<PlayerThreatLevelEvent>.Raise(new PlayerThreatLevelEvent
            {
                character = player,
                value = val
            });

            UpdateGlobalThreatLevel();
        }).AddTo(player);

        _activePlayersCount++;
        return player;
    }

    private void OnPlayerDied(GameObject player)
    {
        _activePlayersCount--;
        player.SetActive(false);
        if (_activePlayersCount <= 0)
            EventBus<GameOverEvent>.Raise(new GameOverEvent { });
    }

    private void UpdateGlobalThreatLevel()
    {
        if (_activePlayersCount == 0)
            return;

        float totalThreat = 0f;

        foreach (var player in Players)
            totalThreat += player.GetComponent<IThreatLevel>().Value.Value;

        float averageThreat = totalThreat / _activePlayersCount;

        EventBus<GlobalThreatLevelEvent>.Raise(new GlobalThreatLevelEvent
        {
            averageThreatLevel = averageThreat
        });
    }

    private void OnDestroy()
    {
        EventBus<CharacterDiedEvent>.Deregister(_OnPlayerDiedEventBinding);
    }
}