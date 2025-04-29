using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultSelectedButton : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}