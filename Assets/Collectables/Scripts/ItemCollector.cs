using SAS.Collectables;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class ItemCollector : Collector
{
    [FieldRequiresSelf] private IHealthPresenter _healthPresenter;
    [FieldRequiresChild] private IEventDispatcher _eventDispatcher;
    [SerializeField] private string m_HealVFX = "Heal";
    private void Start()
    {
        this.Initialize();
        EventBus<CollectionEvent<Healer>>.Register(new EventBinding<CollectionEvent<Healer>>(val => IncreaseHealth(val)));

    }

    private void OnDestroy()
    {
        EventBus<CollectionEvent<Healer>>.Deregister(new EventBinding<CollectionEvent<Healer>>(val => IncreaseHealth(val)));
    }

    private void IncreaseHealth(CollectionEvent<Healer> collectionEventData)
    {
        if (collectionEventData.collector == this)
        {
            var healer = collectionEventData.collectable;
            _healthPresenter.HealthModel.Increase(healer.Value);
            _eventDispatcher.TriggerEvent(m_HealVFX);
        }
    }
}
