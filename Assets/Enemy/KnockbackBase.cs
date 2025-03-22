using SAS.StateMachineCharacterController;
using SAS.StateMachineGraph.Utilities;
using TMPro;
using UnityEngine;

public abstract class KnockbackBase : MonoBehaviour, IKnockbackable
{
    [SerializeField] private float _knockbackDuration = 0.2f;
    [SerializeField] private Parameter m_AnimParameterConfig;

    private Vector3 _knockbackVelocity;
    private float _knockbackTimer;
    private bool _isKnockbackActive = false;
    private Animator _animator;

    public void PerformAction(ICharacter attacker, Vector3 angle, float strength)
    {
        Vector3 attackDirection = attacker.Forward;

        Vector3 adjustedDirection = (angle + attackDirection) / 2;

        Vector3 knockbackDirection = adjustedDirection.normalized;

        knockbackDirection.y = 0;

        _knockbackVelocity = knockbackDirection * strength;
        _knockbackTimer = _knockbackDuration;
        _isKnockbackActive = true;
        if (_animator == null)
            _animator = GetComponent<Animator>();
        if (!string.IsNullOrEmpty(m_AnimParameterConfig.Name))
            _animator.Apply(m_AnimParameterConfig);

        Debug.Log($"Knockback Applied: Direction={knockbackDirection}, Strength={strength}");
    }

    private void Update()
    {
        if (!_isKnockbackActive)
            return; // Exit early if knockback is not active

        ApplyKnockback(_knockbackVelocity);
        _knockbackTimer -= Time.deltaTime;

        if (_knockbackTimer <= 0)
        {
            _isKnockbackActive = false; // Stop knockback
            _knockbackVelocity = Vector3.zero; // Reset velocity
            ApplyKnockback(_knockbackVelocity);
        }
    }

    protected abstract void ApplyKnockback(Vector3 movement);
}
