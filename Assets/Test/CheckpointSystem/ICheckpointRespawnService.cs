using UnityEngine;

namespace SAS.Checkpoints
{
    public interface ICheckpointRespawnService
    {
        bool TryGetSpawnPoint(int playerId, out SpawnPoint spawnPoint);
        bool TryRespawn(int playerId, GameObject player);
    }
}
