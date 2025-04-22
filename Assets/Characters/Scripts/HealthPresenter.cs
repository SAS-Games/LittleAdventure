using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public interface IHealthPresenter
{
    IHealthModel HealthModel { get; }
}

public class HealthPresenter : MonoBehaviour, IHealthPresenter
{
    [FieldRequiresSelf] private IProxyView<float> _healthView;
    [FieldRequiresChild] private IDamageable _damageable;
    [SerializeField] private int m_MaxHealth = 100;
    [SerializeField] private UnityEvent m_OnDeath;
    public IHealthModel HealthModel => _healthModel;

    private IHealthModel _healthModel;

    void Awake()
    {
        this.Initialize();
        _healthModel = new HealthModel(m_MaxHealth);
        _healthModel.CurrentHealth.Subscribe(current => { _healthView?.OnValueChanged(current); }).AddTo(this);
        _healthModel.OnDeath.Subscribe(_ => { m_OnDeath?.Invoke(); }).AddTo(this);
    }

    private void HandleDamage(float amount)
    {
        _healthModel.Decrease(amount);
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