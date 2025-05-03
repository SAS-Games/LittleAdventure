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

        public void PlayFootStepVFX() => m_FootStep.SendEvent("OnPlay");
        public void PlayAttackVFX() => m_AttackVFX.SendEvent("OnPlay");

        public void PlayBeingHitVFX(Vector3 position)
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