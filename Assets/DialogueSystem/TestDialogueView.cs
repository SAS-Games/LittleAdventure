using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestDialogueWidget : MonoBehaviour, IDialogueWidget
{
    [SerializeField] private InputActionReference m_InputActionReference;

    public event Action OnContinuePressed;

    private void Awake()
    {
        m_InputActionReference.action.performed += OnInputPerformed;
        m_InputActionReference.action.Enable();
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        OnClick();
    }

    public void OnClick()
    {
        OnContinuePressed?.Invoke();
    }

    public void ShowDialogue()
    {
        Debug.Log("Showing dialogue");
    }

    public void HideDialogue()
    {
        Debug.Log("Hiding dialogue");
    }

    public IEnumerator DisplayLine(string text)
    {
        Debug.Log(text);
        yield break;
    }
    

    public void UpdateSpeaker(SpeakerState speaker)
    {
        Debug.Log("Updating speaker");
    }

    public IEnumerator RunOperations(IEnumerable<IEnumerator> operations, Action onComplete)
    {
        Debug.Log("Running operations");
        yield return null;
    }
}