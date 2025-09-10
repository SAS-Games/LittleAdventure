public interface IStreamingLoader<TRegion>
{
    void Load(TRegion region, System.Action<TRegion> onLoaded);
    void Unload(TRegion region, System.Action<TRegion> onUnloaded);
    bool IsLoading(TRegion region);
    bool IsLoaded(TRegion region);
}