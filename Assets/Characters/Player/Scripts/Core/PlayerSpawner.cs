using System;
using SAS.StateMachineCharacterController;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_PlayerPrefab;
    public List<GameObject> Players { get; } = new();

    public event Action<GameObject> PlayerDied;
    public event Action<GameObject, int> PlayerThreatLevelChanged;
    public event Action<float> GlobalThreatLevelChanged;
    public event Action AllPlayersDied;

    private EventBinding<CharacterDiedEvent> _OnPlayerDiedEventBinding;
    private int _activePlayersCount;

    private void Awake()
    {
        _OnPlayerDiedEventBinding =
            new EventBinding<CharacterDiedEvent>(playerDiedEvent => OnPlayerDied(playerDiedEvent.character));
        EventBus<CharacterDiedEvent>.Register(_OnPlayerDiedEventBinding);
    }

    public GameObject SpawnPlayer(PlayerInput playerInput = null)
    {
        var player = Instantiate(m_PlayerPrefab);
        player.transform.position = Vector3.one * -1000;

        if (player.TryGetComponent<IInputHandler>(out var inputHandler))
            inputHandler.PlayerInput = playerInput;

        player.SetActive(true);
        Players.Add(player);

        var threatLevel = player.GetComponent<IThreatLevel>();
        if (threatLevel != null)
        {
            threatLevel.Value.Subscribe(val =>
            {
                PlayerThreatLevelChanged?.Invoke(player, val);
                UpdateGlobalThreatLevel();
            }).AddTo(player);
        }

        _activePlayersCount++;
        return player;
    }

    private void OnPlayerDied(GameObject player)
    {
        _activePlayersCount = Mathf.Max(0, _activePlayersCount - 1);
        player.SetActive(false);
        PlayerDied?.Invoke(player);

        if (_activePlayersCount <= 0)
            AllPlayersDied?.Invoke();
    }

    private void UpdateGlobalThreatLevel()
    {
        if (_activePlayersCount == 0)
            return;

        float totalThreat = 0f;

        foreach (var player in Players)
            totalThreat += player.GetComponent<IThreatLevel>().Value.Value;

        float averageThreat = totalThreat / _activePlayersCount;

        GlobalThreatLevelChanged?.Invoke(averageThreat);
    }

    private void OnDestroy()
    {
        EventBus<CharacterDiedEvent>.Deregister(_OnPlayerDiedEventBinding);
    }
}
