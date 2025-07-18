using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
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

public class GameManager : MonoBehaviour, IReady
{
    [SerializeField] private PlayerSpawner m_PlayerSpawner;
    [Inject] private IPlayerSetupModel _playerSetupModel;
    private bool _gamePaused = false;
    private bool _isReady = false;

    private void Start()
    {
        this.Initialize();
        foreach (PlayerProfile player in _playerSetupModel.Players)
        {
            SpawnPlayer(player);
        }
        _isReady = true;
    }

    private void SpawnPlayer(PlayerProfile playerProfile)
    {
        var player = m_PlayerSpawner.SpawnPlayer(playerProfile);
        SceneUtility.MoveGameObjectToScene(player, gameObject.scene);

        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        var targetGroup = (brain.ActiveVirtualCamera as CinemachineCamera).Target.TrackingTarget;
        var cinemachineTargetGroup = targetGroup.GetComponent<CinemachineTargetGroup>();
        cinemachineTargetGroup.AddMember(player.GetComponent<ICameraLookAt>().Target, 0.5f, 1);
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