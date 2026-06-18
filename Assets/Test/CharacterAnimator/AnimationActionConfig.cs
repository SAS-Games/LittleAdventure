using SAS.StringTest;
using UnityEngine;

public enum AnimationCategory
{
    Locomotion,
    Ability,
    Combat,
    Damage,
    Override
}

// public enum AnimationActionId
// {
//     Dash,
//     Attack,
//     Climb,
//     Hurt,
//     Death
// }

[CreateAssetMenu(menuName = "Animation/Action Config")]
public class AnimationActionConfig : ScriptableObject
{
    [ReferenceStringDropdown("Actions")]
    public ReferenceString actionId;

    [Header("Animator")]
    public string animatorStateName;
    public float blendTime = 0.08f;
    public int layer = 0;

    [Header("Classification")]
    public AnimationCategory category;

    [Header("Behaviour")]
    public bool canBeInterrupted = true;
    public bool isLooping = false;
    public float lockDuration = 0f;
}