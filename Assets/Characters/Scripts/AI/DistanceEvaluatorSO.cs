using EnemySystem;
using SAS.StateMachineCharacterController;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Evaluators/Distance")]
public class DistanceEvaluatorSO : TargetEvaluatorSO
{
    [SerializeField] private float maxChaseDistance = 15f;

    protected override float ComputeScore(IEntity self, IEntity target)
    {
        float distance = Vector3.Distance(self.Transform.position, target.Transform.position);
        return 1f - Mathf.Clamp01(distance / maxChaseDistance); // Closer = higher score
    }
}