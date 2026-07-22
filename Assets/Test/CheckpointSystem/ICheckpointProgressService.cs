using System;
using System.Threading.Tasks;

namespace SAS.Checkpoints
{
    public interface ICheckpointProgressService
    {
        event Action Initialized;
        event Action<string> CheckpointCompleted;
        event Action ProgressReset;

        bool IsInitialized { get; }
        bool IsCompleted(string checkpointId);
        ActiveCheckpointData GetActiveCheckpoint();
        Task InitializeAsync(int userId);
        Task<bool> CompleteAsync(string checkpointId);
        Task<bool> ActivateCheckpointAsync(ActiveCheckpointData checkpointData);
        Task SetActiveCheckpointAsync(ActiveCheckpointData checkpointData);

        Task ResetAsync();
    }
}
