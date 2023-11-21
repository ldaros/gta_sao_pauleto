using UnityEngine;

namespace UI
{
    public class MinimapCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float height = 20f;
        [SerializeField] private float distance = 20f;
        [SerializeField] private float angle = 45f;

        private void LateUpdate()
        {
            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            var position = target.position;
            
            Vector3 offset = Quaternion.Euler(angle, 0, 0) * new Vector3(0, 0, -distance);
            Vector3 targetPosition = position + offset;
            targetPosition.y = height;

            transform.position = targetPosition;
            transform.LookAt(position);
        }
    }
}