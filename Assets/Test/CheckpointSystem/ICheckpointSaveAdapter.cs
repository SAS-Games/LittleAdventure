using System.Threading.Tasks;

namespace SAS.Checkpoints
{
    /// <summary>
    /// Adapts checkpoint progress loading and saving to a game's save system.
    /// Supplying this adapter is optional.
    /// </summary>
    public interface ICheckpointSaveAdapter
    {
        Task<CheckpointProgressData> LoadAsync(int userId);
        Task<bool> SaveAsync(int userId, CheckpointProgressData data);
    }
}
