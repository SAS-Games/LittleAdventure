using System.Collections.Generic;
using UnityEngine;

namespace SAS.Checkpoints
{
    public readonly struct CheckpointPlayer
    {
        public int PlayerId { get; }
        public GameObject GameObject { get; }

        public CheckpointPlayer(int playerId, GameObject gameObject)
        {
            PlayerId = playerId;
            GameObject = gameObject;
        }
    }

    /// <summary>
    /// Exposes the players that should be respawned after a scene load.
    /// </summary>
    public interface ICheckpointPlayerProvider
    {
        IEnumerable<CheckpointPlayer> GetPlayers();
    }
}