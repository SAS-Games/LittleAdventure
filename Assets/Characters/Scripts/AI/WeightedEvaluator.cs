using EnemySystem;
using SAS.StateMachineCharacterController;
using UnityEngine;

public interface ITargetEvaluator
{
    float Evaluate(IEntity self, IEntity target);
}

public abstract class TargetEvaluatorSO : ScriptableObject, ITargetEvaluator
{
    [Range(-10f, 10f)] public float weight = 1f;

    public float Evaluate(IEntity self, IEntity target)
    {
        return weight * ComputeScore(self, target);
    }

    protected abstract float ComputeScore(IEntity self, IEntity target);
}