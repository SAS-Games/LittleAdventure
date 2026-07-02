using System;

namespace SAS.ActionGraph.WeaponSystem
{
[Serializable]
public class ComboInputAcceptedCondition : ICondition
{
    public bool expected = true;

    public bool Evaluate(ActionContext context)
    {
        var weaponContext = context as WeaponContext;
        return weaponContext != null && weaponContext.ComboInputAccepted == expected;
    }
}
}
