namespace SAS.Checkpoints
{
    /// <summary>
    /// Default provider for games that do not have profiles or save slots.
    /// </summary>
    public sealed class FixedCheckpointUserIdProvider : ICheckpointUserIdProvider
    {
        private readonly int _userId;

        public FixedCheckpointUserIdProvider(int userId = 0)
        {
            _userId = userId;
        }

        public int GetActiveUserId()
        {
            return _userId;
        }
    }
}
