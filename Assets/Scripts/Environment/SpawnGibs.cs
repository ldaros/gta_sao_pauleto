using UnityEngine;

namespace GTASP.Environment
{
    public class SpawnGibs : MonoBehaviour
    {
        [SerializeField] private GameObject[] gibPrefabs;
        [SerializeField] private float despawnTime = 5f; // Time in seconds after which gibs will be destroyed
        [SerializeField] private float impulseForce = 5f; // Force of the impulse applied to gibs

        private void SpawnSingleGib(Vector3 position, Quaternion rotation, float size = 1f)
        {
            if (gibPrefabs.Length == 0) return;

            int randomIndex = Random.Range(0, gibPrefabs.Length);
            GameObject gib = Instantiate(gibPrefabs[randomIndex], position, rotation);
            gib.transform.localScale = Vector3.one * size;

            // Apply random impulse
            Rigidbody gibRigidbody = gib.GetComponent<Rigidbody>();
            if (gibRigidbody != null)
            {
                Vector3 randomDirection = Random.insideUnitSphere.normalized;
                gibRigidbody.AddForce(randomDirection * impulseForce, ForceMode.Impulse);
            }

            Destroy(gib, despawnTime);
        }

        public void SpawnMultipleGibs(Vector3 position, int quantity, float size = 1f)
        {
            for (int i = 0; i < quantity; i++)
            {
                Quaternion randomRotation = Random.rotation;
                SpawnSingleGib(position, randomRotation, size);
            }
        }
    }
}