namespace LevelStreaming
{
    /// <summary>
    /// Implement on a camera controller whose zoom cannot be derived from camera
    /// distance, orthographic size, or field of view.
    /// </summary>
    public interface IStreamingZoomSource
    {
        float NormalizedZoom { get; }
    }
}
