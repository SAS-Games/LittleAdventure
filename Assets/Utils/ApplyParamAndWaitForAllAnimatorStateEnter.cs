using System;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public class ApplyParamAndWaitForAllAnimatorStateEnter : AwaitableAnimationAction
    {
        protected override async Task WaitForState(Animator animator, string awaitableStateTag)
        {
            try
            {
                await animator.WhenStateEnter(awaitableStateTag).GetAwaiter();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }
    }
}
