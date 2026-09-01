using System;
using System.Collections.Generic;

namespace SAS.Checkpoints
{
    [Serializable]
    public sealed class CheckpointProgressData
    {
        public const int CurrentVersion = 2;
        public int Version = CurrentVersion;

        public List<string> CompletedCheckpointIds = new();
        public ActiveCheckpointData ActiveCheckpoint;

        internal CheckpointProgressData Clone()
        {
            return new CheckpointProgressData
            {
                Version = Version,
                CompletedCheckpointIds = new List<string>(CompletedCheckpointIds),
                ActiveCheckpoint = ActiveCheckpoint?.Clone()
            };
        }
    }
}