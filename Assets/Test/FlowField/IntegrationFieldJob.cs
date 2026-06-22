using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct IntegrationFieldJob : IJob
{
    public int width;
    public int height;

    [ReadOnly]
    public NativeArray<byte> cost;

    [ReadOnly]
    public NativeArray<float> terrainHeight;

    public NativeArray<ushort> integration;

    public NativeQueue<int> openQueue;

    public void Execute()
    {
        while (openQueue.TryDequeue(out int index))
        {
            ushort currentCost =
                integration[index];

            int x = index % width;
            int y = index / width;

            // Cardinal

            RelaxNeighbor(
                x, y,
                x, y + 1,
                currentCost,
                10);

            RelaxNeighbor(
                x, y,
                x + 1, y,
                currentCost,
                10);

            RelaxNeighbor(
                x, y,
                x, y - 1,
                currentCost,
                10);

            RelaxNeighbor(
                x, y,
                x - 1, y,
                currentCost,
                10);

            // Diagonal

            RelaxNeighbor(
                x, y,
                x + 1, y + 1,
                currentCost,
                14);

            RelaxNeighbor(
                x, y,
                x - 1, y + 1,
                currentCost,
                14);

            RelaxNeighbor(
                x, y,
                x + 1, y - 1,
                currentCost,
                14);

            RelaxNeighbor(
                x, y,
                x - 1, y - 1,
                currentCost,
                14);
        }
    }

    private void RelaxNeighbor(
        int cx,
        int cy,
        int nx,
        int ny,
        ushort currentIntegration,
        ushort moveCost)
    {
        if ((uint)nx >= (uint)width ||
            (uint)ny >= (uint)height)
        {
            return;
        }

        int currentIndex =
            cx + cy * width;

        int neighborIndex =
            nx + ny * width;

        byte terrainCost =
            cost[neighborIndex];

        // blocked
        if (terrainCost == byte.MaxValue)
            return;

        //--------------------------------------------------
        // Terrain climb penalty
        //--------------------------------------------------

        float currentHeight =
            terrainHeight[currentIndex];

        float neighborHeight =
            terrainHeight[neighborIndex];

        float climb =
            neighborHeight - currentHeight;

        byte climbPenalty = 0;

        if (climb > 0f)
        {
            climbPenalty =
                (byte)math.min(
                    climb * 5f,
                    100f);
        }

        int totalTerrainCost =
            terrainCost + climbPenalty;

        ushort newCost =
            (ushort)(
                currentIntegration +
                totalTerrainCost * moveCost);

        ushort oldCost =
            integration[neighborIndex];

        if (newCost < oldCost)
        {
            integration[neighborIndex] =
                newCost;

            openQueue.Enqueue(
                neighborIndex);
        }
    }
}