using UnityEngine;

namespace SAS.WeaponSystem
{
    public interface IWeapon
    {
        void Enter();
        void Exit();
        bool IsInUse {  get; }
    }
}
