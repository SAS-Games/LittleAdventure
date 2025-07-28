using UnityEngine;

public class PrologueEndEventEmitter : MonoBehaviour
{
    public void Emit(string prologueName)
    {
        EventBus<PrologueEndEvent>.Raise(new PrologueEndEvent { name = prologueName });
    }
}
