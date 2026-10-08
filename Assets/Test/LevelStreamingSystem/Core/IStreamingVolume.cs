using System;
using UnityEngine;

namespace LevelStreaming
{
    /// <summary>
    /// Geometry used by streaming. BroadphaseBounds must enclose the complete volume;
    /// it is only a spatial-index hint, never an exact intersection result.
    /// Implementations must be queryable while the region content is unloaded.
    /// </summary>
    public interface IStreamingVolume
    {
        Bounds BroadphaseBounds { get; }
        bool Contains(Vector3 point);

        /// <summary>
        /// Return false when this shape pair is unsupported. Return true with the
        /// exact result otherwise. Intersection must be symmetric and include contact.
        /// </summary>
        bool TryIntersects(IStreamingVolume other, out bool intersects);
    }

    /// <summary>Base for region volumes serialized with SerializeReference.</summary>
    [Serializable]
    public abstract class StreamingVolume : IStreamingVolume
    {
        public abstract Bounds BroadphaseBounds { get; }
        public abstract bool Contains(Vector3 point);
        public abstract bool TryIntersects(IStreamingVolume other, out bool intersects);
    }

    /// <summary>The existing axis-aligned box implementation.</summary>
    [Serializable]
    public sealed class BoxStreamingVolume : StreamingVolume
    {
        [SerializeField] private Bounds bounds;
        public BoxStreamingVolume(Bounds bounds) => this.bounds = bounds;

        public Bounds Bounds
        {
            get => bounds;
            set => bounds = value;
        }

        public override Bounds BroadphaseBounds => bounds;
        public override bool Contains(Vector3 point) => bounds.Contains(point);

        public override bool TryIntersects(IStreamingVolume other, out bool intersects)
        {
            intersects = false;
            if (other is not BoxStreamingVolume box)
                return false;
            intersects = bounds.Intersects(box.bounds);
            return true;
        }
    }

    public static class StreamingVolumeIntersection
    {
        public static bool Intersects(IStreamingVolume first, IStreamingVolume second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (!first.BroadphaseBounds.Intersects(second.BroadphaseBounds))
                return false;
            if (first.TryIntersects(second, out bool result))
                return result;
            if (second.TryIntersects(first, out result))
                return result;
            throw new NotSupportedException($"No exact streaming intersection for {first.GetType().Name} and {second.GetType().Name}.");
        }
    }
}