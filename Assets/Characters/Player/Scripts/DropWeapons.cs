using SAS.Utilities.TagSystem;
using UnityEngine;

public class DropWeapons : MonoBehaviour
{
    [FieldRequiresSelf] private IEventDispatcher _eventDispatcher;
    [SerializeField] private GameObject[] m_Weapons;
    [SerializeField] private string m_weaponDropAnimEventName = "DropWeapon";

    private void Start()
    {
        this.Initialize();
        _eventDispatcher.Subscribe(m_weaponDropAnimEventName, Drop);
    }

    private void Drop()
    {
        foreach (var weapon in m_Weapons)
        {
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
