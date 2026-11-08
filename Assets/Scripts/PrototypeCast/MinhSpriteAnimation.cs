using UnityEngine;
using UnityEngine.InputSystem;

namespace SacredDrum.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(Animator), typeof(SpriteRenderer))]
    public sealed class MinhSpriteAnimation : MonoBehaviour
    {
        private PlayerController movement;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private float attackUntil;
        private string currentState;
        private string facing = "Down";
        public string CurrentState => currentState;

        private void Awake()
        {
            movement = GetComponent<PlayerController>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (Time.time < attackUntil) return;
            var direction = movement.LastFacingDirection;
            facing = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? "Side" : direction.y > 0 ? "Up" : "Down";
            spriteRenderer.flipX = direction.x < 0;

            bool attackPressed = (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
            if (attackPressed)
            {
                // This preview plays a staff swing only; it creates no hitbox or damage.
                Play("Attack_" + facing);
                attackUntil = Time.time + .6f;
                return;
            }
            Play((movement.MoveInput.sqrMagnitude > .001f ? "Run_" : "Idle_") + facing);
        }

        private void Play(string state)
        {
            if (currentState == state) return;
            currentState = state;
            animator.Play(Animator.StringToHash(state), 0, 0);
        }

        private void OnDisable()
        {
            currentState = null;
            attackUntil = 0;
        }
    }
}
