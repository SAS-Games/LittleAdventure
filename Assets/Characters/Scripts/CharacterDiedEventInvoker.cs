using UnityEngine;
struct CharacterDiedEvent : IEvent
{
    public GameObject character;
}

public class CharacterDiedEventInvoker : MonoBehaviour
{
    public void OnDied()
    {
        EventBus<CharacterDiedEvent>.Raise(new CharacterDiedEvent
        {
            character = this.gameObject
        });
    }

}
