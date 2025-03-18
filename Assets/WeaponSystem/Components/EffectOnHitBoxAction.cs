using System.Collections.Generic;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    public class EffectOnHitBoxAction : WeaponComponent<EffectOnHitBoxActionData, EmptyAttackData>
    {
        private ActionHitBox _hitBox;
        private IEventDispatcher _eventDispatcher;

        private void HandleDetectCollider(List<(Collider collider, Vector3 point)> colliders)
        {
            foreach (var (collider, point) in colliders)
            {
                //todo: play effect only once 
                _eventDispatcher.TriggerParamEvent(data.EventName, point + Vector3.up * 0.5f);
            }
        }

        public override void Init()
        {
            base.Init();
            _hitBox = GetComponent<ActionHitBox>();
            _eventDispatcher = GetComponentInParent<EventDispatcher>();
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
