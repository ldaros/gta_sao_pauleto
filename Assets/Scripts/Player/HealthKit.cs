using System.Collections;
using System.Collections.Generic;
using GTASP.Player;
using UnityEngine;

namespace GTASP
{
    public class HealthKit : MonoBehaviour
    {
        [SerializeField] private int healthAmount = 30;
        [SerializeField] AudioClip healthPickupSound;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var playerHealth = other.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth.CurrentHealth >= 100f)
                {
                    return;
                }
                
                var playerAudioSource = other.gameObject.GetComponent<AudioSource>();
                playerAudioSource.PlayOneShot(healthPickupSound);
                playerHealth.Heal(healthAmount);
                Destroy(gameObject);
            }
        }
    }
}