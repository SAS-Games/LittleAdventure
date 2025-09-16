using UnityEngine;

namespace LevelStreaming
{
    public interface IStreamingBoundsProvider
    {
        Bounds GetLoadBounds();
        Bounds GetUnloadBounds();
        Bounds GetActivateBounds();
    }
}