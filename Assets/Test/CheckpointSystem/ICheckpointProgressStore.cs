using System.Threading.Tasks;

namespace SAS.Checkpoints
{
    /// <summary>
    /// Persists checkpoint progress without coupling the checkpoint system to a
    /// game's save-system implementation.
    /// </summary>
    public interface ICheckpointProgressStore
    {
        Task<CheckpointProgressData> LoadAsync(int userId);
        Task<bool> SaveAsync(int userId, CheckpointProgressData data);
    }
}
