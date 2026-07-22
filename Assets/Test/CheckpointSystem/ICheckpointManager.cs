using System;
using System.Threading.Tasks;

namespace SAS.Checkpoints
{
    public interface ICheckpointManager
    {
        event Action<Checkpoint, Checkpoint> ActiveCheckpointChanged;
        Checkpoint ActiveCheckpoint { get; }
        string ActiveCheckpointId { get; }
        bool HasActiveCheckpoint { get; }
        bool IsActive(Checkpoint checkpoint);
        bool CanActivate(Checkpoint checkpoint);
        Task<bool> ActivateAsync(Checkpoint checkpoint);
        void RestoreFromProgress();
        void RegisterCheckpoint(Checkpoint checkpoint);
        void UnregisterCheckpoint(Checkpoint checkpoint);
        void RegisterGroup(SpawnPointGroup group);
        void UnregisterGroup(SpawnPointGroup group);
        bool TryGetCheckpoint(string checkpointId, out Checkpoint checkpoint);
        bool TryGetSpawnPointGroup(string spawnPointGroupId, out SpawnPointGroup group);
        bool TryGetActiveSpawnPointGroup(out SpawnPointGroup group);
        bool TryGetDefaultSpawnPointGroup(out SpawnPointGroup group);
        bool TryGetSpawnPoint(int playerId, out SpawnPoint spawnPoint);
    }
}
