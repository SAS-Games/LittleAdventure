using UnityEngine;

namespace LevelStreaming
{
    /// <summary>
    /// Supplies one atomic sample. Volumes must remain stable for the duration of
    /// the streaming tick. Activation is contained by load, and load by unload.
    /// </summary>
    public interface IStreamingVolumeProvider
    {
        bool TryGetVolumes(out StreamingVolumeSnapshot snapshot);
    }

    /// <summary>
    /// Atomic geometry and observer metadata. A provider may reuse its volume objects;
    /// consume the snapshot in the current tick rather than retaining it as history.
    /// </summary>
    public readonly struct StreamingVolumeSnapshot
    {
        public StreamingVolumeSnapshot(IStreamingVolume activate, IStreamingVolume load, IStreamingVolume unload, Vector3 observerPosition = default, Vector3 velocity = default, float normalizedZoom = 0f, uint revision = 0)
        {
            Activate = activate;
            Load = load;
            Unload = unload;
            ObserverPosition = observerPosition;
            Velocity = velocity;
            NormalizedZoom = normalizedZoom;
            Revision = revision;
        }

        public IStreamingVolume Activate { get; }
        public IStreamingVolume Load { get; }
        public IStreamingVolume Unload { get; }
        public Vector3 ObserverPosition { get; }
        public Vector3 Velocity { get; }
        public float NormalizedZoom { get; }
        public uint Revision { get; }
        public bool IsValid => Activate != null && Load != null && Unload != null;
    }
}
