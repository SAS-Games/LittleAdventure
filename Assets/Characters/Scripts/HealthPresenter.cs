using SAS.Utilities.TagSystem;

public interface IHealthPresenter
{
    IHealthModel HealthModel { get; }
}

public class HealthPresenter : StatPresenter<IHealthModel>, IHealthPresenter
{
    [field: FieldRequiresSelf(tag: Tag.Health)] protected override IProxyView<float> View { get; }
    [FieldRequiresChild] private IDamageable _damageable;
    public IHealthModel HealthModel => _model;

    private void HandleDamage(float amount)
    {
        _model.Decrease(amount);
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