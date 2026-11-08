using UnityEngine;
using UnityEngine.InputSystem;

namespace SacredDrum.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        [SerializeField] private InputActionAsset inputActions;
        private Rigidbody2D body;
        private InputAction moveAction;
        public Vector2 MoveInput { get; private set; }
        public Vector2 LastFacingDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            // Own only a clone of the existing Move action; other gameplay actions stay disabled.
            if (inputActions != null)
                moveAction = inputActions.FindAction("Player/Move", true).Clone();
            else
                Debug.LogError("PlayerController requires the project's input action asset.", this);
        }

        private void OnEnable() => moveAction?.Enable();
        private void OnDisable()
        {
            moveAction?.Disable();
            MoveInput = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }
        private void OnDestroy() => moveAction?.Dispose();

        private void Update()
        {
            MoveInput = moveAction == null ? Vector2.zero : Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
            if (MoveInput.sqrMagnitude > 0.001f)
            {
                // Store a cardinal facing even while travelling diagonally.
                LastFacingDirection = Mathf.Abs(MoveInput.x) > Mathf.Abs(MoveInput.y)
                    ? new Vector2(Mathf.Sign(MoveInput.x), 0f)
                    : new Vector2(0f, Mathf.Sign(MoveInput.y));
            }
        }

        private void FixedUpdate() => body.linearVelocity = MoveInput * moveSpeed;
        private void OnValidate() => moveSpeed = Mathf.Max(0f, moveSpeed);
    }
}
