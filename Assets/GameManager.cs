using SAS.StateMachineCharacterController;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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

public class GameManager : MonoBehaviour, IReady
{
    [SerializeField] private PlayerSpawner m_PlayerSpawner;
    private bool _gamePaused = false;
    private bool _isReady = false;

    private void Start()
    {
        PlayerInputManager.instance.JoinPlayer(0);
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        var player = input.gameObject;
        SceneUtility.MoveGameObjectToScene(player, gameObject.scene);
        m_PlayerSpawner.SpawnPlayer(player);

        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        (brain.ActiveVirtualCamera as CinemachineCamera).Target.TrackingTarget =
            player.GetComponent<ICameraLookAt>().Target;
        _isReady = true;
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

    public bool IsReady => _isReady;
}