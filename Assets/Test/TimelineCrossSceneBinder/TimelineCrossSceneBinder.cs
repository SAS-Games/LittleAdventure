using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;
using Debug = SAS.Debug;

[DisallowMultipleComponent, RequireComponent(typeof(PlayableDirector))]
public class TimelineCrossSceneBinder : MonoBehaviour
{
    [Serializable]
    public class TrackBindingInfo
    {
        public Object timelineTrack;
        public GuidReference guidReference = null;
    }

    [SerializeField] private PlayableDirector m_Director;
    [SerializeField] private List<TrackBindingInfo> m_Bindings = new();

    public PlayableDirector Director => m_Director;
    public List<TrackBindingInfo> Bindings => m_Bindings;

    private void Reset()
    {
        if (!m_Director)
            m_Director = GetComponent<PlayableDirector>();
    }
    
    private void OnValidate()
    {
        if (!m_Director)
            m_Director = GetComponent<PlayableDirector>();
    }

    /// <summary>
    /// Attempts to bind all stored references to their corresponding Timeline tracks.
    /// </summary>
    public void BindAll()
    {
        if (m_Director == null)
        {
            Debug.LogError("PlayableDirector is missing!");
            return;
        }

        foreach (var info in m_Bindings)
        {
            if (info.timelineTrack == null)
                continue;

            // Try to resolve the object from the loaded scene
            var obj = info.guidReference.gameObject;

            // Make sure the track is valid
            if (info.timelineTrack is not TrackAsset trackAsset)
            {
                Debug.LogWarning($"'{info.timelineTrack.name}' is not a valid TrackAsset. Skipping...");
                continue;
            }

            var output = trackAsset.outputs.FirstOrDefault();
            var expectedType = output.outputTargetType;

            Object bindingToAssign = null;

            if (expectedType == typeof(GameObject))
                bindingToAssign = obj;
            else if (typeof(Component).IsAssignableFrom(expectedType))
            {
                var component = obj.GetComponent(expectedType);
                if (component != null)
                    bindingToAssign = component;
                else
                {
                    Debug.LogWarning($"Object '{obj.name}' has no component of type {expectedType.Name} for track '{info.timelineTrack.name}'");
                    continue;
                }
            }
            else
            {
                Debug.LogWarning($"Unexpected binding type '{expectedType}' for track '{info.timelineTrack.name}'");
                continue;
            }

            // Assign the correct binding
            m_Director.SetGenericBinding(trackAsset, bindingToAssign);
            Debug.Log($"Bound '{info.timelineTrack.name}' → {bindingToAssign.name} ({expectedType.Name})");
        }

        m_Director.RebuildGraph();
        Debug.Log("Timeline cross-scene binding completed.");
    }
}