using UnityEngine;

public class Player_Run : StateMachineBehaviour
{
    private IEventDispatcher _animationEventDispatcher;
    [SerializeField] private CustomParamBool m_CustomParamBool;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_animationEventDispatcher == null)
            _animationEventDispatcher = animator.GetComponent<EventDispatcher>();
        m_CustomParamBool.Param = true;
        _animationEventDispatcher?.TriggerParamEvent(m_CustomParamBool);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_CustomParamBool.Param = false;
        _animationEventDispatcher?.TriggerParamEvent(m_CustomParamBool);
    }
}
