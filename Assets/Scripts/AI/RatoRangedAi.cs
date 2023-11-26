using GTASP.Game;
using UnityEngine;

namespace GTASP.AI
{
    public class RatoRangedAi : MonoBehaviour
    {
        // Configurações de perseguição
        public float speed = 5.0f;
        public float rotationSpeed = 10.0f;
        public float visionRange = 20.0f;
        public float attackRange = 10.0f;
        private Transform playerTransform;
        private CharacterController controller;

        // Configurações de disparo
        public GameObject bulletPrefab;
        public Transform firePoint;
        public float bulletSpeed = 20f;
        public float shootingInterval = 2f;
        private float shootTimer;
        private bool isDie = false;

        void Start()
        {
            controller = GetComponent<CharacterController>();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        void Update()
        {
            if (playerTransform != null && controller != null && !isDie)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                Vector3 direction = playerTransform.position - transform.position;
                direction.y = 0;
                direction.Normalize();

                if (distanceToPlayer < attackRange)
                {
                    StopFollowingPlayer();
                    if (direction != Vector3.zero)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation,
                            Time.deltaTime * rotationSpeed);
                    }

                    ShootAtPlayer();
                }
                else if (IsPlayerVisible())
                {
                    FollowPlayer();
                }
            }
        }

        private void StopFollowingPlayer()
        {
            controller.SimpleMove(Vector3.zero);
        }

        private void FollowPlayer()
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0;
            direction.Normalize();

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }

            controller.SimpleMove(direction * speed);
        }

        private void ShootAtPlayer()
        {
            shootTimer += Time.deltaTime;

            if (shootTimer >= shootingInterval)
            {
                Shoot();
                shootTimer = 0;
            }
        }

        private void Shoot()
        {
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 shootingDirection = CalculateShootingDirection();
                    rb.velocity = shootingDirection * bulletSpeed;
                }
            }
        }

        private Vector3 CalculateShootingDirection()
        {
            Vector3 directionToPlayer = playerTransform.position - firePoint.position;
            directionToPlayer.y += 1.0f; // Eleva ligeiramente a direção do tiro
            return directionToPlayer.normalized;
        }

        private bool IsPlayerVisible()
        {
            RaycastHit hit;
            Vector3 directionToPlayer = playerTransform.position - transform.position;

            if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRange))
            {
                return hit.collider.CompareTag("Player");
            }

            return false;
        }

        public void TakeShot()
        {
            Debug.Log("Ai, tomei um tiro!");
            if (!isDie)
            {
                isDie = true;
                Destroy(gameObject);
            }
        }
    }
}