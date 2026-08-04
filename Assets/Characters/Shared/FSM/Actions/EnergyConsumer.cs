using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.Core.BlackboardSystem;
using SAS.Core.TagSystem;
using UnityEngine;

public class EnergyConsumer : IStateAction
{
    [Inject] protected IEnergyModel _energyModel;

    [SerializeField] private float _energyCost;
    private BlackboardKey _energyCostKey;

    void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
    {
        actor.Initialize(this);
        actor.TryGet<float>(new BlackboardKey(FSMCharacterBlackboardKey.EnergyCost), out _energyCost);
    }

    void IStateAction.Execute(ActionExecuteEvent executeEvent)
    {
        _energyModel.Decrease(_energyCost);
    }
}