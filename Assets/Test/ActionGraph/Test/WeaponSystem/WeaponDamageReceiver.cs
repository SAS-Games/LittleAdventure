using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
public interface IWeaponDamageable
{
    void Damage(WeaponDamageInfo info);
}

public interface IWeaponDamageModifier
{
    float GetDamageMultiplier();
}

public interface IWeaponKnockbackable
{
    void ApplyKnockback(WeaponKnockbackInfo info);
}

public struct WeaponDamageInfo
{
    public readonly float Amount;
    public readonly GameObject Source;
    public readonly Vector3 Point;

    public WeaponDamageInfo(float amount, GameObject source, Vector3 point)
    {
        Amount = amount;
        Source = source;
        Point = point;
    }
}

public struct WeaponKnockbackInfo
{
    public readonly Vector3 Direction;
    public readonly float Force;
    public readonly GameObject Source;
    public readonly Vector3 Point;

    public WeaponKnockbackInfo(Vector3 direction, float force, GameObject source, Vector3 point)
    {
        Direction = direction;
        Force = force;
        Source = source;
        Point = point;
    }
}

public class WeaponDamageReceiver : MonoBehaviour, IWeaponDamageable, IWeaponKnockbackable
{
    [SerializeField] private float hitPoints = 100f;
    [SerializeField] private bool destroyOnDeath;
    [SerializeField] private Rigidbody targetRigidbody;

    public float HitPoints => hitPoints;

    private void Awake()
    {
        if (targetRigidbody == null)
            targetRigidbody = GetComponent<Rigidbody>();
    }

    public void Damage(WeaponDamageInfo info)
    {
        hitPoints -= info.Amount;

        if (destroyOnDeath && hitPoints <= 0f)
            Destroy(gameObject);
    }

    public void ApplyKnockback(WeaponKnockbackInfo info)
    {
        if (targetRigidbody == null)
            return;

        targetRigidbody.AddForce(info.Direction.normalized * info.Force, ForceMode.Impulse);
    }
}
}
