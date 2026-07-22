using System;
using System.Threading.Tasks;
using SAS.SceneManagement;
using UnityEngine;

namespace SAS.Checkpoints
{
    internal sealed class CheckpointSceneRespawner : IDisposable
    {
        private readonly ICheckpointRespawnService _respawnService;
        private readonly IPlayerSetupModel _playerSetupModel;
        private readonly Task _systemReady;
        private readonly EventBinding<SceneGroupLoadedEvent> _sceneLoadedBinding;

        private bool _isDisposed;

        public CheckpointSceneRespawner(ICheckpointRespawnService respawnService, IPlayerSetupModel playerSetupModel, Task systemReady)
        {
            _respawnService = respawnService ?? throw new ArgumentNullException(nameof(respawnService));
            _playerSetupModel = playerSetupModel ?? throw new ArgumentNullException(nameof(playerSetupModel));
            _systemReady = systemReady ?? throw new ArgumentNullException(nameof(systemReady));

            _sceneLoadedBinding = new EventBinding<SceneGroupLoadedEvent>(OnSceneGroupLoaded);
            EventBus<SceneGroupLoadedEvent>.Register(_sceneLoadedBinding);
        }

        private async void OnSceneGroupLoaded(SceneGroupLoadedEvent sceneGroupLoadedEvent)
        {
            try
            {
                await _systemReady;

                if (_isDisposed)
                    return;

                foreach (PlayerProfile player in _playerSetupModel.Players)
                {
                    GameObject character = player.Character;

                    if (character == null || !character.activeSelf)
                        continue;

                    _respawnService.TryRespawn(player.Index, character);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Respawning players after a scene-group load failed.\n" + exception);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            EventBus<SceneGroupLoadedEvent>.Deregister(_sceneLoadedBinding);
            _isDisposed = true;
        }
    }
}
