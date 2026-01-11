using UnityEngine;
using System.Collections.Generic;
using SAS.Core.TagSystem;
using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph;
using SAS.StateMachineGraph.Utilities;


[RequireComponent(typeof(IEntityPresenceProvider))]
public abstract class EntityInteractionHandler : MonoBehaviour
{
    [SerializeField] protected Transform[] m_Markers;
    [FieldRequiresSelf] protected IEntityPresenceProvider _presenceProvider;
    protected Collider _proximityCollider;

    protected virtual void Awake()
    {
        this.Initialize();
        _proximityCollider = GetComponent<Collider>();
    }

    protected List<GameObject> GetEntitiesWithinBounds()
    {
        var players = _presenceProvider.GetPresentEntities();
        var result = new List<GameObject>();

        foreach (var entity in players)
        {
            if (_proximityCollider.bounds.Contains(entity.transform.position))
                result.Add(entity);
        }

        return result;
    }

    protected void ApplyParameterToEntities(IEnumerable<GameObject> entities, Parameter actorParam)
    {
        foreach (var obj in entities)
        {
            var actor = obj.GetComponent<Actor>();
            actor?.Apply(in actorParam);
        }
    }

    protected void MoveEntitiesToMarkers(List<GameObject> entities)
    {
        for (int i = 0; i < entities.Count && i < m_Markers.Length; i++)
        {
            var obj = entities[i];
            var controller = obj.GetComponent<FSMCharacterController>();

            if (controller != null)
            {
                controller.SetPosition(m_Markers[i].position);
                obj.transform.rotation = m_Markers[i].rotation;
            }
        }
    }
}
