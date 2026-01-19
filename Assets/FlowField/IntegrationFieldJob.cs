using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct IntegrationFieldJob : IJob
{
    public int width;
    public int height;

    [ReadOnly] public NativeArray<byte> cost;
    public NativeArray<ushort> integration;

    public NativeQueue<int> openQueue;

    public void Execute()
    {
        // Process until queue empty
        while (openQueue.TryDequeue(out int index))
        {
            ushort currentCost = integration[index];

            int x = index % width;
            int y = index / width;

            // Cardinal neighbors
            RelaxNeighbor(x, y, x,     y + 1, currentCost, 10);
            RelaxNeighbor(x, y, x + 1, y,     currentCost, 10);
            RelaxNeighbor(x, y, x,     y - 1, currentCost, 10);
            RelaxNeighbor(x, y, x - 1, y,     currentCost, 10);

            // Diagonals
            RelaxNeighbor(x, y, x + 1, y + 1, currentCost, 14);
            RelaxNeighbor(x, y, x - 1, y + 1, currentCost, 14);
            RelaxNeighbor(x, y, x + 1, y - 1, currentCost, 14);
            RelaxNeighbor(x, y, x - 1, y - 1, currentCost, 14);
        }
    }

    private void RelaxNeighbor(
        int cx, int cy,
        int nx, int ny,
        ushort currentIntegration,
        ushort moveCost)
    {
        // Bounds check
        if ((uint)nx >= (uint)width || (uint)ny >= (uint)height)
            return;

        int nIndex = nx + ny * width;

        byte terrainCost = cost[nIndex];

        // Blocked cell
        if (terrainCost == 255)
            return;

        // total cost
        ushort newCost = (ushort)(
            currentIntegration +
            terrainCost * moveCost
        );

        ushort oldCost = integration[nIndex];

        if (newCost < oldCost)
        {
            integration[nIndex] = newCost;
            openQueue.Enqueue(nIndex);
        }
    }
}