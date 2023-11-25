using System.Collections.Generic;
using UnityEngine;

namespace GTASP.Player
{
    public class PlayerOutOfBounds : MonoBehaviour
    {
        [Header("References")]
        public Transform player;

        [Header("Settings")]
        public float maxY = -100f;

        private List<Transform> safeZones;

        private void Start()
        {
            safeZones = FindSafeZones();
        }

        private void Update()
        {
            if (PlayerIsOutOfBounds())
            {
                Transform closestSafeZone = GetClosestSafeZone();
                if (closestSafeZone != null)
                {
                    ReturnPlayerInBounds(closestSafeZone);
                }
            }
        }

        private List<Transform> FindSafeZones()
        {
            List<Transform> foundSafeZones = new List<Transform>();
            foreach (GameObject safeZoneObject in GameObject.FindGameObjectsWithTag("SafeZone"))
            {
                foundSafeZones.Add(safeZoneObject.transform);
            }
            return foundSafeZones;
        }

        private bool PlayerIsOutOfBounds()
        {
            return player.position.y < maxY;
        }

        private Transform GetClosestSafeZone()
        {
            float closestDistance = Mathf.Infinity;
            Transform closestSafeZone = null;

            foreach (Transform safeZone in safeZones)
            {
                float distance = Vector3.Distance(player.position, safeZone.position);
                if (distance < closestDistance)
                {
                    closestSafeZone = safeZone;
                    closestDistance = distance;
                }
            }

            return closestSafeZone;
        }

        private void ReturnPlayerInBounds(Transform safeZone)
        {
            player.position = safeZone.position;
            ResetPlayerVelocity();
            Debug.Log("Player returned to the safe zone.");
        }

        private void ResetPlayerVelocity()
        {
            if (player.TryGetComponent(out Rigidbody rigidBody))
            {
                rigidBody.velocity = Vector3.zero;
            }
        }
    }
}
