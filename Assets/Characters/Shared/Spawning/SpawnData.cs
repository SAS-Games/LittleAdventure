using System;
using UnityEngine;

public readonly struct SpawnData
{
    public SpawnPoint Point { get; }
    public Action<GameObject> OnDespawn { get; }

    public SpawnData(SpawnPoint point, Action<GameObject> onDespawn)
    {
        Point = point;
        OnDespawn = onDespawn;
    }
}
