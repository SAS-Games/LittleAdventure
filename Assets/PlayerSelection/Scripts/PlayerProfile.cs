using UnityEngine.InputSystem;

public class PlayerProfile
{
    public PlayerInput Input { get; private set; }
    public int Index { get; private set; }
    public string Name { get; private set; }

    public PlayerProfile(PlayerInput playerInput)
    {
        Input = playerInput;
        Index = playerInput.playerIndex;
        Name = $"Player_{Index}";
    }


    public PlayerProfile(PlayerInput playerInput, string name, int index)
    {
        Input = playerInput;
        Index = index;
        Name = name;
    }
    
}