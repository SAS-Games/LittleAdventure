using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class Knockback : WeaponComponent<KnockbackData, AttackKnockback>
    {
        [FieldRequiresParent] private ICharacter _character;
        [FieldRequiresSelf] private ActionHitBox _hitBox;
        private HashSet<GameObject> _hitObjects = new();

        private void HandleDetectCollider(List<(Collider collider, Vector3 point)> colliders)
        {
            foreach (var (collider, point) in colliders)
            {
                if (_hitObjects.Contains(collider.gameObject))
                    continue; // Skip if already hit in this attack

                if (collider.TryGetComponent(out IKnockbackable knockbackable))
                {
                    knockbackable.PerformAction(_character, currentAttackData.Angle, currentAttackData.Strength);
                    _hitObjects.Add(collider.gameObject);
                }
            }
        }

        public override void Init()
        {
            base.Init();
            this.Initialize();
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
