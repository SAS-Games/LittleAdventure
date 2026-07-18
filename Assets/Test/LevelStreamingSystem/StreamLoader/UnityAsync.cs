using System.Threading.Tasks;
using UnityEngine;

public static class UnityAsync
{
    public static async Task NextFrame()
    {
        // Task.Yield only defers to Unity's synchronization context and can resume
        // later in the same frame. Awaitable guarantees that Start has a frame boundary
        // before region activation callbacks run.
        await Awaitable.NextFrameAsync();
    }
}
