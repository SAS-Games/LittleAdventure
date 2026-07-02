using System;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    [Serializable]
    public class AttackKnockback : AttackData
    {
        [field: SerializeField] public Vector3 Angle { get; private set; }
        [field: SerializeField] public float Strength { get; private set; }
    }
}