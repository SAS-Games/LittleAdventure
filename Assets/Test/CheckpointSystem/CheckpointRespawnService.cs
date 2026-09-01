using System;
using UnityEngine;

namespace SAS.Checkpoints
{
    public sealed class CheckpointRespawnService : ICheckpointRespawnService
    {
        private readonly ICheckpointManager _checkpointManager;
        private readonly ICheckpointProgressService _progressService;

        public CheckpointRespawnService(ICheckpointManager checkpointManager,
            ICheckpointProgressService progressService)
        {
            _checkpointManager = checkpointManager ?? throw new ArgumentNullException(nameof(checkpointManager));
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        }

        public bool TryRespawn(int playerId, GameObject player)
        {
            if (player == null)
                return false;

            if (_checkpointManager.TryGetSpawnPoint(playerId, out SpawnPoint spawnPoint))
            {
                Teleport(player, spawnPoint.Position, spawnPoint.Rotation);
                spawnPoint.Assign(player);
                return true;
            }

            ActiveCheckpointData activeCheckpoint = _progressService.GetActiveCheckpoint();

            if (activeCheckpoint == null)
                return false;

            Teleport(player, activeCheckpoint.FallbackPosition, activeCheckpoint.FallbackRotation);
            return true;
        }

        private static void Teleport(GameObject player, Vector3 position, Quaternion rotation)
        {
            player.TryGetComponent(out CharacterController characterController);

            bool controllerWasEnabled = characterController != null && characterController.enabled;

            if (characterController != null)
                characterController.enabled = false;

            try
            {
                if (player.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                    rigidbody.position = position;
                    rigidbody.rotation = rotation;
                }
                else
                    player.transform.SetPositionAndRotation(position, rotation);
            }
            finally
            {
                if (characterController != null)
                    characterController.enabled = controllerWasEnabled;
            }
        }
    }
}
