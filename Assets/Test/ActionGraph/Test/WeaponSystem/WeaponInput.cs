using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
public class WeaponInput : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    private void Awake()
    {
        if (weapon == null)
            weapon = GetComponent<Weapon>();
    }

    private void Update()
    {
        if (weapon != null && Input.GetKeyDown(attackKey))
            weapon.Attack();
    }
}
}
