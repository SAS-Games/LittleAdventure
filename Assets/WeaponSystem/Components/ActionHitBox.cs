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

        [FieldRequiresParent] private IMovementVectorHandler _movementVectorHandler;

        private Vector3 offset;
        private Collider[] detected;

        public override void Init()
        {
            base.Init();
            this.Initialize();
        }

        private void HandleAttackAction()
        {

            Vector3 characterForward = (_movementVectorHandler as Component).transform.forward; // Use forward vector for direction

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

        protected override void Start()
        {
            base.Start();
            _animationEventDispatcher.Subscribe(data.AnimationEventName, HandleAttackAction);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _animationEventDispatcher.Unsubscribe(data.AnimationEventName, HandleAttackAction);
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
