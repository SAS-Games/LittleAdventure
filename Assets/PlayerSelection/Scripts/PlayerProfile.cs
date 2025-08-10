using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerProfile
{
    public PlayerInput Input { get; private set; }
    public int Index { get; private set; }
    public string IndexedName { get; private set; }
    public string Name { get; set; } = "Arclen"; 
    public Color Color { get; set; } = Color.white; 
    public GameObject Character { get; set; }

    public PlayerProfile(PlayerInput playerInput)
    {
        Input = playerInput;
        Index = playerInput.playerIndex;
        IndexedName = $"Player_{Index}";
    }


    public PlayerProfile(PlayerInput playerInput, string name, int index)
    {
        Input = playerInput;
        Index = index;
        IndexedName = name;
    }
}