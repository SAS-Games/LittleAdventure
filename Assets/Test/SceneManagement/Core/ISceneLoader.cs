using UnityEngine;

public interface IStreamingLoader
{
    void Load(string name, System.Action<string> onLoaded);
    void Unload(string name, System.Action<string> onUnloaded);
    bool IsLoading(string name);
}

// public abstract class StreamingLoaderSO : ScriptableObject, IStreamingLoader
// {
//     public abstract bool IsLoading(string sceneName);
//     public abstract void Load(string sceneName, System.Action<string> onComplete);
//     public abstract void Unload(string sceneName, System.Action<string> onComplete);
// }
