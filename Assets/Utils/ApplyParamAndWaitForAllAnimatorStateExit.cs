using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using System.Collections.Generic;
using System;

namespace SAS.StateMachineGraph.Utilities
{
    public class ApplyParamAndWaitForAllAnimatorStateExit : AwaitableAnimationAction
    {
        protected override async Task WaitForState(Animator animator, string awaitableStateTag)
        {
            try
            {
                await animator.WhenStateExit(awaitableStateTag).GetAwaiter();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }
    }
}
