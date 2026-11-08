using UnityEngine;

namespace SacredDrum.Gameplay
{
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class PlayerFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
        [SerializeField] private Rect mapBounds = new Rect(0f, 0f, 88f, 56f);
        private Camera view;
        private Vector3 velocity;

        private void Awake() => view = GetComponent<Camera>();
        private void Start() => SnapToTarget();

        public void SnapToTarget()
        {
            if (target == null) return;
            if (view == null) view = GetComponent<Camera>();
            velocity = Vector3.zero;
            transform.position = ClampToMap(target.position);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            var desired = ClampToMap(target.position);
            var next = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.position = ClampToMap(next);
        }

        private Vector3 ClampToMap(Vector3 point)
        {
            // Account for viewport size, including aspect changes and very wide windows.
            float halfHeight = Mathf.Min(view.orthographicSize, mapBounds.height * 0.5f);
            float halfWidth = Mathf.Min(view.orthographicSize * view.aspect, mapBounds.width * 0.5f);
            return new Vector3(
                Mathf.Clamp(point.x, mapBounds.xMin + halfWidth, mapBounds.xMax - halfWidth),
                Mathf.Clamp(point.y, mapBounds.yMin + halfHeight, mapBounds.yMax - halfHeight),
                transform.position.z);
        }
    }
}
