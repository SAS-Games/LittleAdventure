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

    // --------------------------------------------------
    // Terrain Helpers
    // --------------------------------------------------

    public static float3 CellToWorldOnTerrain(
        int2 cell,
        float2 origin,
        float cellSize,
        Terrain terrain)
    {
        float3 pos = CellToWorld(
            cell,
            origin,
            cellSize);

        if (terrain != null)
        {
            pos.y = terrain.SampleHeight(pos)
                  + terrain.transform.position.y;
        }

        return pos;
    }

    public static float SampleTerrainHeight(
        float3 worldPos,
        Terrain terrain)
    {
        if (terrain == null)
            return 0f;

        return terrain.SampleHeight(worldPos)
             + terrain.transform.position.y;
    }

    public static float3 SampleTerrainNormal(
        float3 worldPos,
        Terrain terrain)
    {
        if (terrain == null)
            return math.up();

        TerrainData data = terrain.terrainData;

        float3 local =
            worldPos - (float3)terrain.transform.position;

        float u =
            local.x / data.size.x;

        float v =
            local.z / data.size.z;

        Vector3 normal =
            data.GetInterpolatedNormal(u, v);

        return normal;
    }

    public static float GetSlopeAngle(
        float3 normal)
    {
        return math.degrees(
            math.acos(
                math.clamp(
                    math.dot(
                        math.normalize(normal),
                        math.up()),
                    -1f,
                    1f)));
    }
    
    public static bool IsValidCell(
        int2 cell,
        int width,
        int height)
    {
        return cell.x >= 0 &&
               cell.y >= 0 &&
               cell.x < width &&
               cell.y < height;
    }
}