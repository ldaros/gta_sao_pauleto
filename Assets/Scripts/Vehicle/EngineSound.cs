using System.Collections;
using UnityEngine;

namespace GTASP.Vehicle
{
    public class EngineSound : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VehicleController vehicleController;

        [SerializeField] private AudioClip engineSound;
        [SerializeField] private AudioClip startSound;

        [SerializeField] private float minPitch = 0.5f;
        [SerializeField] private float maxPitch = 1.5f;

        private Coroutine stallCoroutine;

        public void StartEngine(bool value)
        {
            audioSource.PlayOneShot(startSound);

            audioSource.clip = engineSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        public void StopEngine()
        {
            audioSource.Stop();
        }

        private void FixedUpdate()
        {
            float speed = vehicleController.GetSpeed();
            float pitch = Mathf.Lerp(minPitch, maxPitch, speed / 100f);
            audioSource.pitch = pitch;
        }

        public void Stall()
        {
            if (stallCoroutine != null)
            {
                return;
            }

            stallCoroutine = StartCoroutine(StallCoroutine());
        }

        private IEnumerator StallCoroutine()
        {
            audioSource.Stop();
            audioSource.PlayOneShot(startSound);
            yield return new WaitForSeconds(2f);
            stallCoroutine = null;
        }
    }
}