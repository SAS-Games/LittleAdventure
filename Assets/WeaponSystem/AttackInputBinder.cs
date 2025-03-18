using SAS.StateMachineCharacterController;
using UnityEngine;

namespace SAS.WeaponSystem
{
    public class AttackInputBinder : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponentInParent<InputHandler>().CreateInputCommand("Attack", new FireCommand(GetComponentInChildren<FSMCharacterController>()), true);
        }
    }
}
