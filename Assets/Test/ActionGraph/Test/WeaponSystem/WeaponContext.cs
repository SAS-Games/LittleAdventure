using System.Collections.Generic;
using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
public class WeaponContext : ActionContext
{
    private readonly HashSet<GameObject> _hitObjects = new HashSet<GameObject>();

    public Weapon Weapon { get; internal set; }
    public ComboWeapon ComboWeapon { get; internal set; }
    public Animator Animator { get; internal set; }
    public Transform FirePoint { get; internal set; }
    public Transform HitOrigin { get; internal set; }
    public int CurrentAttackIndex { get; internal set; }
    public bool ComboInputAccepted { get; set; }
    public bool IsAttackRunning { get; internal set; }
    public int AttackInputVersion { get; private set; }
    public float LastAttackInputTime { get; private set; } = -999f;
    public float LastAcceptedAttackInputTime { get; private set; } = -999f;
    public List<WeaponHit> Hits { get; } = new List<WeaponHit>();

    public Transform OriginTransform
    {
        get
        {
            if (HitOrigin != null)
                return HitOrigin;

            if (FirePoint != null)
                return FirePoint;

            return Owner != null ? Owner.transform : null;
        }
    }

    public void BeginAttack(int attackIndex)
    {
        CurrentAttackIndex = attackIndex;
        ComboInputAccepted = false;
        IsAttackRunning = true;
        Hits.Clear();
        _hitObjects.Clear();
    }

    public void EndAttack()
    {
        IsAttackRunning = false;
    }

    public void ResetCombo()
    {
        CurrentAttackIndex = 0;
        ComboInputAccepted = false;
        IsAttackRunning = false;
        Hits.Clear();
        _hitObjects.Clear();
    }

    public void RecordAttackInput(float time)
    {
        AttackInputVersion++;
        LastAttackInputTime = time;
    }

    public void MarkAttackInputAccepted(float time)
    {
        LastAcceptedAttackInputTime = time;
    }

    public void ClearHits()
    {
        Hits.Clear();
    }

    public bool TryRegisterHit(Collider collider, Vector3 point, bool groupByRoot)
    {
        if (collider == null)
            return false;

        GameObject hitObject = groupByRoot && collider.transform.root != null
            ? collider.transform.root.gameObject
            : collider.gameObject;

        if (_hitObjects.Contains(hitObject))
            return false;

        _hitObjects.Add(hitObject);
        Hits.Add(new WeaponHit(collider, point));
        return true;
    }
}

public struct WeaponHit
{
    public readonly Collider Collider;
    public readonly Vector3 Point;

    public WeaponHit(Collider collider, Vector3 point)
    {
        Collider = collider;
        Point = point;
    }
}
}
