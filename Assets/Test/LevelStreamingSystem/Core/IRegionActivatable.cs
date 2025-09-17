namespace LevelStreaming
{
    public interface IRegionActivatable
    {
        void OnRegionActivated(RegionManager.Region region, bool active);
    }
}