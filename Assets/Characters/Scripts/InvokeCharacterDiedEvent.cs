using SAS.StateMachineGraph;
using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.StateMachineCharacterController
{
    public class InvokeCharacterDiedEvent : IStateAction
    {
        private GameObject _character;
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _character = actor.gameObject;
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            EventBus<CharacterDiedEvent>.Raise(new CharacterDiedEvent
            {
                character = _character
            });
        }
    }
}