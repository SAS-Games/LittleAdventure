using UnityEngine;

namespace LevelStreaming
{
    /// <summary>
    /// An atomic sample of the three streaming hysteresis volumes.
    /// </summary>
    public readonly struct StreamingBoundsSnapshot
    {
        public StreamingBoundsSnapshot(Bounds activate, Bounds load, Bounds unload,
            Vector3 observerPosition, Vector3 velocity, float normalizedZoom, uint revision)
        {
            Activate = activate;
            Load = load;
            Unload = unload;
            ObserverPosition = observerPosition;
            Velocity = velocity;
            NormalizedZoom = normalizedZoom;
            Revision = revision;
        }

        public Bounds Activate { get; }
        public Bounds Load { get; }
        public Bounds Unload { get; }
        public Vector3 ObserverPosition { get; }
        public Vector3 Velocity { get; }
        public float NormalizedZoom { get; }
        public uint Revision { get; }
    }

    public interface IStreamingBoundsProvider
    {
        Bounds GetLoadBounds();
        Bounds GetUnloadBounds();
        Bounds GetActivateBounds();
    }

    /// <summary>
    /// Optional richer contract for providers that calculate all bounds as one sample.
    /// Existing bounds providers only need to implement <see cref="IStreamingBoundsProvider"/>.
    /// </summary>
    public interface IStreamingBoundsSnapshotProvider : IStreamingBoundsProvider
    {
        bool TryGetSnapshot(out StreamingBoundsSnapshot snapshot);
        void ResetPrediction();
    }

    /// <summary>
    /// Implement on a camera controller when its notion of zoom cannot be derived from
    /// Camera distance, orthographic size, or field of view.
    /// </summary>
    public interface IStreamingZoomSource
    {
        float NormalizedZoom { get; }
    }
}
