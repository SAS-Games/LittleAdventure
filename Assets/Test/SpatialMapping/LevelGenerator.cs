using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public BoxCollider[] agents;
    [Header("Grid Size")]
    [SerializeField] private int gridX = 250;
    [SerializeField] private int gridY = 250;

    [Header("Cell Spacing")]
    [SerializeField] private Vector2 spacing = new Vector2(1.2f, 1.2f);

    [Header("Prefab")]
    [SerializeField] private GameObject cellPrefab;

    [Header("Options")]
    [SerializeField] private bool centerGrid = true;
    List<Transform> transforms = new List<Transform>();
    private NativeList<int> queryResults;
    private NativeList<int> queryResults1;

    private Bounds[] bounds;

    private void Start()
    {
        bounds = new Bounds[agents.Length];
        for (int i = 0; i < agents.Length; i++)
            bounds[i] = agents[i].bounds;

        queryResults = new NativeList<int>(Allocator.Persistent);
        queryResults1 = new NativeList<int>(Allocator.Persistent);

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (!cellPrefab)
        {
            Debug.LogError("GridGenerator: Cell Prefab is missing!");
            return;
        }

        Vector2 offset = Vector2.zero;

        if (centerGrid)
        {
            offset.x = (gridX - 1) * spacing.x * 0.5f;
            offset.y = (gridY - 1) * spacing.y * 0.5f;
        }

        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                Vector3 position = new Vector3(
                    x * spacing.x - offset.x,
                    0f,
                    y * spacing.y - offset.y
                );
                var tr = Instantiate(cellPrefab, position, Quaternion.identity, transform).transform;
                tr.name = $"{x},{0}, {y}";
                transforms.Add(tr);
            }
        }

    }

    bool initialized = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            GetComponent<SpatialMapping>().Build(transforms);
            initialized = true;
        }

        if (initialized)
        {
            for (int i = 0; i < agents.Length; i++)
                bounds[i] = agents[i].bounds;
            GetComponent<SpatialMapping>().QueryDeltaMultipleBounds(bounds, queryResults, queryResults1);
            foreach (var rs in queryResults)
                transforms[rs].gameObject.SetActive(true);
            foreach (var rs in queryResults1)
                transforms[rs].gameObject.SetActive(false);
        }
    }
}
