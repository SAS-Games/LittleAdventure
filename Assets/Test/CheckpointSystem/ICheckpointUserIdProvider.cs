namespace SAS.Checkpoints
{
    /// <summary>
    /// Supplies the game-specific user or save-slot ID used for checkpoint data.
    /// </summary>
    public interface ICheckpointUserIdProvider
    {
        int GetActiveUserId();
    }
}
