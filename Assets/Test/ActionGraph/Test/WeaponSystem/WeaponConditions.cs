using System;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class WeaponComboStepCondition : ICondition
{
    public int comboIndex;

    public bool Evaluate(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        return weaponContext != null && weaponContext.CurrentAttackIndex == comboIndex;
    }
}

[Serializable]
public class WeaponHasHitsCondition : ICondition
{
    public bool Evaluate(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        return weaponContext != null && weaponContext.Hits.Count > 0;
    }
}
}
