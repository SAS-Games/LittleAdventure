using SAS.Utilities.TagSystem;
using UnityEngine;

public class WeaponDropController : MonoBehaviour
{
    [FieldRequiresSelf] private IEventDispatcher _eventDispatcher;
    [SerializeField] private GameObject[] m_WeaponsContainer;
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

    private void RestoreWeaponsToOriginalState()
    {
        for (int i = 0; i < m_Weapons.Length; i++)
        {
            var weapon = m_Weapons[i];
            var container = m_WeaponsContainer[i];

            Destroy(weapon.GetComponent<Rigidbody>());
            Destroy(weapon.GetComponent<BoxCollider>());

            weapon.transform.SetParent(container.transform);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }
    }
}
