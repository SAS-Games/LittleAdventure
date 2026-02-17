using System.Threading.Tasks;

public static class UnityAsync
{
    public static async Task NextFrame()
    {
        await Task.Yield();
    }
}