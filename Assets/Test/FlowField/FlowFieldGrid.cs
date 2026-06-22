using System;
using Unity.Collections;
using Unity.Mathematics;

public struct FlowFieldGrid : IDisposable
{
    public int Width;
    public int Height;

    public float CellSize;
    public float2 Origin;

    // Existing fields
    public NativeArray<byte> Cost;
    public NativeArray<ushort> Integration;
    public NativeArray<float2> Flow;

    // Terrain data
    public NativeArray<float> TerrainHeight;
    public NativeArray<float3> TerrainNormal;

    public int CellCount => Width * Height;

    public FlowFieldGrid(int width, int height, float cellSize, float2 origin, Allocator allocator = Allocator.Persistent)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        Origin = origin;

        int count = width * height;

        Cost = new NativeArray<byte>(count, allocator);
        Integration = new NativeArray<ushort>(count, allocator);
        Flow = new NativeArray<float2>(count, allocator);

        TerrainHeight = new NativeArray<float>(count, allocator);
        TerrainNormal = new NativeArray<float3>(count, allocator);
    }

    public void Dispose()
    {
        if (Cost.IsCreated)
            Cost.Dispose();

        if (Integration.IsCreated)
            Integration.Dispose();

        if (Flow.IsCreated)
            Flow.Dispose();

        if (TerrainHeight.IsCreated)
            TerrainHeight.Dispose();

        if (TerrainNormal.IsCreated)
            TerrainNormal.Dispose();
    }
}