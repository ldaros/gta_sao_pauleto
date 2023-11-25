using UnityEngine;

namespace GTASP.Utils
{
    public class DestroyAfterTime : MonoBehaviour
    {
        public float lifetime = 3.0f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}