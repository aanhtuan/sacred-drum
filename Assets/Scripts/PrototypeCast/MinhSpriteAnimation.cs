using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SacredDrum.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController), typeof(Animator), typeof(SpriteRenderer))]
    public sealed class MinhSpriteAnimation : MonoBehaviour
    {
        private readonly Dictionary<string, float> durations = new Dictionary<string, float>();
        private PlayerController movement;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private float actionUntil;
        private string currentState;
        public string CurrentState => currentState;

        private void Awake()
        {
            movement = GetComponent<PlayerController>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable() => CacheDurations();

        private void CacheDurations()
        {
            if (animator == null) animator = GetComponent<Animator>();
            durations.Clear();
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
                durations[clip.name] = clip.length;
        }

        private void LateUpdate()
        {
            if (Time.time < actionUntil) return;
            if (currentState == "CrouchDown") Play("CrouchHold");
            var keyboard = Keyboard.current;
            bool crouch = keyboard != null && (keyboard.cKey.isPressed || keyboard.leftCtrlKey.isPressed);
            if (crouch)
            {
                if (currentState != "CrouchHold") PlayAction("CrouchDown");
                return;
            }
            if (currentState == "CrouchHold") { PlayAction("CrouchUp"); return; }
            var direction = movement.LastFacingDirection;
            string facing = Mathf.Abs(direction.x) > .01f ? "Side" : direction.y > 0 ? "Up" : "";
            spriteRenderer.flipX = facing == "Side" && direction.x < 0;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                // A visual hop only; the original top-down controller and collisions stay on the ground.
                PlayAction("Jump");
                return;
            }
            bool attack = (keyboard != null && keyboard.jKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
            if (attack) { PlayAction("StaffAttack"); return; }
            Play((movement.MoveInput.sqrMagnitude > .001f ? "Run" : "Idle") + facing);
        }

        private void PlayAction(string state)
        {
            // Managed dictionaries are not restored by a Play Mode script reload.
            if (!durations.TryGetValue(state, out float duration))
            {
                CacheDurations();
                if (!durations.TryGetValue(state, out duration))
                {
                    Debug.LogError("Missing Minh animation clip: " + state, this);
                    return;
                }
            }
            Play(state, true);
            actionUntil = Time.time + duration;
        }

        private void Play(string state, bool restart = false)
        {
            if (!restart && currentState == state) return;
            currentState = state;
            animator.Play(Animator.StringToHash(state), 0, 0);
        }

        private void OnDisable()
        {
            currentState = null;
            actionUntil = 0;
        }
    }
}
