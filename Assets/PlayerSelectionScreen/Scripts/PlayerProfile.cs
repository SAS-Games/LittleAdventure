using UnityEngine.InputSystem;

public class PlayerProfile
{
    public PlayerProfile(PlayerInput playerInput)
    {
        Index = playerInput.playerIndex;
        Input = playerInput;
        Name = $"Player_{playerInput.playerIndex}";
    }

    public PlayerInput Input { get; private set; }
    public int Index { get; private set; }
    public string Name { get; private set; }
}