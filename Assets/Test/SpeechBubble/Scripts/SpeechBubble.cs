using SAS.TimerSystem;
using UnityEngine;
using UnityEngine.Events;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private float m_DisplayDuration = 5f;
    [SerializeField] private UnityEvent m_OnShow;
    [SerializeField] private UnityEvent m_OnHide;
    
    private CountdownTimer _countdownTimer;
    private bool _isShowing;

    public void Show()
    {
        if (_isShowing)
            return;
        _isShowing = true;
        m_OnShow?.Invoke();
        if (m_DisplayDuration > 0)
            ShowUntilTimeExpire();
    }

    public void Unload()
    {
        _countdownTimer?.Dispose();
        _countdownTimer = null;
        _isShowing = false;
    }

    private void ShowUntilTimeExpire()
    {
        _countdownTimer ??= new CountdownTimer(m_DisplayDuration);
        _countdownTimer.Start();
        _countdownTimer.OnTimerStop += () => m_OnHide?.Invoke();
    }
}