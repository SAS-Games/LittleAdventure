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
        Task<bool> ActivateCheckpointAsync(ActiveCheckpointData checkpointData);
        Task ResetAsync();
    }
}
