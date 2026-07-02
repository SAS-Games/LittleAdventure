using System;
using System.Threading;
using System.Threading.Tasks;
using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponHitBoxData
{
    public Bounds hitBox = new Bounds(new Vector3(0f, 1f, 1f), new Vector3(1.25f, 1f, 1.5f));
    public LayerMask layers = -1;
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
    public bool ignoreOwner = true;
    public bool hitOncePerAttack = true;
    public bool groupHitsByRoot = true;
    public int maxHits = 16;
}

[NodeBinding(typeof(OverlapBoxNode))]
[Serializable]
public class WeaponHitBoxProvider : ActionDataProvider<WeaponHitBoxData>, IIndexedActionDataProvider
{
}

[ActionNodeMenu("Weapon/Overlap Box")]
public class OverlapBoxNode : ActionNode<WeaponHitBoxData>
{
    private Collider[] _results = new Collider[16];

    public OverlapBoxNode(ActionDataProvider<WeaponHitBoxData> dataProvider) : base(dataProvider)
    {
    }

    public override Task ExecuteAsync(ActionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var weaponContext = RequireWeaponContext(context);
        var data = WeaponNodeUtility.GetAttackData(_dataProvider, weaponContext);
        if (data == null)
            return Task.CompletedTask;

        weaponContext.ClearHits();

        if (_results == null || _results.Length != Mathf.Max(1, data.maxHits))
            _results = new Collider[Mathf.Max(1, data.maxHits)];

        Transform origin = weaponContext.OriginTransform;
        if (origin == null)
            return Task.CompletedTask;

        Vector3 center = origin.TransformPoint(data.hitBox.center);
        Vector3 halfExtents = data.hitBox.extents;
        Quaternion rotation = origin.rotation;

        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            _results,
            rotation,
            data.layers,
            data.triggerInteraction);

        for (int i = 0; i < hitCount; i++)
        {
            Collider collider = _results[i];
            if (collider == null)
                continue;

            if (data.ignoreOwner &&
                weaponContext.Owner != null &&
                collider.transform.root == weaponContext.Owner.transform.root)
            {
                continue;
            }

            Vector3 point = collider.ClosestPoint(center);
            if (data.hitOncePerAttack)
                weaponContext.TryRegisterHit(collider, point, data.groupHitsByRoot);
            else
                weaponContext.Hits.Add(new WeaponHit(collider, point));
        }

        return Task.CompletedTask;
    }

    private static WeaponContext RequireWeaponContext(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        if (weaponContext == null)
            throw new InvalidOperationException("Overlap box node requires WeaponContext.");

        return weaponContext;
    }
}
}


