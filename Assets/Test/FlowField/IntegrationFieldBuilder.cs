using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public sealed class IntegrationFieldBuilder
{
    FlowFieldGrid grid;
    NativeQueue<int> openQueue;

    public IntegrationFieldBuilder(FlowFieldGrid grid)
    {
        this.grid = grid;
        openQueue = new NativeQueue<int>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (openQueue.IsCreated)
            openQueue.Dispose();
    }

    public void Build(int2 target)
    {
        // Reset integration
        for (int i = 0; i < grid.CellCount; i++)
            grid.Integration[i] = ushort.MaxValue;

        openQueue.Clear();

        int targetIndex = target.x + target.y * grid.Width;

        grid.Integration[targetIndex] = 0;
        openQueue.Enqueue(targetIndex);

        var job = new IntegrationFieldJob
        {
            width = grid.Width,
            height = grid.Height,
            cost = grid.Cost,
            integration = grid.Integration,
            openQueue = openQueue
        };

        job.Run(); // or Schedule().Complete();
    }
    
    public void Build(NativeArray<int2> targets)
    {
        // Reset integration
        for (int i = 0; i < grid.CellCount; i++)
            grid.Integration[i] = ushort.MaxValue;

        openQueue.Clear();

        for (int i = 0; i < targets.Length; i++)
        {
            int2 t = targets[i];
            int index = t.x + t.y * grid.Width;

            grid.Integration[index] = 0;
            openQueue.Enqueue(index);
        }

        var job = new IntegrationFieldJob
        {
            width = grid.Width,
            height = grid.Height,
            cost = grid.Cost,
            integration = grid.Integration,
            openQueue = openQueue
        };

        job.Run();
    }

}