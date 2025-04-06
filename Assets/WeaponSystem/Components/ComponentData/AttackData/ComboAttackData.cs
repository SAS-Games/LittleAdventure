using System;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    [Serializable]
    public class ComboAttackData : AttackData
    {
        [field: SerializeField] public float RequiredAnimationProgress { get; private set; } = 0.7f;
        [field: SerializeField] public string StateTag { get; private set; } = "Attack";
    }
}