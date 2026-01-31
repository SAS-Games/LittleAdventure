using UnityEngine;

public class XZPlaneMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Keys")]
    [SerializeField] private KeyCode forwardKey = KeyCode.W;
    [SerializeField] private KeyCode backwardKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

    void Update()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(forwardKey))
            direction += Vector3.forward;

        if (Input.GetKey(backwardKey))
            direction += Vector3.back;

        if (Input.GetKey(leftKey))
            direction += Vector3.left;

        if (Input.GetKey(rightKey))
            direction += Vector3.right;

        if (direction.sqrMagnitude > 0f)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}
