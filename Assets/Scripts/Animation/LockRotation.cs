using UnityEngine;

namespace Animation
{
    public class LockRotation : MonoBehaviour
    {
        private Quaternion rotation;

        private void Awake()
        {
            rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            transform.rotation = rotation;
        }
    }
}