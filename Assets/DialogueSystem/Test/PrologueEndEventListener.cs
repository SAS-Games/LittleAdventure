using SAS.StateMachineGraph;
using UnityEngine;

public class PrologueEndEventListener : MonoBehaviour
{
    EventBinding<PrologueEndEvent> prologueEndEventBinding;
    private void OnEnable()
    {
        prologueEndEventBinding = new EventBinding<PrologueEndEvent>(LoadGameScene);
        EventBus<PrologueEndEvent>.Register(prologueEndEventBinding);
    }

    private void OnDisable()
    {
        EventBus<PrologueEndEvent>.Deregister(prologueEndEventBinding);
    }

    void LoadGameScene(PrologueEndEvent prologueEndEvent)
    {
        GetComponent<Actor>().SetTrigger("LoadScene");
    }
}
