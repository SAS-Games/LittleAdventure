using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelGenerator : MonoBehaviour
{
    [Header("Agents")]
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

    [Header("Input")]
    [SerializeField] private InputAction buildAction;

    private List<Transform> _cells = new List<Transform>();

    private SpatialDatabase _database;
    private SpatialDeltaSystem _deltaSystem;
    private SpatialBoundsQuery _spatialBoundsQuery;

    private NativeList<int> _entered;
    private NativeList<int> _exited;

    private Bounds[] _agentBounds;
    Transform[] _cube= new Transform[200];

    private bool _initialized;
//private NativeArray<byte> _seen;
    private void Awake()
    {
        _entered = new NativeList<int>(Allocator.Persistent);
        _exited  = new NativeList<int>(Allocator.Persistent);
        _agentBounds = new Bounds[agents.Length];
    }

    private void OnEnable()
    {
        buildAction.Enable();
    }

    private void OnDisable()
    {
        buildAction.Disable();
    }

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
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
                Vector3 pos = new Vector3(
                    x * spacing.x - offset.x,
                    0f,
                    y * spacing.y - offset.y);

                Transform tr = Instantiate(
                    cellPrefab,
                    pos,
                    Quaternion.identity,
                    transform).transform;

                tr.name = $"{x},0,{y}";
                tr.gameObject.SetActive(false);

                _cells.Add(tr);
            }
        }
    }

    private void Update()
    {
        // Build spatial database
        if (!_initialized && buildAction.WasPressedThisFrame())
        {
            _database = new SpatialDatabase(_cells, new int3(9, 9, 9));

            _deltaSystem = new SpatialDeltaSystem(_database);
            _spatialBoundsQuery = new SpatialBoundsQuery(_database,500);
       //     _seen = new NativeArray<byte>(_database.Capacity, Allocator.Persistent);

            _initialized = true;

            Debug.Log("Spatial database initialized.");
        }

        if (!_initialized)
            return;

        // Collect agent bounds
        for (int i = 0; i < agents.Length; i++)
            _agentBounds[i] = agents[i].bounds;

        // Update delta (once per frame)
        _deltaSystem.UpdateDelta(_agentBounds, _entered, _exited);

       // Activate entered tiles
        for (int i = 0; i < _entered.Length; i++)
        {
            int index = _entered[i];
            _cells[index].gameObject.SetActive(true);
        }
        
        // Deactivate exited tiles
        for (int i = 0; i < _exited.Length; i++)
        {
            int index = _exited[i];
            _cells[index].gameObject.SetActive(false);
        }

        // for (int j = 0; j < _agentBounds.Length; j++)
        // {
        //     var val = _spatialBoundsQuery.QueryAllInBoundsNonAlloc(_agentBounds[j], _cube);
        //     Debug.Log(val);
        //     for (int i = 0; i < val; i++)
        //     {
        //         _cube[i].gameObject.SetActive(true);
        //     }
        // }
    }

    private void OnDestroy()
    {
        if (_database != null)
            _database.Dispose();
        
        if (_deltaSystem != null)
            _deltaSystem.Dispose();
        
        if(_spatialBoundsQuery != null)
            _spatialBoundsQuery.Dispose();
        
        if (_entered.IsCreated)
            _entered.Dispose();

        if (_exited.IsCreated)
            _exited.Dispose();
    }
}
