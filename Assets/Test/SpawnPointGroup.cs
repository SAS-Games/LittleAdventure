using SAS.Core.TagSystem;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointGroup : MonoBehaviour
{
    [Inject] private ICheckpointManager _checkpointManager;
    [field: SerializeField] public string SpawnPointGroupID { get; private set; } = "SpawnPointGroup";
    [SerializeField] private SpawnPoint[] m_SpawnPoints;

    private void Awake()
    {
        this.InjectFieldBindings();
        _checkpointManager.RegisterGroup(this);
    }

    private void OnDestroy()
    {
        _checkpointManager.UnregisterGroup(this);
    }  

    public SpawnPoint GetSpawnPointRandom()
    {
        if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
            return null;

        // Filter only available points
        List<SpawnPoint> availablePoints = new();

        foreach (var point in m_SpawnPoints)
        {
            if (point.SpawnedObject == null)
                availablePoints.Add(point);
        }

        if (availablePoints.Count == 0)
        {
            // fallback: return first spawn point anyway
            return m_SpawnPoints[0];
        }

        return availablePoints[Random.Range(0, availablePoints.Count)];
    }

    public SpawnPoint GetFirstAvailableSpawnPoint()
    {
        foreach (var point in m_SpawnPoints)
        {
            if (point.SpawnedObject == null) // meaning it's unused or unoccupied
                return point;
        }

        // fallback: just return first
        return m_SpawnPoints.Length > 0 ? m_SpawnPoints[0] : null;
    }

    public SpawnPoint GetSpawnPointByPlayerId(int playerId)
    {
        if (m_SpawnPoints == null || m_SpawnPoints.Length == 0)
            return null;

        // Simple modulo-based deterministic assignment
        int index = playerId % m_SpawnPoints.Length;
        return m_SpawnPoints[index];
    }
}
