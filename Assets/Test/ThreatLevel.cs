using UniRx;
using UnityEngine;

public interface IThreatLevel
{
    ReactiveProperty<int> Value { get; set; }
}

public class ThreatLevel : MonoBehaviour, IThreatLevel
{
    public ReactiveProperty<int> Value { get; set; } = new ReactiveProperty<int>(0);
}