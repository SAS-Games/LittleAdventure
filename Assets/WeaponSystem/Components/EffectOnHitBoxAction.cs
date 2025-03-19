using UnityEngine;
using System.Collections.Generic;

namespace SAS.WeaponSystem.Components
{
    public class EffectOnHitBoxAction : WeaponComponent<EffectOnHitBoxActionData, EmptyAttackData>
    {
        private ActionHitBox _hitBox;
        private IEventDispatcher _eventDispatcher;
        private bool _effectPlayed = false; // Track if the effect has been played

        private void HandleDetectCollider(List<(Collider collider, Vector3 point)> colliders)
        {
            if (_effectPlayed)
                return; // Prevent playing the effect multiple times in the same attack

            foreach (var (collider, point) in colliders)
            {
                if (collider.TryGetComponent<IDamageable>(out _))
                {
                    _eventDispatcher.TriggerParamEvent(data.EventName, transform.position + Vector3.up * 0.5f);
                    _effectPlayed = true;
                    break; // Play the effect only once per attack if a damageable is found
                }
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

        protected override void HandleEnter()
        {
            base.HandleEnter();
            _effectPlayed = false; // Reset for the next attack
        }
    }
}
