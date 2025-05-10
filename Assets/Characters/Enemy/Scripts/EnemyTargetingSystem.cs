using System.Collections;
using System.Collections.Generic;
using EnemySystem;
using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class EnemyTargetingSystem : MonoBase
{
    [Inject] ITargetRegistry _targetRegistry;
    [SerializeField] private TargetingProfileSO defaultProfile;
    [SerializeField] private float evaluationInterval = 1.5f;

    private List<Enemy> _activeEnemies = new();
    private YieldInstruction _waitForSeconds;

    public void RegisterEnemy(Enemy enemy)
    {
        if (!_activeEnemies.Contains(enemy))
            _activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        _activeEnemies.Remove(enemy);
    }

    private void OnEnable()
    {
        _waitForSeconds = new WaitForSeconds(evaluationInterval);
        StartCoroutine(EvaluateTargetsRoutine());
    }

    private IEnumerator EvaluateTargetsRoutine()
    {
        while (true)
        {
            foreach (var enemy in _activeEnemies)
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
            float score = profile.Evaluate(enemy, target);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _activeEnemies.Clear();
    }
}