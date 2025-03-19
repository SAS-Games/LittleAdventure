using System;
using UnityEngine;

namespace SAS.WeaponSystem.Components
{
    [Serializable]
    public class AttackActionHitBox3D : AttackData
    {
        public bool Debug;
        [field: SerializeField] public Bounds HitBox { get; private set; }
        [field: SerializeField] public float StartTime { get; private set; }
        [field: SerializeField] public float EndTime { get; private set; }

    }
}
