using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FlowDirectionJob : IJobParallelFor
{
    public int width;
    public int height;

    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<ushort> integration;
    [ReadOnly] [NativeDisableParallelForRestriction] public NativeArray<byte> cost;
    [WriteOnly] public NativeArray<float2> flow;

    public void Execute(int index)
    {
        ushort currentIntegration = integration[index];

        if (currentIntegration == ushort.MaxValue)
        {
            flow[index] = float2.zero;
            return;
        }

        int x = index % width;
        int y = index / width;

        ushort bestIntegration = currentIntegration;
        float2 bestDir = float2.zero;

        for (int j = -1; j <= 1; j++)
        {
            for (int i = -1; i <= 1; i++)
            {
                if (i == 0 && j == 0)
                    continue;

                int nx = x + i;
                int ny = y + j;

                if ((uint)nx >= (uint)width || (uint)ny >= (uint)height)
                    continue;

                int neighborIndex = nx + ny * width;
                if (cost[neighborIndex] == byte.MaxValue)
                    continue;

                if (i != 0 && j != 0)
                {
                    int sideA = (x + i) + y * width;
                    int sideB = x + (y + j) * width;

                    if (cost[sideA] == byte.MaxValue || cost[sideB] == byte.MaxValue)
                        continue;
                }

                ushort neighborIntegration = integration[neighborIndex];
                if (neighborIntegration < bestIntegration)
                {
                    bestIntegration = neighborIntegration;
                    bestDir = new float2(i, j);
                }
            }
        }

        flow[index] = math.normalizesafe(bestDir);
    }
}
