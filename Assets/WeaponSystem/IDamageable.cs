using System;

public interface IDamageable
{
    event Action<float> OnDamageTaken; 
    void Damage(IDamageInfo damageInfo);
}