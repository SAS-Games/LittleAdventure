using SAS.Pool;
using UnityEngine;
using UnityEngine.VFX;

public class VisualEffectObject : SelfDespawnable
{
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 2;
    protected override void OnSpawn(object data)
    {
        if (data is Vector3 postion)
            transform.position = (Vector3)data + spawnOffset;
        else
            Debug.LogError($"VisualEffectObject.OnSpawn received invalid spawn data. {this.gameObject.name}");
    }

    protected override void OnDespawn()
    {
        transform.position = Vector3.zero;
    }

}
