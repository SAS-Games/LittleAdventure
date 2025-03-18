using UnityEngine;

public interface IDamageInfo
{
    float Amount { get; }
    GameObject Source { get; }
}

public class DamageInfo : IDamageInfo
{
    public float Amount { get; }
    public GameObject Source { get; }

    public DamageInfo(float amount, GameObject source)
    {
        Amount = amount;
        Source = source;
    }
}
