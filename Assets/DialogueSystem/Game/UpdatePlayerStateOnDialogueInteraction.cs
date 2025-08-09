using SAS.StateMachineGraph.Utilities;
using UnityEngine;

public class UpdatePlayerStateOnDialogueInteraction : EntityInteractionHandler
{
    [SerializeField] protected Parameter m_InteractParamOn;
    [SerializeField] protected Parameter m_InteractParamOff;
    
    public void OnDialogueStart()
    {
        var playersInBounds = GetEntitiesWithinBounds();
        MoveEntitiesToMarkers(playersInBounds);
        ApplyParameterToEntities(_presenceProvider.GetPresentEntities(), m_InteractParamOn);
    }

    public void OnDialogueEnd()
    {
        ApplyParameterToEntities(_presenceProvider.GetPresentEntities(), m_InteractParamOff);
    }
}
