using UniRx;
using UnityEngine;

public interface IStatModel
{
    void Setup(float value);
    float Max { get; }
    ReactiveProperty<float> Current { get; }
    void Increase(float amount);
    void Decrease(float amount);
    void Reset();
}

public abstract class StatBase : IStatModel
{
    public ReactiveProperty<float> Current { get; protected set; }
    public float Max { get; private set; }


    public virtual void Setup(float max)
    {
        Max = max;
        Current = new ReactiveProperty<float>(max);
    }

    public virtual void Increase(float amount)
    {
        Current.Value = Mathf.Clamp(Current.Value + amount, 0, Max);
    }

    public virtual void Decrease(float amount)
    {
        Current.Value = Mathf.Clamp(Current.Value - amount, 0, Max);
    }

    public virtual void Reset()
    {
        Current.Value = Max;
    }
}
