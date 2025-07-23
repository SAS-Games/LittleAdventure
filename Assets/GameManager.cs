using System.Threading.Tasks;
using SAS.SceneManagement;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Utilities.TagSystem;
using Unity.Cinemachine;
using UnityEngine;
using Debug = SAS.Debug;

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
    private const string Tag = "GameManager";
    private EventBinding<SceneGroupLoadedEvent> _sceneGroupLoadedEventBinding;
    private EventBinding<SceneGroupLoadStartEvent> _sceneGroupLoadStartEventBinding;

    [SerializeField] private PlayerSpawner m_PlayerSpawner;
    [Inject] private IPlayerSetupModel _playerSetupModel;
    private bool _gamePaused = false;
    private bool _isReady = false;

    private void Start()
    {
        this.Initialize();
        _sceneGroupLoadedEventBinding = new EventBinding<SceneGroupLoadedEvent>(OnSceneGroupLoaded);
        _sceneGroupLoadStartEventBinding = new EventBinding<SceneGroupLoadStartEvent>(OnSceneGroupLoadStart);
        EventBus<SceneGroupLoadedEvent>.Register(_sceneGroupLoadedEventBinding);
        EventBus<SceneGroupLoadStartEvent>.Register(_sceneGroupLoadStartEventBinding);
        foreach (PlayerProfile player in _playerSetupModel.Players)
        {
            SpawnPlayer(player);
        }
    }

    private void SpawnPlayer(PlayerProfile playerProfile)
    {
        var player = m_PlayerSpawner.SpawnPlayer(playerProfile);
        SceneUtility.MoveGameObjectToScene(player, gameObject.scene);
    }
    private void OnSceneGroupLoadStart(SceneGroupLoadStartEvent sceneGroupLoadStartEvent)
    {
        foreach (var player in m_PlayerSpawner.Players)
            player.GetComponent<Actor>().enabled = false;
    }

    private async void OnSceneGroupLoaded(SceneGroupLoadedEvent sceneGroupLoadedEvent)
    {
        _isReady = false;

        await CameraTargetSetupAsync();
        foreach (var player in m_PlayerSpawner.Players)
            player.GetComponent<Actor>().enabled = true;

        _isReady = true;
    }

    private async Task CameraTargetSetupAsync()
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogWarning("CinemachineBrain not found on main camera.", Tag);
            return;
        }

        await WaitUntilCameraIsReady(brain);

        var virtualCamera = brain.ActiveVirtualCamera as CinemachineCamera;
        if (virtualCamera == null)
        {
            Debug.LogWarning("ActiveVirtualCamera is not a CinemachineCamera.", Tag);
            return;
        }

        var targetGroup = virtualCamera.Target.TrackingTarget;
        var cinemachineTargetGroup = targetGroup.GetComponent<CinemachineTargetGroup>();
        if (cinemachineTargetGroup == null)
        {
            Debug.LogWarning("CinemachineTargetGroup not found on TrackingTarget.", Tag);
            return;
        }

        foreach (var player in m_PlayerSpawner.Players)
        {
            var lookAtTarget = player.GetComponent<ICameraLookAt>()?.Target;
            if (lookAtTarget == null)
            {
                Debug.LogWarning("ICameraLookAt.Target is null.", Tag);
                return;
            }

            cinemachineTargetGroup.AddMember(lookAtTarget, 0.5f, 1f);
        }

        GetComponent<CameraManager>().enabled = m_PlayerSpawner.Players.Count == 1;
    }

    private async Task WaitUntilCameraIsReady(CinemachineBrain brain)
    {
        while (brain.ActiveVirtualCamera == null || brain.IsBlending)
            await Task.Yield();
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

    void OnDestroy()
    {
        EventBus<SceneGroupLoadedEvent>.Deregister(_sceneGroupLoadedEventBinding);
        EventBus<SceneGroupLoadStartEvent>.Deregister(_sceneGroupLoadStartEventBinding);
    }
}