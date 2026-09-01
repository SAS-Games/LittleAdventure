namespace SAS.Checkpoints
{
    /// <summary>
    /// Supplies the game-specific user or save-slot ID used for checkpoint data.
    /// Supplying this provider is optional; the installer uses ID 0 when absent.
    /// </summary>
    public interface ICheckpointUserIdProvider
    {
        int GetActiveUserId();
    }
}
