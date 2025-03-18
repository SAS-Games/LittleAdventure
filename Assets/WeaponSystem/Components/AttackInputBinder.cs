using SAS.StateMachineCharacterController;
using SAS.WeaponSystem;
using UnityEngine;

public class AttackInputBinder : MonoBehaviour
{
    void Start()
    {
        GetComponent<InputHandler>().CreateInputCommand("Attack", new FireCommand(GetComponent<FSMCharacterController>()), true);
    }
}
