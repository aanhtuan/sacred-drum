using UnityEngine;

namespace SacredDrum.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PrototypeDepthSort : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateOrder();
        }

        private void LateUpdate() => UpdateOrder();

        private void UpdateOrder()
        {
            // Map y is -36..0; orders 100..460 stay below the mist at order 1000.
            spriteRenderer.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10f);
        }
    }
}
