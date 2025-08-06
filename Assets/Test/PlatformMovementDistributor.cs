using SAS.StateMachineCharacterController;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class PlatformMovementDistributor : MonoBehaviour
{
    private readonly HashSet<IMovementVectorHandler> _characters = new();
    private Vector3 _lastPosition;

    private void Awake()
    {
        _lastPosition = transform.position;
        enabled = false;
    }

    public void OnCharacterEnter(GameObject obj)
    {
        if (obj.TryGetComponent<IMovementVectorHandler>(out var handler) && _characters.Add(handler))
        {
            if(!enabled)
                _lastPosition = transform.position;
            enabled = true;
        }
    }

    public void OnCharacterExit(GameObject obj)
    {
        if (obj.TryGetComponent<IMovementVectorHandler>(out var handler))
        {
            _characters.Remove(handler);
            if (_characters.Count == 0)
                enabled = false;
        }
    }

    private void Update()
    {
        Vector3 platformVelocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        foreach (var handler in _characters)
            handler.MovementVector += platformVelocity;
    }
}
