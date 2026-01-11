using SAS.Core.TagSystem;
using UnityEngine;

namespace EnemySystem
{
    [RequireComponent(typeof(Enemy))]
    public class EnemyTargetingAgent : MonoBehaviour
    {
        [Inject] private IEnemyRegistry _enemyRegistry;
        private Enemy _enemy;

        private void Awake()
        {
            this.Initialize();
            _enemy = GetComponent<Enemy>();
        }
        
        private void OnEnable()
        {
            _enemyRegistry?.Register(_enemy);
        }

        private void OnDisable()
        {
            _enemyRegistry?.Unregister(_enemy);
        }
    }
}