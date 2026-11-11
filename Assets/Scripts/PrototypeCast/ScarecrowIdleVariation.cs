using UnityEngine;

namespace SacredDrum.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class ScarecrowIdleVariation : MonoBehaviour
    {
        public enum Facing { Down, Right, Up, Left }
        [SerializeField] private Facing facing;
        private Animator animator;
        private float returnToIdleAt;
        public string CurrentState { get; private set; }

        private void Start()
        {
            animator = GetComponent<Animator>();
            float phase = Mathf.Repeat(transform.position.x * .618034f + transform.position.y * .414214f, 1f);
            animator.speed = .9f + phase * .2f;
            PlayVisual("Idle", phase);
        }

        private void Update()
        {
            if (returnToIdleAt > 0 && Time.time >= returnToIdleAt) PreviewIdle();
        }

        // Visual previews only: no movement, targeting, health, hitboxes or damage.
        public void Preview(string action, Facing direction)
        {
            facing = direction;
            PlayVisual(action, 0);
        }

        private void PlayVisual(string action, float phase)
        {
            if (animator == null) animator = GetComponent<Animator>();
            CurrentState = action + facing;
            animator.Play(Animator.StringToHash(CurrentState), 0, phase);
            returnToIdleAt = 0;
            if (action != "Attack" && action != "Hurt") return;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
                if (clip.name == CurrentState)
                    returnToIdleAt = Time.time + clip.length / Mathf.Max(.01f, animator.speed);
        }

        [ContextMenu("Visual Preview/Idle")]
        public void PreviewIdle() => PlayVisual("Idle", 0);
        [ContextMenu("Visual Preview/Walk in place")]
        public void PreviewWalk() => PlayVisual("Walk", 0);
        [ContextMenu("Visual Preview/Attack")]
        public void PreviewAttack() => PlayVisual("Attack", 0);
        [ContextMenu("Visual Preview/Hurt")]
        public void PreviewHurt() => PlayVisual("Hurt", 0);
        [ContextMenu("Visual Preview/Die")]
        public void PreviewDie() => PlayVisual("Die", 0);
    }
}
