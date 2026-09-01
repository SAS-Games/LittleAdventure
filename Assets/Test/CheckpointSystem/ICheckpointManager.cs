using System;
using System.Threading.Tasks;

namespace SAS.Checkpoints
{
    public interface ICheckpointManager
    {
        event Action<Checkpoint, Checkpoint> ActiveCheckpointChanged;
        bool IsActive(Checkpoint checkpoint);
        bool CanActivate(Checkpoint checkpoint);
        Task<bool> ActivateAsync(Checkpoint checkpoint);
        void RestoreFromProgress();
        void RegisterCheckpoint(Checkpoint checkpoint);
        void UnregisterCheckpoint(Checkpoint checkpoint);
        void RegisterGroup(SpawnPointGroup group);
        void UnregisterGroup(SpawnPointGroup group);
        bool TryGetSpawnPoint(int playerId, out SpawnPoint spawnPoint);
    }
}
