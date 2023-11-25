using UnityEngine;

namespace GTASP.Animation
{
    public class LockRotation : MonoBehaviour
    {
        public bool x;
        public bool y;
        public bool z;

        private Quaternion rotation;

        private void Awake()
        {
            rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            if (y)
            {
                transform.rotation = Quaternion.Euler(rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
                    rotation.eulerAngles.z);
            }

            if (x)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotation.eulerAngles.y,
                    rotation.eulerAngles.z);
            }

            if (z)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.y,
                    rotation.eulerAngles.z);
            }
        }
    }
}