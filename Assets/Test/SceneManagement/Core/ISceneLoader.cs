public interface IStreamingLoader
{
    void Load(string name, System.Action<string> onLoaded);
    void Unload(string name, System.Action<string> onUnloaded);
    bool IsLoading(string name);
}