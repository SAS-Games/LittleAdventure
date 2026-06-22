using UnityEngine;

[CreateAssetMenu(
    fileName = "FlowFieldAsset",
    menuName = "Flow Field/Flow Field Asset")]
public class FlowFieldAsset : ScriptableObject
{
    [Header("Grid")]
    public int width;
    public int height;
    public float cellSize;

    [Header("World")]
    public Vector2 origin;

    [Header("Baked Data")]
    public byte[] costs;
    public float[] terrainHeights;
    public Vector3[] terrainNormals;

    public int CellCount => width * height;
}