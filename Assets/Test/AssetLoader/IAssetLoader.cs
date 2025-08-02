using SAS.Utilities.TagSystem;
using System.Threading.Tasks;

namespace SAS.AssetLoader
{
    public interface IAssetLoader<T> : IBindable
    {
        Task<T> LoadAsync(string path);
    }
}
