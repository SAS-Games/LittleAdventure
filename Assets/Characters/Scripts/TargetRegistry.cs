using System.Collections.Generic;
using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;

public interface ITargetRegistry : IBindable
{
    List<ITarget> Targets { get; }
    public void RegisterTarget(ITarget target);
    public void UnregisterTarget(ITarget target);
}


public class TargetRegistry : ITargetRegistry
{
    private List<ITarget> _targets;
    List<ITarget> ITargetRegistry.Targets => _targets;

    public TargetRegistry(IContextBinder _)
    {
    }

    void IBindable.OnInstanceCreated()
    {
        _targets = new List<ITarget>();
    }

    void ITargetRegistry.RegisterTarget(ITarget target)
    {
        if (target != null)
            _targets.Add(target);
    }

    void ITargetRegistry.UnregisterTarget(ITarget target)
    {
        if (target != null)
            _targets.Remove(target);
    }
}