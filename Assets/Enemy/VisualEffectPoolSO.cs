using SAS.Pool;
using UnityEngine.VFX;
using UnityEngine;

[CreateAssetMenu(menuName = "SAS/Pool/VisualEffect Pool")]

public class VisualEffectPoolSO : ComponentPoolSO<VisualEffect>
{
    [SerializeField] private VisualEffectFactory m_Factory;
    protected override IFactory<VisualEffect> Factory => m_Factory;
}
