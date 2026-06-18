using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct FlowDirectionJob : IJobParallelFor
{
    public int width;
    public int height;

    [ReadOnly] 
    [NativeDisableParallelForRestriction] public NativeArray<ushort> integration;
    public NativeArray<float2> flow;

    public void Execute(int index)
    {
        ushort currentCost = integration[index];

        if (currentCost == ushort.MaxValue)
        {
            flow[index] = float2.zero;
            return;
        }

        int x = index % width;
        int y = index / width;

        ushort bestCost = currentCost;
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

                int nIndex = nx + ny * width;
                ushort nCost = integration[nIndex];

                if (nCost < bestCost)
                {
                    bestCost = nCost;
                    bestDir = new float2(i, j);
                }
            }
        }

        if (math.lengthsq(bestDir) > 0f)
            flow[index] = math.normalize(bestDir);
        else
            flow[index] = float2.zero;
    }
}