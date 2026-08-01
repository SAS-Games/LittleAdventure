using System.Threading;
using System.Threading.Tasks;
using SAS.SceneManagement;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Core.TagSystem;
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
    private EventBinding<SceneGroupLoadedEvent> _sceneGroupLoadedEventBinding;
    private EventBinding<SceneGroupLoadStartEvent> _sceneGroupLoadStartEventBinding;

    [SerializeField] private PlayerSpawner m_PlayerSpawner;
    [Inject] private IPlayerSetupModel _playerSetupModel;
    private bool _gamePaused = false;
    private int _readinessVersion;
    private TaskCompletionSource<bool> _readySource = CreateReadySource();

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
        _readinessVersion++;
        _readySource.TrySetCanceled();
        _readySource = CreateReadySource();

        foreach (var player in m_PlayerSpawner.Players)
            player.GetComponent<Actor>().enabled = false;
    }

    private async void OnSceneGroupLoaded(SceneGroupLoadedEvent sceneGroupLoadedEvent)
    {
        var readinessVersion = _readinessVersion;

        await CameraTargetSetupAsync();

        if (readinessVersion != _readinessVersion)
            return;

        foreach (var player in m_PlayerSpawner.Players)
            player.GetComponent<Actor>().enabled = true;

        _readySource.TrySetResult(true);
    }

    private async Task CameraTargetSetupAsync()
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogWarning("CinemachineBrain not found on main camera.");
            return;
        }

        await WaitUntilCameraIsReady(brain);

        var virtualCamera = brain.ActiveVirtualCamera as CinemachineCamera;
        if (virtualCamera == null)
        {
            Debug.LogWarning("ActiveVirtualCamera is not a CinemachineCamera.");
            return;
        }

        var targetGroup = virtualCamera.Target.TrackingTarget;
        var cinemachineTargetGroup = targetGroup.GetComponent<CinemachineTargetGroup>();
        if (cinemachineTargetGroup == null)
        {
            Debug.LogWarning("CinemachineTargetGroup not found on TrackingTarget.");
            return;
        }

        foreach (var player in m_PlayerSpawner.Players)
        {
            var lookAtTarget = player.GetComponent<ICameraLookAt>()?.Target;
            if (lookAtTarget == null)
            {
                Debug.LogWarning("ICameraLookAt.Target is null.");
                return;
            }

            cinemachineTargetGroup.AddMember(lookAtTarget, 0.5f, 1f);
        }


        var cameraManager = GetComponent<CameraManager>();
        if (cameraManager)
        {
            cameraManager.enabled = false;
            await Awaitable.NextFrameAsync();
            cameraManager.enabled = m_PlayerSpawner.Players.Count == 1;
        }
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

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        return ReadinessTask.WaitAsync(_readySource.Task, cancellationToken);
    }

    private static TaskCompletionSource<bool> CreateReadySource()
    {
        return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private void OnDestroy()
    {
        _readySource.TrySetCanceled();
        EventBus<SceneGroupLoadedEvent>.Deregister(_sceneGroupLoadedEventBinding);
        EventBus<SceneGroupLoadStartEvent>.Deregister(_sceneGroupLoadStartEventBinding);
    }
}
