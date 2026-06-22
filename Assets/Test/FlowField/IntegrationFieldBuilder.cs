using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public sealed class IntegrationFieldBuilder
{
    private FlowFieldGrid grid;
    private NativeQueue<int> openQueue;

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
            terrainHeight = grid.TerrainHeight,

            integration = grid.Integration,
            openQueue = openQueue
        };

        job.Run();
    }

    public void Build(NativeArray<int2> targets)
    {
        for (int i = 0; i < grid.CellCount; i++)
            grid.Integration[i] = ushort.MaxValue;

        openQueue.Clear();

        for (int i = 0; i < targets.Length; i++)
        {
            int2 target = targets[i];
            int targetIndex = target.x + target.y * grid.Width;
            grid.Integration[targetIndex] = 0;
            openQueue.Enqueue(targetIndex);
        }

        var job = new IntegrationFieldJob
        {
            width = grid.Width,
            height = grid.Height,

            cost = grid.Cost,
            terrainHeight = grid.TerrainHeight,

            integration = grid.Integration,
            openQueue = openQueue
        };

        job.Run();
    }
}