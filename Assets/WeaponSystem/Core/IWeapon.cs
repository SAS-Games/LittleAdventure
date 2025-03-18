using UnityEngine;

namespace SAS.WeaponSystem
{
    public interface IWeapon
    {
        void Init();
        void Enter();
        void Exit();
        bool IsInUse {  get; }
    }
}
