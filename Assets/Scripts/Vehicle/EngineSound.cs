using UnityEngine;

namespace Vehicle
{
    public class EngineSound : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VehicleController vehicleController;

        [SerializeField] private AudioClip engineSound;
        [SerializeField] private AudioClip startSound;

        [SerializeField] private float minPitch = 0.5f;
        [SerializeField] private float maxPitch = 1.5f;


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
    }
}