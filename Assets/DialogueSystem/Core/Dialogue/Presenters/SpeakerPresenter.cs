using System;
using System.Collections.Generic;
using SAS.DialogueSystem;
using SAS.Core.TagSystem;
using UnityEngine;
using Debug = SAS.Debug;

[RequireComponent(typeof(SpeakerTagProcessor)), DisallowMultipleComponent]
public class SpeakerPresenter : MonoBehaviour
{
    [Serializable]
    private class Speaker
    {
        public string id;
        public SpeakerView view;
    }

    [SerializeField] private List<Speaker> m_Speakers;

    [FieldRequiresParent] protected DialogueHandler _dialogueHandler;

    private readonly Dictionary<string, SpeakerView> _views = new();

    void Awake()
    {
        this.Initialize();

        if (_dialogueHandler != null)
            _dialogueHandler.OnLineReady += OnLineReady;

        RegisterConfiguredSpeakers();
        RegisterChildSpeakerViews();
    }

    private void RegisterConfiguredSpeakers()
    {
        if (m_Speakers == null)
            return;

        foreach (var speaker in m_Speakers)
        {
            if (string.IsNullOrEmpty(speaker.id) || speaker.view == null)
                continue;

            _views[speaker.id] = speaker.view;
        }
    }

    private void RegisterChildSpeakerViews()
    {
        foreach (var view in GetComponentsInChildren<SpeakerView>(true))
        {
            var animatorProcessor = view.GetComponent<IAnimatorProcessor>();
            if (animatorProcessor == null || string.IsNullOrEmpty(animatorProcessor.Tag))
                continue;

            if (!_views.ContainsKey(animatorProcessor.Tag))
                _views.Add(animatorProcessor.Tag, view);
        }
    }

    void OnDestroy()
    {
        if (_dialogueHandler != null)
            _dialogueHandler.OnLineReady -= OnLineReady;
    }

    void OnLineReady(DialogueLineContext lineContext)
    {
        if (lineContext == null)
            return;

        foreach (var view in _views.Values)
            view.gameObject.SetActive(false);

        foreach (var kvp in lineContext.Speakers)
        {
            var speakerId = kvp.Key;
            var state = kvp.Value;

            if (!_views.TryGetValue(speakerId, out var view))
            {
                Debug.LogWarning($"Speaker '{speakerId}' not found.");
                continue;
            }

            view.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(state.Name))
                view.SetName(state.Name);

            if (!string.IsNullOrEmpty(state.Image))
                view.SetImage(state.Image);

            if (!string.IsNullOrEmpty(state.Animation))
                view.SetAnimationState(state.Animation);
        }
    }
}
