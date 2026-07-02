using UnityEngine;

public class ActionContext
{
    public GameObject Owner;
    public IActionGraphBlackboard Blackboard;

    public IActionGraphBlackboard ResolveBlackboard()
    {
        if (Blackboard != null)
            return Blackboard;

        return Owner != null ? Owner.GetComponentInParent<IActionGraphBlackboard>() : null;
    }
}
