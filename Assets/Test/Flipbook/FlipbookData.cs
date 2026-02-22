using UnityEngine;

[CreateAssetMenu(menuName = "Flipbook/Flipbook Data")]
public class FlipbookData : ScriptableObject
{
    public Sprite[] frames;
    public float defaultSpeed = 8f;
}