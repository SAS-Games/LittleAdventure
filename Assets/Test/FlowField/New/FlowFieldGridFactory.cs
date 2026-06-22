using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class FlowFieldGridFactory
{
    public static FlowFieldGrid Create(
        FlowFieldAsset asset,
        Allocator allocator = Allocator.Persistent)
    {
        FlowFieldGrid grid =
            new FlowFieldGrid(
                asset.width,
                asset.height,
                asset.cellSize,
                asset.origin,
                allocator);

        int count = asset.CellCount;

        for (int i = 0; i < count; i++)
        {
            grid.Cost[i] =
                asset.costs[i];

            grid.TerrainHeight[i] =
                asset.terrainHeights[i];

            grid.TerrainNormal[i] =
                asset.terrainNormals[i];
        }

        return grid;
    }
}