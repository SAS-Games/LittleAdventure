using System.Collections.Generic;
using SAS.StateMachineCharacterController;
using UniRx;
using Unity.Cinemachine;
using UnityEngine;

struct GamePauseEvent : IEvent
{
    public bool state;
}

struct GameOverEvent : IEvent
{
}

struct LevelCompleteEvent : IEvent
{
}

struct PlayerThreatLevelEvent : IEvent
{
    public GameObject character;
    public int value;
}

public struct GlobalThreatLevelEvent : IEvent
{
    public float averageThreatLevel; // Can be int if you want whole numbers
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject m_PlayerPrefab;
    private bool _gamePaused = false;
    private List<GameObject> _players = new List<GameObject>();

    private void Start()
    {
        var player = m_PlayerPrefab; //Instantiate(m_PlayerPrefab);
        _players.Add(player);

        player.GetComponent<IThreatLevel>().Value.Subscribe(val =>
        {
            EventBus<PlayerThreatLevelEvent>.Raise(new PlayerThreatLevelEvent
            {
                character = player,
                value = val
            });

            UpdateGlobalThreatLevel();
        }).AddTo(player);

        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        (brain.ActiveVirtualCamera as CinemachineCamera).Target.TrackingTarget =
            player.GetComponent<ICameraLookAt>().Target;
        player.GetComponent<IHealthPresenter>().HealthModel.OnDeath.Subscribe(_ => { GameOver(); }).AddTo(this);
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

    public void PauseGame()
    {
        _gamePaused = !_gamePaused;
        EventBus<GamePauseEvent>.Raise(new GamePauseEvent { state = _gamePaused });
    }

    private void GameOver()
    {
        EventBus<GameOverEvent>.Raise(new GameOverEvent { });
    }

    public static void LevelComplete()
    {
        EventBus<LevelCompleteEvent>.Raise(new LevelCompleteEvent() { });
    }
}