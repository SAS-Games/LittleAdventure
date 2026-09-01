using System;

namespace SAS.Checkpoints
{
    /// <summary>
    /// Converts a game's scene-loading mechanism into a checkpoint-owned event.
    /// </summary>
    public interface ICheckpointSceneLoadNotifier
    {
        event Action SceneLoaded;
    }
}
