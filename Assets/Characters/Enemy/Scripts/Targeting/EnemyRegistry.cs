using System.Collections.Generic;
using SAS.Core.TagSystem;
using UnityEngine;

namespace EnemySystem
{
    public interface IEnemyRegistry : IBindable
    {
        IReadOnlyList<Enemy> Enemies { get; }
        void Register(Enemy enemy);
        void Unregister(Enemy enemy);
    }

    public class EnemyRegistry : IEnemyRegistry
    {
        private readonly List<Enemy> _enemies = new();
        public IReadOnlyList<Enemy> Enemies => _enemies;
        
        public EnemyRegistry(IContextBinder _){}

        void IEnemyRegistry.Register(Enemy enemy)
        {
            if (!_enemies.Contains(enemy))
                _enemies.Add(enemy);
        }

        void IEnemyRegistry.Unregister(Enemy enemy)
        {
            _enemies.Remove(enemy);
        }

        public void Clear() => _enemies.Clear();
    }
}