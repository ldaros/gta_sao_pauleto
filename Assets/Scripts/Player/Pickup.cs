using GTASP.Game;
using UnityEngine;

namespace GTASP.Player
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupType pickupType;
        [SerializeField] private AudioClip pickupSound;

        private AudioSource audioSource;

        public enum PickupType
        {
            Rifle,
            CarBattery
        }

        private GameState gameState;

        private void Awake()
        {
            gameState = FindObjectOfType<GameState>();
            audioSource = gameState.GetComponent<AudioSource>();
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                switch (pickupType.ToString())
                {
                    case "Rifle":
                        gameState.hasRifle = true;
                        break;
                    case "CarBattery":
                        gameState.hasCarBattery = true;
                        break;
                }
                
                audioSource.PlayOneShot(pickupSound);
                Destroy(gameObject);
            }
        }
    }
}