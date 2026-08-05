using SAS.Pool;

namespace SAS.Collectables
{
    public struct CollectionEvent<T> : IEvent where T : BaseCollectable<T>
    {
        public T collectable;
        public Collector collector;
    }

    public abstract class BaseCollectable<T> : Poolable, ICollectable where T : BaseCollectable<T>
    {
        private SelfDespawnable _selfDespawnable;
        
        protected override void Awake()
        {
            base.Awake();
            _selfDespawnable = GetComponent<SelfDespawnable>();
        }

        public virtual void Collect(Collector collector)
        {
            EventBus<CollectionEvent<T>>.Raise(new CollectionEvent<T>
            {
                collectable = (T)this,
                collector = collector
            });
            
            if (_selfDespawnable)
                _selfDespawnable.StartDespawnTimer();
            else
                Despawn();
        }
    }
}
