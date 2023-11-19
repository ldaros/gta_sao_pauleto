using System.Collections;
using UnityEngine;

public class SpawnGibs : MonoBehaviour
{
    [SerializeField] private GameObject[] gibPrefabs;
    [SerializeField] private float despawnTime = 5f; // Time in seconds after which gibs will be destroyed
    [SerializeField] private float impulseForce = 5f; // Force of the impulse applied to gibs

    public void SpawnSingleGib(Vector3 position, Quaternion rotation)
    {
        if (gibPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, gibPrefabs.Length);
        GameObject gib = Instantiate(gibPrefabs[randomIndex], position, rotation);

        // Apply random impulse
        Rigidbody gibRigidbody = gib.GetComponent<Rigidbody>();
        if (gibRigidbody != null)
        {
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            gibRigidbody.AddForce(randomDirection * impulseForce, ForceMode.Impulse);
        }

        // Start the despawn coroutine
        StartCoroutine(DespawnGib(gib));
    }

    public void SpawnMultipleGibs(Vector3 position, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            Quaternion randomRotation = Random.rotation;
            SpawnSingleGib(position, randomRotation);
        }
    }

    private IEnumerator DespawnGib(GameObject gib)
    {
        // Wait for the specified despawn time
        yield return new WaitForSeconds(despawnTime);

        // Destroy the gib
        if (gib != null)
        {
            Destroy(gib);
        }
    }
}
