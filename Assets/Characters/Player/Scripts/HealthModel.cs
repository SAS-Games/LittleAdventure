using UniRx;
using UnityEngine;

public interface IHealthModel
{
    float MaxHealth { get; }
    ReactiveProperty<float> CurrentHealth { get; }
    void Decrease(float health);
    void Increase(float health);
    void Reset();
}
public class HealthModel : IHealthModel
{
    private readonly float _maxHealth;
    public HealthModel(float health)
    {
        CurrentHealth = new ReactiveProperty<float>(health);
        _maxHealth = health;
    }

    public ReactiveProperty<float> CurrentHealth { get; }

    float IHealthModel.MaxHealth => _maxHealth;

    void IHealthModel.Decrease(float damage)
    {
        var value = CurrentHealth.Value;
        value = Mathf.Clamp(0, value - damage, _maxHealth);
        CurrentHealth.Value = value;
    }

    void IHealthModel.Increase(float health)
    {
        var value = CurrentHealth.Value;
        value = Mathf.Clamp(0, value + health, _maxHealth);
        CurrentHealth.Value = value;
    }

    void IHealthModel.Reset()
    {
        CurrentHealth.Value = _maxHealth;
    }
}
