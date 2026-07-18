using System.Threading.Tasks;

namespace LevelStreaming
{
    public interface IStreamingLoader<TRegion>
    {
        Task LoadAsync(TRegion region);
        Task UnloadAsync(TRegion region);
        bool IsLoading(TRegion region);
        bool IsLoaded(TRegion region);
    }
}
