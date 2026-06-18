using Unity.Mathematics;
using UnityEngine;

public static class FlowFieldGridUtility
{
    public static int2 WorldToCell(
        float3 worldPos,
        float2 origin,
        float cellSize)
    {
        float2 local = (worldPos.xz - origin) / cellSize;
        return (int2)math.floor(local);
    }

    public static bool IsInsideGrid(
        int2 cell,
        int width,
        int height)
    {
        return (uint)cell.x < (uint)width &&
               (uint)cell.y < (uint)height;
    }

    public static int CellToIndex(
        int2 cell,
        int width)
    {
        return cell.x + cell.y * width;
    }

    public static float3 CellToWorld(
        int2 cell,
        float2 origin,
        float cellSize)
    {
        return new float3(
            origin.x + (cell.x + 0.5f) * cellSize,
            0f,
            origin.y + (cell.y + 0.5f) * cellSize
        );
    }
}