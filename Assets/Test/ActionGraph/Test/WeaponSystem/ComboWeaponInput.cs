using UnityEngine;

namespace SAS.ActionGraph.WeaponSystem
{
public class ComboWeaponInput : MonoBehaviour
{
    [SerializeField] private ComboWeapon weapon;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    private void Awake()
    {
        if (weapon == null)
            weapon = GetComponent<ComboWeapon>();
    }

    private void Update()
    {
        if (weapon != null && Input.GetKeyDown(attackKey))
            weapon.SetAttackInput(true);
    }
}
}
