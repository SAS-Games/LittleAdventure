using SAS.Pool;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.VFX;

namespace EnemySystem
{
    public class VFXManager : MonoBehaviour
    {
        [SerializeField] private VisualEffectPoolSO m_SplashVFXPoolS0;
        [SerializeField] private VisualEffect m_FootStep;
        [SerializeField] private VisualEffect m_AttackVFX;
        [SerializeField] private ParticleSystem m_BeingHit;

        [FieldRequiresSelf] private IEventDispatcher _eventDispatcher;

        private void Awake()
        {
            this.Initialize();
            _eventDispatcher.Subscribe("BurstFootStep", PlayFootStepVFX);
            _eventDispatcher.Subscribe("PlayAttack", PlayAttackVFX);
            _eventDispatcher.Subscribe<Vector3>("BeingHit", PlayBeingHitVFX);
        }

        private void OnDestroy()
        {
            _eventDispatcher.Unsubscribe("BurstFootStep", PlayFootStepVFX);
            _eventDispatcher.Unsubscribe("PlayAttack", PlayAttackVFX);
            _eventDispatcher.Unsubscribe<Vector3>("BeingHit", PlayBeingHitVFX);
        }

        private void PlayFootStepVFX() => m_FootStep.SendEvent("OnPlay");
        private void PlayAttackVFX() => m_AttackVFX.SendEvent("OnPlay");
        private void PlayBeingHitVFX(Vector3 position)
        {
            var dir = (transform.position - position).normalized;
            dir.y = 0;
            m_BeingHit.transform.rotation = Quaternion.LookRotation(dir);
            m_BeingHit.Play();

            var splashPosition = transform.position;
            var splash = m_SplashVFXPoolS0.Spawn(splashPosition);
            splash.SendEvent("OnPlay");
        }
    }
}
