using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
[RequireComponent(typeof(Collider))]
public class SimpleProjectile : MonoBehaviour
{
    private GameObject _source;
    private Vector3 _direction;
    private float _speed;
    private float _damage;
    private float _lifeTime;
    private float _spawnTime;

    public void Launch(GameObject source, Vector3 direction, float speed, float damage, float lifeTime)
    {
        _source = source;
        _direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        _speed = speed;
        _damage = damage;
        _lifeTime = Mathf.Max(0.01f, lifeTime);
        _spawnTime = Time.time;
    }

    private void Update()
    {
        transform.position += _direction * (_speed * Time.deltaTime);

        if (Time.time - _spawnTime >= _lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Hit(other, other.ClosestPoint(transform.position));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        Hit(collision.collider, point);
    }

    private void Hit(Collider other, Vector3 point)
    {
        if (other == null)
            return;

        if (_source != null && other.transform.root == _source.transform.root)
            return;

        var damageable = other.GetComponentInParent<IWeaponDamageable>();
        if (damageable != null)
            damageable.Damage(new WeaponDamageInfo(_damage, _source, point));

        Destroy(gameObject);
    }
}
}
