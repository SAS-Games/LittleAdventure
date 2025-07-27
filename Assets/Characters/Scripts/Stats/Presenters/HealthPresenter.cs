using SAS.Utilities.TagSystem;

public class HealthPresenter : StatPresenter<IHealthModel>
{
    [field: FieldRequiresSelf(tag: Tag.Health)] protected override IProxyView View { get; }
    [FieldRequiresChild] private IDamageable _damageable;

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