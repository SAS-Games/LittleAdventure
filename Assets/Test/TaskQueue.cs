using System;
using System.Collections.Generic;

public class TaskQueue
{
    private readonly Queue<Action<Action>> _queue = new();
    private bool _running;

    public void Enqueue(Action<Action> task)
    {
        _queue.Enqueue(task);

        if (!_running)
            RunNext();
    }

    private void RunNext()
    {
        if (_queue.Count == 0)
        {
            _running = false;
            return;
        }

        _running = true;

        var task = _queue.Dequeue();

        // Call task and pass finish callback
        task.Invoke(OnTaskComplete);
    }

    private void OnTaskComplete()
    {
        _running = false;
        RunNext();
    }
}