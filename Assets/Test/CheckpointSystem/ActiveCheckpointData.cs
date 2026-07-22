using System;
using UnityEngine;

namespace SAS.Checkpoints
{
    [Serializable]
    public sealed class ActiveCheckpointData
    {
        public string CheckpointId;
        public string SceneName;
        public string SpawnPointGroupId;

        public Vector3 FallbackPosition;
        public Quaternion FallbackRotation = Quaternion.identity;

        public ActiveCheckpointData()
        {
        }

        public ActiveCheckpointData(string checkpointId, string sceneName, string spawnPointGroupId, Vector3 fallbackPosition, Quaternion fallbackRotation)
        {
            CheckpointId = checkpointId;
            SceneName = sceneName;
            SpawnPointGroupId = spawnPointGroupId;
            FallbackPosition = fallbackPosition;
            FallbackRotation = fallbackRotation;
        }

        internal ActiveCheckpointData Clone()
        {
            return new ActiveCheckpointData(CheckpointId, SceneName, SpawnPointGroupId, FallbackPosition, FallbackRotation);
        }
    }
}
