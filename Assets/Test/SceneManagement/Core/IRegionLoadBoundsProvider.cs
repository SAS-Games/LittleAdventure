using UnityEngine;

public interface IRegionLoadBoundsProvider
{
    Bounds GetLoadBounds();
    Bounds GetUnloadBounds();
}