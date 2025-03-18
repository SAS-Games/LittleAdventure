using UnityEngine;
using UnityEngine.VFX;

public class VisualEffectObject : SelfDespawnable<VisualEffect>
{
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 2;
    protected override void OnSpawn(object data)
    {
        transform.position = (Vector3)data + spawnOffset;
    }

    protected override void OnDespawn()
    {
        transform.position = Vector3.zero;
    }

}
