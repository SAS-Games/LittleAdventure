using UnityEngine;
using UnityEngine.InputSystem;

public class XZPlaneMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Input")]
    [SerializeField] private InputAction moveAction;

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        if (input.sqrMagnitude < 0.001f)
            return;

        Vector3 direction = new Vector3(input.x, 0f, input.y);

        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}