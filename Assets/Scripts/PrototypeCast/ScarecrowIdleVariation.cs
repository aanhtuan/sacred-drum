using UnityEngine;

namespace SacredDrum.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class ScarecrowIdleVariation : MonoBehaviour
    {
        private void Start()
        {
            // Stable variation from the LDtk placement keeps the crowd from moving in unison.
            float phase = Mathf.Repeat(transform.position.x * .618034f + transform.position.y * .414214f, 1f);
            var animator = GetComponent<Animator>();
            animator.speed = .9f + phase * .2f;
            animator.Play(Animator.StringToHash("HauntedIdle"), 0, phase);
        }
    }
}
