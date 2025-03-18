using System.Collections.Generic;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class DamageOnHitBoxAction : WeaponComponent<DamageOnHitBoxActionData, AttackDamage>
    {
        private ActionHitBox _hitBox;
        private GameObject Root;

        private void HandleDetectCollider(List<(Collider collider, Vector3 point)> colliders)
        {
            foreach (var (collider, point) in colliders)
            {
                if (collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.Damage(new DamageInfo(currentAttackData.Amount, Root));
                }
            }
        }

        public override void Init()
        {
            base.Init();
            _hitBox = GetComponent<ActionHitBox>();
            Root = this.transform.root.gameObject;
        }

        protected override void Start()
        {
            base.Start();

            _hitBox.OnDetectedCollider3D += HandleDetectCollider;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _hitBox.OnDetectedCollider3D -= HandleDetectCollider;
        }
    }
}
