using System.Collections;
using EnemySystem;
using SAS.StateMachineCharacterController;
using SAS.Core.TagSystem;
using UnityEngine;

public class EnemyTargetingSystem : MonoBase
{
    [Inject] private ITargetRegistry _targetRegistry;
    [Inject] private IEnemyRegistry _enemyRegistry;
    [SerializeField] private TargetingProfileSO defaultProfile;
    [SerializeField] private float evaluationInterval = 1.5f;
    private YieldInstruction _waitForSeconds;

    private void OnEnable()
    {
        _waitForSeconds = new WaitForSeconds(evaluationInterval);
        StartCoroutine(EvaluateTargetsRoutine());
    }

    private void OnDisable()
    {
        StopCoroutine(EvaluateTargetsRoutine());
    }

    private IEnumerator EvaluateTargetsRoutine()
    {
        while (true)
        {
            foreach (var enemy in _enemyRegistry.Enemies)
            {
                var profile = enemy.TargetingProfile ?? defaultProfile;
                var bestTarget = GetBestTargetFor(enemy, profile);
                if (bestTarget != null)
                    enemy.SetTarget(bestTarget);
            }

            yield return _waitForSeconds;
        }
    }

    private ITarget GetBestTargetFor(Enemy enemy, TargetingProfileSO profile)
    {
        ITarget bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var target in _targetRegistry.Targets)
        {
            if (target.IsActive)
            {
                float score = profile.Evaluate(enemy, target);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = target;
                }
            }
        }

        return bestTarget;
    }
}