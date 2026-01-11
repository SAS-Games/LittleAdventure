using System;
using UniRx;
using SAS.Core.TagSystem;

public interface IHealthModel : IStatModel, IBindable
{
    IObservable<Unit> OnDeath { get; }
}

public class HealthModel : StatBase, IHealthModel
{
    private readonly float _maxHealth;
    private readonly Subject<Unit> _onDeath = new Subject<Unit>();
    public IObservable<Unit> OnDeath => _onDeath;
    private bool _isDead;

    public HealthModel(IContextBinder _) : base() { }

    public override void Setup(float max)
    {
        base.Setup(max);
        Current
            .Where(valle => valle <= 0)
            .Where(_ => !_isDead)
            .Subscribe(_ =>
            {
                _isDead = true;
                _onDeath.OnNext(Unit.Default);
            });
    }

    public override void Reset()
    {
        base.Reset();
        _isDead = false;
    }
}