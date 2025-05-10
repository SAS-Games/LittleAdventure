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
    [SerializeField] private PlayerSpawner m_PlayerSpawner;
    private bool _gamePaused = false;

    private void Start()
    {
        var player = m_PlayerSpawner.SpawnPlayer();
        
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        (brain.ActiveVirtualCamera as CinemachineCamera).Target.TrackingTarget =
            player.GetComponent<ICameraLookAt>().Target;
        player.GetComponent<IHealthPresenter>().HealthModel.OnDeath.Subscribe(_ => { GameOver(); }).AddTo(this);
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