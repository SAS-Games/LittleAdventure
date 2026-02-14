using UnityEngine;
using Unity.Mathematics;
using System;
using System.Collections.Generic;

public abstract partial class SpatialSystemManager : MonoBehaviour, IDisposable
{
    [Header("Spatial Settings")]
    [SerializeField] protected int3 cellSize = new int3(9, 9, 9);

    protected SpatialDatabase database;

    protected virtual void Awake()
    {
        Initialize();
    }

    protected abstract List<Transform> CollectTargets();

    protected virtual void Initialize()
    {
        var targets = CollectTargets();

        database = new SpatialDatabase(targets, cellSize);
    }

    public virtual SpatialDatabase Database => database;

    public virtual void Dispose()
    {
        database?.Dispose();
    }

    protected virtual void OnDestroy()
    {
        Dispose();
    }
}
