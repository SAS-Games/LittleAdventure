using System.Collections.Generic;
using SAS.StateMachineCharacterController;
using SAS.Core.TagSystem;

public interface ITargetRegistry : IBindable, IInitializable
{
    HashSet<ITarget> Targets { get; }
    public void RegisterTarget(ITarget target);
    public void UnregisterTarget(ITarget target);
}


public class TargetRegistry : ITargetRegistry
{
    private HashSet<ITarget> _targets;
    HashSet<ITarget> ITargetRegistry.Targets => _targets;

    public TargetRegistry(IContextBinder _)
    {
    }

    void IInitializable.OnCreated(IContextBinder contextBinder)
    {
        _targets = new HashSet<ITarget>();
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