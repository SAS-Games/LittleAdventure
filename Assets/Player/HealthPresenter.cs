using SAS.StateMachineGraph.Utilities;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Events;

public interface IHealthPresenter
{
    IHealthModel HealthModel { get; }
}

public class HealthPresenter : MonoBehaviour, IHealthPresenter, IActivatable
{
    [FieldRequiresChild] private IDamageable _damageable;

    [SerializeField] private int m_MaxHealth = 100;
    [SerializeField] private UnityEvent m_OnDeath;
    public IHealthModel HealthModel => _healthModel;

    private IHealthModel _healthModel;

    void Awake()
    {
        this.Initialize();
    }

    void Start()
    {
        _healthModel = new HealthModel(m_MaxHealth);
    }

    private void HandleDamage(float amount)
    {
        _healthModel.Decrease(amount);
        if ((int)_healthModel.CurrentHealth.Value <= 0)
            m_OnDeath.Invoke();
    }


    void IActivatable.Activate()
    {
        enabled = true;
    }

    void IActivatable.Deactivate()
    {
        enabled = false;
    }

    void OnEnable()
    {
        _damageable.OnDamageTaken -= HandleDamage;
        _damageable.OnDamageTaken += HandleDamage;
    }

    void OnDisable()
    {
        _damageable.OnDamageTaken -= HandleDamage;
    }
}
