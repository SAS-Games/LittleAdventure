using System;
using UniRx;
using UnityEngine;

public interface IHealthModel
{
    float MaxHealth { get; }
    ReactiveProperty<float> CurrentHealth { get; }
    IObservable<Unit> OnDeath { get; }
    void Decrease(float health);
    void Increase(float health);
    void Reset();
}

public class HealthModel : IHealthModel
{
    private readonly float _maxHealth;
    private readonly Subject<Unit> _onDeath = new Subject<Unit>();
    public IObservable<Unit> OnDeath => _onDeath;
    private bool _isDead;

    public HealthModel(float health)
    {
        CurrentHealth = new ReactiveProperty<float>(health);
        _maxHealth = health;

        CurrentHealth
            .Where(valle => valle <= 0)
            .Where(_ => !_isDead)
            .Subscribe(_ =>
            {
                _isDead = true;
                _onDeath.OnNext(Unit.Default);
            });
    }

    public ReactiveProperty<float> CurrentHealth { get; }

    float IHealthModel.MaxHealth => _maxHealth;

    void IHealthModel.Decrease(float damage)
    {
        var value = CurrentHealth.Value;
        value = Mathf.Clamp(value - damage, 0, _maxHealth);
        CurrentHealth.Value = value;
    }

    void IHealthModel.Increase(float health)
    {
        var value = CurrentHealth.Value;
        value = Mathf.Clamp(value + health, 0, _maxHealth);
        CurrentHealth.Value = value;
    }

    void IHealthModel.Reset()
    {
        CurrentHealth.Value = _maxHealth;
        _isDead = false;
    }
}