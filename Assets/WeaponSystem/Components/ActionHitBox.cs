using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class ActionHitBox : WeaponComponent<ActionHitBoxData, AttackActionHitBox3D>
    {
        public event Action<List<(Collider collider, Vector3 point)>> OnDetectedCollider3D;

        [FieldRequiresParent] private ICharacter _character;
        private Animator _animator;

        private Vector3 offset;
        private Collider[] detected = new Collider[10];

        public override void Init()
        {
            this.Initialize();
            base.Init();
            _animator = _weapon.Animator;
        }

        private void HandleAttackAction()
        {
            Vector3 characterForward = _character.Forward; // Use forward vector for direction

            offset = transform.position +
                     (characterForward * currentAttackData.HitBox.center.z) + // Move in the forward direction
                     (transform.right * currentAttackData.HitBox.center.x) + // Move left/right
                     (transform.up * currentAttackData.HitBox.center.y); // Move up/down


            // Perform the OverlapBoxNonAlloc check
            int hitCount = Physics.OverlapBoxNonAlloc(offset, currentAttackData.HitBox.extents, detected, transform.rotation, data.DetectableLayers);

            // If no colliders are found, exit early
            if (hitCount == 0)
                return;

            // Create a list to store detected colliders and their closest hit points
            List<(Collider, Vector3)> detectedCollisions = new();

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = detected[i];
                Vector3 collisionPoint = collider.ClosestPoint(offset);
                detectedCollisions.Add((collider, collisionPoint));
            }
            OnDetectedCollider3D?.Invoke(detectedCollisions);
        }

        private void Update()
        {
            if (!isAttackActive)
                return;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            float animTime = stateInfo.normalizedTime;

            if (animTime >= currentAttackData.StartTime && animTime <= currentAttackData.EndTime)
                HandleAttackAction();
        }

        private void OnDrawGizmosSelected()
        {
            if (data == null) return;

            foreach (var item in data.GetAllAttackData())
            {
                if (!item.Debug) continue;

                Gizmos.color = Color.red;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(item.HitBox.center, item.HitBox.size);
            }
        }
    }
}
