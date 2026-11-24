using UnityEngine;

namespace SacredDrum.Gameplay
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;
        [SerializeField] private float zOffset = -10f;

        private Vector3 currentVelocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, zOffset);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime
            );
        }
    }
}