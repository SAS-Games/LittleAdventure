using SAS.StateMachineCharacterController;
using UnityEngine;

public interface IKnockbackable
{
    void PerformAction(ICharacter attacker, Vector3 angle, float strength);
}