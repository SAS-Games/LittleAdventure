using System;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    [Serializable]
    public class MovementAttackData : AttackData
    {
        [field: SerializeField] public float Velocity { get; private set; }
        [field: SerializeField] public float Duration { get; private set; }

    }
}