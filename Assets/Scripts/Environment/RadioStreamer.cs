using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GTASP.Environment
{
    [RequireComponent(typeof(AudioSource))]
    public class RadioStreamer : MonoBehaviour
    {
        [SerializeField] private float volume = 0.3f;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.time = Random.Range(0f, audioSource.clip.length);
        }

        public void EnableRadio()
        {
            StartCoroutine(EnableAfterDelay());
        }

        public void DisableRadio()
        {
            audioSource.volume = 0f;
        }

        private IEnumerator EnableAfterDelay()
        {
            yield return new WaitForSeconds(1.5f);
            audioSource.volume = volume;
        }
    }
}