using UnityEngine;

public interface IStreamingTarget
{
    Bounds GetLoadBounds();
    Bounds GetUnloadBounds();
}