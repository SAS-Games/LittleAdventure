using SAS.Utilities.TagSystem;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SAS.StateMachineGraph.Utilities
{
    public class WaitForButtonClick : IAwaitableStateAction
    {
        public bool IsCompleted { get; private set; }

        private Button _button;
        private Tag _tag;
        private Actor _actor;
        private UnityAction _action;

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _tag = tag;
            _actor = actor;
        }

        async void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            if (_button == null)
                _button = _actor.GetComponentInChildren<Button>(_tag, true);

            IsCompleted = false;
            var tcs = new TaskCompletionSource<bool>();

            _action = () =>
            {
                tcs.TrySetResult(true);
                _button.onClick.RemoveListener(_action);
            };

            _button.onClick.AddListener(_action);

            await tcs.Task;
            IsCompleted = true;
        }
    }
}
