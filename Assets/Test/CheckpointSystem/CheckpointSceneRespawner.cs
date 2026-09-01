using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SAS.Checkpoints
{
    internal sealed class CheckpointSceneRespawner : IDisposable
    {
        private readonly ICheckpointRespawnService _respawnService;
        private readonly ICheckpointPlayerProvider _playerProvider;
        private readonly ICheckpointSceneLoadNotifier _sceneLoadNotifier;
        private readonly Task _systemReady;

        private bool _isDisposed;

        public CheckpointSceneRespawner(ICheckpointRespawnService respawnService, ICheckpointPlayerProvider playerProvider, ICheckpointSceneLoadNotifier sceneLoadNotifier, Task systemReady)
        {
            _respawnService = respawnService ?? throw new ArgumentNullException(nameof(respawnService));
            _playerProvider = playerProvider ?? throw new ArgumentNullException(nameof(playerProvider));
            _sceneLoadNotifier = sceneLoadNotifier ?? throw new ArgumentNullException(nameof(sceneLoadNotifier));
            _systemReady = systemReady ?? throw new ArgumentNullException(nameof(systemReady));

            _sceneLoadNotifier.SceneLoaded += OnSceneLoaded;
        }

        private async void OnSceneLoaded()
        {
            try
            {
                await _systemReady;

                if (_isDisposed)
                    return;

                IEnumerable<CheckpointPlayer> players = _playerProvider.GetPlayers();

                if (players == null)
                    return;

                foreach (CheckpointPlayer player in players)
                {
                    GameObject character = player.GameObject;

                    if (character == null || !character.activeSelf)
                        continue;

                    _respawnService.TryRespawn(player.PlayerId, character);
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

            _sceneLoadNotifier.SceneLoaded -= OnSceneLoaded;
            _isDisposed = true;
        }
    }
}