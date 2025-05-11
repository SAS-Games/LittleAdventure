using SAS.StateMachineCharacterController;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class Target : MonoBehaviour, ITarget
{
    [Inject] private ITargetRegistry _targetRegistry;
    private Transform _transform;

    Transform IEntity.Transform => _transform;
    Vector3 IEntity.Position => _transform.position;
    Vector3 IEntity.Forward => _transform.forward;
    bool ITarget.IsActive => enabled && gameObject.activeSelf;

    private void Start()
    {
        _transform = transform;
        this.Initialize();
        this._targetRegistry.RegisterTarget(this);
    }

    private void OnEnable()
    {
        this._targetRegistry?.RegisterTarget(this);
    }

    private void OnDisable()
    {
        this._targetRegistry.UnregisterTarget(this);
    }

    private void OnDestroy()
    {
        this._targetRegistry.UnregisterTarget(this);
    }
}