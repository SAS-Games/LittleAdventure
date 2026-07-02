using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponProjectileData
{
    public GameObject projectilePrefab;
    public Vector3 localOffset;
    public Vector3 localDirection = Vector3.forward;
    public float speed = 12f;
    public float damage = 10f;
    public float lifeTime = 5f;
}

[NodeBinding(typeof(SpawnProjectileNode))]
[Serializable]
public class WeaponProjectileProvider : ActionDataProvider<WeaponProjectileData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Spawn Projectile")]
public class SpawnProjectileNode : ActionNode<WeaponProjectileData>
{
    public SpawnProjectileNode(ActionDataProvider<WeaponProjectileData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);
        if (data == null || data.projectilePrefab == null)
            return Task.CompletedTask;

        Transform origin = weaponContext.FirePoint != null ? weaponContext.FirePoint : weaponContext.OriginTransform;
        if (origin == null)
            return Task.CompletedTask;

        Vector3 direction = origin.TransformDirection(data.localDirection);
        if (direction.sqrMagnitude <= 0f)
            direction = origin.forward;
        direction.Normalize();

        Vector3 position = origin.TransformPoint(data.localOffset);
        Quaternion rotation = Quaternion.LookRotation(direction, origin.up);
        GameObject projectile = UnityEngine.Object.Instantiate(data.projectilePrefab, position, rotation);

        var simpleProjectile = projectile.GetComponent<SimpleProjectile>();
        if (simpleProjectile != null)
            simpleProjectile.Launch(weaponContext.Owner, direction, data.speed, data.damage, data.lifeTime);

        var rigidbody = projectile.GetComponent<Rigidbody>();
        if (rigidbody != null)
            rigidbody.linearVelocity = direction * data.speed;

        return Task.CompletedTask;
    }

    private static WeaponContext RequireWeaponContext(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        if (weaponContext == null)
            throw new InvalidOperationException("Spawn projectile node requires WeaponContext.");

        return weaponContext;
    }
}
}


