using UnityEngine;
using UnityEngine.InputSystem;

namespace LevelStreaming.TestWorld
{
    /// <summary>
    /// Deterministic XZ-plane movement used by the streaming test world.
    /// Movement is camera-relative but is always projected onto the ground plane.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StreamingTestPlayerController : MonoBehaviour
    {
        [SerializeField] private Camera m_MovementCamera;
        [SerializeField, Min(0f)] private float m_MoveSpeed = 45f;
        [SerializeField, Min(1f)] private float m_BoostMultiplier = 3f;
        [SerializeField] private bool m_FaceMovement = true;
        [SerializeField] private bool m_CreateRuntimeMarker = true;
        [SerializeField] private float m_GroundHeight;

        public Vector3 Velocity { get; private set; }

        private void Awake()
        {
            if (m_MovementCamera == null)
                m_MovementCamera = Camera.main;

            Vector3 position = transform.position;
            position.y = m_GroundHeight;
            transform.position = position;

            if (m_CreateRuntimeMarker && transform.childCount == 0)
                CreateRuntimeMarker();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                Velocity = Vector3.zero;
                return;
            }

            Vector2 input = Vector2.zero;
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            if (m_MovementCamera != null)
            {
                forward = Vector3.ProjectOnPlane(m_MovementCamera.transform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(m_MovementCamera.transform.right, Vector3.up).normalized;
                if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                if (right.sqrMagnitude < 0.001f) right = Vector3.right;
            }

            Vector3 direction = (forward * input.y + right * input.x).normalized;
            float boost = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed
                ? m_BoostMultiplier
                : 1f;
            Velocity = direction * (m_MoveSpeed * boost);

            Vector3 position = transform.position + Velocity * Time.deltaTime;
            position.y = m_GroundHeight;
            transform.position = position;

            if (m_FaceMovement && direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private void OnDisable()
        {
            Velocity = Vector3.zero;
        }

        private void CreateRuntimeMarker()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            marker.name = "Player Marker";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(0f, 3f, 0f);
            marker.transform.localScale = new Vector3(3f, 3f, 3f);

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
                Destroy(markerCollider);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x, m_GroundHeight + 3f, transform.position.z), 3f);
        }
#endif
    }
}
