using System.Threading.Tasks;
using UnityEngine;

namespace LevelStreaming
{
    public static class AsyncOperationExtensions
    {
        public static Task ToTask(this AsyncOperation op)
        {
            if (op.isDone)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            op.completed += _ => tcs.TrySetResult(true);

            return tcs.Task;
        }
    }
}