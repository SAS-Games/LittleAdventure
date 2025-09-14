using UnityEngine;

public interface IStreamingBoundsProvider
{
    Bounds GetLoadBounds();
    Bounds GetUnloadBounds();
    Bounds GetActivateBounds();

}