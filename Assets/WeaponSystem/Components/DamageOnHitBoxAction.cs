using System.Collections.Generic;
using UnityEngine;
using Debug = SAS.Debug;

namespace SAS.WeaponSystem.Components
{
    public class DamageOnHitBoxAction : WeaponComponent<DamageOnHitBoxActionData, AttackDamage>
    {
        private ActionHitBox _hitBox;
        private GameObject _root;
        private HashSet<GameObject> _hitObjects = new();
        private IDamageModifier _damageModifier;


        private void HandleDetectCollider(List<(Collider collider, Vector3 point)> colliders)
        {
            Debug.Log($"HandleDetectCollider: {Time.frameCount}");

            foreach (var (collider, point) in colliders)
            {
                if (_hitObjects.Contains(collider.gameObject))
                    continue; // Skip if already hit in this attack

                if (collider.TryGetComponent(out IDamageable damageable))
                {
                    var damageValue = ApplyUpgradeModifiers(CurrentAttackData.Amount);
                    damageable.Damage(new DamageInfo(damageValue, _root));
                    _hitObjects.Add(collider.gameObject);
                }
            }
        }

        private float ApplyUpgradeModifiers(float baseDamage)
        {
            if (_damageModifier != null)
            {
                float multiplier = _damageModifier.GetDamageMultiplier();
                return baseDamage * multiplier;
            }

            return baseDamage;
        }

        public override void Init()
        {
            base.Init();
            _hitBox = GetComponent<ActionHitBox>();
            _damageModifier = GetComponentInParent<IDamageModifier>();
            _root = this.transform.root.gameObject;
        }

        protected override void Start()
        {
            base.Start();
            if (_hitBox != null)
                _hitBox.OnDetectedCollider3D += HandleDetectCollider;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_hitBox != null)
                _hitBox.OnDetectedCollider3D -= HandleDetectCollider;
        }

        protected override void HandleEnter()
        {
            _hitObjects.Clear(); // Reset hit tracking
            base.HandleEnter();

        }
    }
}
