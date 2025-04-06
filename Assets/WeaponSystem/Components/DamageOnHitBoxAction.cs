using System.Collections.Generic;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class DamageOnHitBoxAction : WeaponComponent<DamageOnHitBoxActionData, AttackDamage>
    {
        private ActionHitBox _hitBox;
        private GameObject _root;
        private HashSet<GameObject> _hitObjects = new();

        private void HandleDetectCollider(List<(Collider collider, Vector3 point)> colliders)
        {
            Debug.Log("HandleDetectCollider");
            foreach (var (collider, point) in colliders)
            {
                if (_hitObjects.Contains(collider.gameObject))
                    continue; // Skip if already hit in this attack

                if (collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.Damage(new DamageInfo(CurrentAttackData.Amount, _root));
                    _hitObjects.Add(collider.gameObject);
                }
            }
        }

        public override void Init()
        {
            base.Init();
            _hitBox = GetComponent<ActionHitBox>();
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
            base.HandleEnter();
            _hitObjects.Clear(); // Reset hit tracking at the start of each attack
        }
    }
}
