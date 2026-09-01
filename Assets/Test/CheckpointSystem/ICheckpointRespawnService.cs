using UnityEngine;

namespace SAS.Checkpoints
{
    public interface ICheckpointRespawnService
    {
        bool TryRespawn(int playerId, GameObject player);
    }
}
