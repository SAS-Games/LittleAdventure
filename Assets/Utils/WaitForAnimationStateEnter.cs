using SAS.Utilities.TagSystem;
using UniRx;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public class WaitForAnimationStateEnter : IAwaitableStateAction
    {
        private Animator _animator;
        private string _key;
        private Tag _tag;
        private Actor _actor;

        public bool IsCompleted { get; set; }

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _actor = actor;
            _key = key;
            _tag = tag;
            _actor.TryGetComponentInChildren(out _animator, _tag, true);
        }

        async void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            if(_animator ==  null)
                _actor.TryGetComponentInChildren(out _animator, _tag, true);

            IsCompleted = false;
            await _animator.WhenStateEnter(_key).GetAwaiter();
            IsCompleted = true;
        }
    }
}
