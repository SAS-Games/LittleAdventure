using System.Collections.Generic;
using EnemySystem;
using SAS.StateMachineCharacterController;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "AI/Targeting Profile")]
public class TargetingProfileSO : ScriptableObject
{
    [field: SerializeField] private List<TargetEvaluatorSO> m_Evaluators;

    public float Evaluate(Enemy self, ITarget target)
    {
        float totalScore = 0f;
        foreach (var evaluator in m_Evaluators)
        {
            if (evaluator == null) continue;
            totalScore += evaluator.Evaluate(self, target);
        }

        return totalScore;
    }
}