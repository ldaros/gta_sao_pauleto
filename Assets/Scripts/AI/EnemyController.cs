using System.Collections;
using Animation;
using Environment;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace AI
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float walkPointRange;
        [SerializeField] private float waitTime = 1f;
        [SerializeField] private float outOfBoundsY = -10f;

        [Header("Combat")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float viewRadius = 10f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private int gibAfter = 3;

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
        private int hitCount;
        private Vector3 walkPoint;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var position = transform.position;

            PlayerInSight = Physics.CheckSphere(position, viewRadius, playerMask);
            PlayerInAttackRange = Physics.CheckSphere(position, attackRange, playerMask);

            HandleOutOfBounds(position);

            if (IsRagdoll || IsDead) return;

            if (!PlayerInSight && !PlayerInAttackRange) Patrolling(position);
            if (PlayerInSight && !PlayerInAttackRange) PursuePlayer();
            if (PlayerInSight && PlayerInAttackRange) AttackPlayer();

            HandleFootsteps(deltaTime);
            UpdateAnimatorValues(deltaTime);
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

        public void TakeHit()
        {
            hitCount++;
            if (hitCount >= gibAfter)
            {
                Gib();
            }

            EnableRagdoll();
            bloodParticles.Play();
            Die();
        }

        public void TakeShot()
        {
            Gib();
            Die();
        }

        private void Gib()
        {
            if (IsGibbed) return;
            IsGibbed = true;
            HideMesh();
            gibParticles.Play();
            spawnGibs.SpawnMultipleGibs(transform.position, 5);
            audioSource.PlayOneShot(gibSound);
            StartCoroutine(ObjectUtils.DestroyAfter(gameObject, 5f));
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
            audioSource.PlayOneShot(deathSound);
            IsDead = true;

            icon.SetActive(false);
        }

        private void EnableRagdoll()
        {
            IsRagdoll = true;
            navMeshAgent.enabled = false;
            rigidbody.isKinematic = false;
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