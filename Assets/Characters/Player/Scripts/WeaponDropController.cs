using SAS.Utilities.TagSystem;
using UnityEngine;

public class WeaponDropController : MonoBehaviour
{
    [FieldRequiresSelf] private IEventDispatcher _eventDispatcher;
    [SerializeField] private GameObject[] m_WeaponsContainer;
    [SerializeField] private string m_weaponDropAnimEventName = "DropWeapon";

    private void Start()
    {
        this.Initialize();
        _eventDispatcher.Subscribe(m_weaponDropAnimEventName, Drop);
    }

    private void Drop()
    {
        foreach (var weaponContainer in m_WeaponsContainer)
        {
            var weapon = weaponContainer.transform.GetChild(0).gameObject;
            if (weapon == null)
                continue;
            weapon.AddComponent<Rigidbody>();
            weapon.AddComponent<BoxCollider>();
            weapon.transform.SetParent(null);
        }
    }

    private void OnDestroy()
    {
        _eventDispatcher.Unsubscribe(m_weaponDropAnimEventName, Drop);
    }
}
