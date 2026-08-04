using UniRx;
using UnityEngine;

public interface IStatModel
{
    void Setup(float value);
    ReactiveProperty<float> Max { get; }
    ReactiveProperty<float> Current { get; }
    void Increase(float amount);
    void Decrease(float amount);
    void Reset();
    void UpdateMax(float value);
}

public abstract class StatBase : IStatModel
{
    public ReactiveProperty<float> Current { get; protected set; }
    public ReactiveProperty<float> Max { get; protected set; }


    public virtual void Setup(float max)
    {
        Max = new ReactiveProperty<float>(max);
        Current = new ReactiveProperty<float>(max);
    }

    public virtual void Increase(float amount)
    {
        Current.Value = Mathf.Clamp(Current.Value + amount, 0, Max.Value);
    }

    public virtual void Decrease(float amount)
    {
        Current.Value = Mathf.Clamp(Current.Value - amount, 0, Max.Value);
    }

    public void UpdateMax(float value)
    {
        Max.Value = value;
    }

    public virtual void Reset()
    {
        Current.Value = Max.Value;
    }
}