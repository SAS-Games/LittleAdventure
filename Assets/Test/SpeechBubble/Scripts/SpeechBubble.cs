using SAS.Utilities;
using System.Collections;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private float m_DisplayDuration = 5;

    private Coroutine _routine;

    public void Show()
    {
        if (m_Animator)
            m_Animator.SetTrigger("show");

        if (m_DisplayDuration > 0)
        {
            _routine = StaticCoroutine.Start(ShowUntilTimeExpire());
        }
    }

    public void Hide()
    {
        if (m_Animator)
            m_Animator.SetTrigger("hide");
        else
            Unload();
    }

    //called from animation event as well
    private void Unload()
    {
        if (_routine != null)
        {
            StaticCoroutine.Stop(_routine);
            _routine = null;
        }
    }

    IEnumerator ShowUntilTimeExpire()
    {
        if (m_DisplayDuration > 0)
        {
            yield return new WaitForSeconds(m_DisplayDuration);
            Unload();
        }
    }
}