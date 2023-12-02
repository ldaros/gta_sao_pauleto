using System.Collections;
using GTASP.Animation;
using GTASP.Environment;
using GTASP.Game;
using GTASP.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace GTASP.AI
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkPointRange;
        [SerializeField] private float waitTime = 1f;
        [SerializeField] private float outOfBoundsY = -10f;
        [SerializeField] private bool canMove = true;
        [SerializeField] private bool canPatrol = true;

        [Header("Combat")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float viewRadius = 10f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float gibAfter = 30;
        [SerializeField] private float maxHealth = 10f;

        [Header("To Ranged Enemies")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject bulletPrefab2;
        [SerializeField] private GameObject bulletPrefab3;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform firePointMeteor1;
        [SerializeField] private Transform firePointMeteor2;
        [SerializeField] private Transform firePointMeteor3;
        [SerializeField] private float bulletSpeed = 20.0f;

        [Header("Sound")]
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private AudioClip gibSound;
        [SerializeField] private float stepRate = 0.5f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem bloodParticles;
        [SerializeField] private ParticleSystem gibParticles;
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private GameObject icon;
        [SerializeField] private int gibAmount = 5;
        [SerializeField] private float gibSize = 1f;

        [Header("Enemy Type")]
        [SerializeField] private bool IsRanged;
        [SerializeField] private bool IsBoss;


        [SerializeField] private Image healthBar;

        public bool PlayerInSight { get; private set; }
        public bool PlayerInAttackRange { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsRagdoll { get; private set; }
        public bool IsGibbed { get; private set; }
        public bool CanAttack { get; private set; } = true;

        public bool WalkPointSet { get; private set; }

        public bool CanWalk { get; private set; } = true;

        private new Rigidbody rigidbody;
        private GameObject player;
        private NavMeshAgent navMeshAgent;
        private AudioSource audioSource;
        private float stepTimer;
        private PlayerHealth playerHealth;
        private AnimatorHandler animationHandler;
        private SpawnGibs spawnGibs;
        private Vector3 walkPoint;
        private GameState gameState;

        private float currentHealth;
        private int projectilesShot;

        private void Awake()
        {
            InitializeComponents();
            currentHealth = maxHealth;
            if (!IsBoss)
                gameState.RatSpawned();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var position = transform.position;

            PlayerInSight = Physics.CheckSphere(position, viewRadius, playerMask);
            PlayerInAttackRange = Physics.CheckSphere(position, attackRange, playerMask);

            HandleOutOfBounds(position);

            if (IsRagdoll || IsDead) return;

            if (!PlayerInSight && !PlayerInAttackRange && canPatrol) Patrolling(position);
            if (PlayerInSight && !PlayerInAttackRange && canMove) PursuePlayer();
            if (PlayerInSight && PlayerInAttackRange && !IsRanged) AttackPlayer();
            if (PlayerInSight && PlayerInAttackRange && IsRanged)
            {
                FaceTarget();
                AttackPlayerRanged();
            }


            HandleFootsteps(deltaTime);
            UpdateAnimatorValues(deltaTime);
        }

        private void FaceTarget()
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation,
                lookRotation, Time.deltaTime * 5f);
        }

        private void Patrolling(Vector3 position)
        {
            if (!WalkPointSet)
            {
                SearchWalkPoint(position);
            }

            if (WalkPointSet && CanWalk)
                navMeshAgent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = position - walkPoint;

            // Walk point reached
            if (distanceToWalkPoint.magnitude < 1f)
            {
                WalkPointSet = false;
                CanWalk = false;
                StartCoroutine(WaitForWalkPoint());
            }
        }

        private IEnumerator WaitForWalkPoint()
        {
            yield return new WaitForSeconds(waitTime);
            CanWalk = true;
        }

        private void SearchWalkPoint(Vector3 position)
        {
            //Calculate random point in range
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(position.x + randomX, position.y,
                position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
                WalkPointSet = true;
        }

        private void UpdateAnimatorValues(float delta)
        {
            float speed = navMeshAgent.velocity.magnitude;
            animationHandler.UpdateAnimatorValues(speed, 0, false, false, delta);
        }

        public void TakeHit(float damage)
        {
            currentHealth -= damage;
            bloodParticles.Play();
            audioSource.PlayOneShot(deathSound);

            if (healthBar != null)
                healthBar.fillAmount = currentHealth / maxHealth;

            if (currentHealth <= gibAfter)
            {
                Gib();
                Die();
                return;
            }

            if (currentHealth <= 0)
            {
                EnableRagdoll();
                Die();
            }
        }

        private void Gib()
        {
            if (IsGibbed) return;
            IsGibbed = true;
            HideMesh();
            gibParticles.Play();
            GetComponent<Collider>().enabled = false;
            spawnGibs.SpawnMultipleGibs(transform.position, gibAmount, gibSize);
            audioSource.PlayOneShot(gibSound);
            Destroy(gameObject, 5f);
        }

        private void HideMesh()
        {
            mesh.enabled = false;
        }

        private void HandleFootsteps(float delta)
        {
            if (IsMoving())
            {
                stepTimer += delta;

                if (stepTimer >= stepRate)
                {
                    audioSource.PlayOneShot(footstepSound);
                    stepTimer = 0;
                }
            }
        }

        private void AttackPlayer()
        {
            if (CanAttack && PlayerIsAlive())
            {
                audioSource.PlayOneShot(attackSound);
                animationHandler.PlayTargetAnimation("Bite", true);
                playerHealth.TakeDamage(attackDamage);
                CanAttack = false;
                StartCoroutine(AttackCooldownTimer());
            }
        }

        private void AttackPlayerRanged()
        {
            if (CanAttack && PlayerIsAlive())
            {
                audioSource.PlayOneShot(attackSound);
                animationHandler.PlayTargetAnimation("Bite", true);
                Shoot(player.transform.position);
                CanAttack = false;
                projectilesShot++;
                StartCoroutine(AttackCooldownTimer());
            }
        }

        private IEnumerator AttackCooldownTimer()
        {
            yield return new WaitForSeconds(attackCooldown);
            CanAttack = true;
        }

        private bool IsMoving()
        {
            return navMeshAgent.velocity.magnitude > 0;
        }

        private void InitializeComponents()
        {
            rigidbody = GetComponent<Rigidbody>();
            player = GameObject.FindGameObjectWithTag("Player");
            playerHealth = player.GetComponent<PlayerHealth>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            audioSource = GetComponent<AudioSource>();
            animationHandler = GetComponent<AnimatorHandler>();
            spawnGibs = GetComponent<SpawnGibs>();
            gameState = FindObjectOfType<GameState>();
        }

        private void HandleOutOfBounds(Vector3 position)
        {
            if (position.y < outOfBoundsY)
            {
                Destroy(gameObject);
            }
        }

        private void PursuePlayer()
        {
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(player.transform.position);
            }
            else if (navMeshAgent.hasPath)
            {
                navMeshAgent.ResetPath();
            }
        }

        private bool PlayerIsAlive()
        {
            return playerHealth.CurrentHealth > 0;
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            icon.SetActive(false);

            if (!IsBoss)
                gameState.RatKilled();
            else
                gameState.EndGame();
        }

        private void EnableRagdoll()
        {
            IsRagdoll = true;
            navMeshAgent.enabled = false;
            rigidbody.isKinematic = false;
        }

        private void Shoot(Vector3 playerPosition)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                GameObject prefab = bulletPrefab;

                if (bulletPrefab2 != null && projectilesShot % 3 == 0)
                {
                    prefab = bulletPrefab2;
                }

                GameObject bullet = Instantiate(prefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 shootingDirection = CalculateShootingDirection(playerPosition);
                    rb.velocity = shootingDirection * bulletSpeed;
                }

                if (projectilesShot % 7 == 0 && bulletPrefab3 != null)
                {
                    FireMeteor(bulletPrefab3, firePointMeteor1);
                    FireMeteor(bulletPrefab3, firePointMeteor2);
                    FireMeteor(bulletPrefab3, firePointMeteor3);
                }

                projectilesShot++;
            }
        }

        private void FireMeteor(GameObject prefab, Transform firePointMeteor)
        {
            GameObject meteor = Instantiate(prefab, firePointMeteor.position, firePointMeteor.rotation);
            Rigidbody rbMeteor = meteor.GetComponent<Rigidbody>();
            if (rbMeteor != null)
            {
                rbMeteor.velocity = Vector3.down * bulletSpeed;
            }
        }

        private Vector3 CalculateShootingDirection(Vector3 playerPosition)
        {
            Vector3 directionToPlayer = playerPosition - firePoint.position;
            directionToPlayer.y += 1.0f;
            return directionToPlayer.normalized;
        }

        private void OnDrawGizmosSelected()
        {
            var position = transform.position;

            Gizmos.color = PlayerInSight ? Color.green : Color.red;
            Gizmos.DrawWireSphere(position, viewRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(position, attackRange);
        }
    }
}