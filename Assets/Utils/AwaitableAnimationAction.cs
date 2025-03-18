using SAS.Utilities.TagSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static SAS.StateMachineGraph.Utilities.AnimatorParameterConfig;

namespace SAS.StateMachineGraph.Utilities
{
    public abstract class AwaitableAnimationAction : IAwaitableStateAction
    {
        public bool IsCompleted { get; protected set; }
        private Actor _actor;
        private Tag _tag;
        private string _key;
        private AnimatorParameterConfig _config;

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _actor = actor;
            _tag = tag;
            _key = key;
            actor.TryGet(out _config);
        }

        async void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            IsCompleted = false;

            if (_config.TryGet(_key, out var parametersKeyMap))
            {
                var animators = _actor.GetComponentsInChildren<Animator>(_tag);
                List<Task> tasks = new List<Task>();
                int count = 0;
                foreach (var animator in animators)
                    tasks.Add(ApplyParameters(animator, parametersKeyMap, count++));

                await Task.WhenAll(tasks);
                IsCompleted = true;
            }
        }

        protected abstract Task WaitForState(Animator animator, string awaitableStateTag);

        async Task ApplyParameters(Animator animator, ParametersKeyMap parametersKeyMap, int count)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(parametersKeyMap.startDelay * count));

                for (int i = 0; i < parametersKeyMap.parameters.Length; ++i)
                    animator.Apply(parametersKeyMap.parameters[i]);

                await WaitForState(animator, parametersKeyMap.awaitableStateTag);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }
    }
}
