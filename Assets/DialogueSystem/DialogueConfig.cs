using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Config")]
public class DialogueConfig : ScriptableObject
{
    [Header("Timing")]
    public float defaultCharacterDelay = 0.05f;
    public bool autoAdvance;
    public float autoAdvanceDelay = 2f;
    
    [Header("Resources")]
    public GameObject defaultSpeakerPrefab;
    public GameObject choiceButtonPrefab;
    
    [Header("Localization")]
    public string defaultTextTable = "Dialogue";

}