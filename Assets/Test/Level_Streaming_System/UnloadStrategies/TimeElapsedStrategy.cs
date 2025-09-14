using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Streaming/UnloadStrategies/TimeElapsed")]
public class TimeElapsedStrategy : UnloadStrategy
{
    [SerializeField] private float m_TimeToUnload = 10;

    public override bool ShouldUnload(Bounds unloadBounds, RegionManager.Region region)
    {
        return Time.time - region.LoadedTime > m_TimeToUnload;
    }
}