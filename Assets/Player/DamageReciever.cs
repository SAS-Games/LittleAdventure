using SAS.StateMachineGraph.Utilities;
using SAS.Utilities.TagSystem;
using System;
using UnityEngine;

public class DamageReciever : MonoBehaviour, IDamageable, IActivatable
{
    [FieldRequiresParent] private IEventDispatcher _eventDispatcher;
    [SerializeField] private string m_EffectEventName = "BeingHit";

    public event Action<float> OnDamageTaken;

    void IActivatable.Activate()
    {
        enabled = true;
    }

    void Awake()
    {
        this.Initialize();
    }

    void IDamageable.Damage(IDamageInfo damageInfo)
    {
        if (!enabled)
            return;
        if (!string.IsNullOrEmpty(m_EffectEventName))
            _eventDispatcher?.TriggerParamEvent(m_EffectEventName, damageInfo.Source.transform.position);

        OnDamageTaken?.Invoke(damageInfo.Amount);
    }

    void IActivatable.Deactivate()
    {
        enabled = false;
    }
}
