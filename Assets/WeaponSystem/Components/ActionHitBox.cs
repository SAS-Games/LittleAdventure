using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SAS.WeaponSystem.Components
{
    public class ActionHitBox : WeaponComponent<ActionHitBoxData, AttackActionHitBox3D>
    {
        public event Action<List<(Collider collider, Vector3 point)>> OnDetectedCollider3D;

        [FieldRequiresParent] private ICharacter _character;
        [FieldRequiresParent] private Animator _animator;

        private Vector3 offset;
        private Collider[] detected;

        public override void Init()
        {
            base.Init();
            this.Initialize();
        }

        private void HandleAttackAction()
        {
            Vector3 characterForward = _character.Forward; // Use forward vector for direction

            offset = transform.position +
                     (characterForward * currentAttackData.HitBox.center.z) + // Move in the forward direction
                     (transform.right * currentAttackData.HitBox.center.x) + // Move left/right
                     (transform.up * currentAttackData.HitBox.center.y); // Move up/down

            detected = Physics.OverlapBox(offset, currentAttackData.HitBox.extents, transform.rotation, data.DetectableLayers);

            if (detected.Length == 0)
                return;

            Debug.Log("HandleAttackAction");

            List<(Collider, Vector3)> detectedCollisions = new();

            foreach (var collider in detected)
            {
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
