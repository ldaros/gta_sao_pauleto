using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float outOfBoundsY = -10f;

    [Header("Ground Check")]
    [SerializeField] private float enemyHeight = 2f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask playerMask;

    [Header("Sound")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float stepRate = 0.5f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    [SerializeField] private ParticleSystem bloodParticles;

    public bool IsDead { get; private set; } = false;

    public bool IsGrounded { get; private set; }
    public bool IsPlayerInSight { get; private set; }
    public bool IsRagdoll { get; private set; } = false;
    public bool canAttack { get; private set; } = true;

    private Rigidbody _rigidbody;
    private GameObject _player;
    private NavMeshAgent _navMeshAgent;
    private AudioSource _audioSource;
    private float stepTimer;
    private PlayerHealth _playerHealth;
    private RatAnimationHandler _animationHandler;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        HandleOutOfBounds();
        PerformGroundCheck();
        seekPlayer();
        UpdateAnimatorValues();
        handleFootsteps();
    }

    private void FixedUpdate()
    {
        PursuePlayer();
        attackPlayer();
    }

    private void UpdateAnimatorValues()
    {
        float speed = _navMeshAgent.velocity.magnitude;
        _animationHandler.UpdateAnimatorValues(speed, Time.deltaTime);
    }

    private void handleFootsteps()
    {
        if (IsMoving())
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepRate)
            {
                _audioSource.PlayOneShot(footstepSound);
                stepTimer = 0;
            }
        }
    }

    private void attackPlayer()
    {
        if (isInAttackRange() && canAttack && _playerHealth.isAlive() && !IsDead)
        {
            _audioSource.PlayOneShot(attackSound);
            _animationHandler.PlayBite();
            _playerHealth.TakeDamage(attackDamage);
            canAttack = false;
            StartCoroutine(attackCooldownTimer());
        }
    }

    private bool isInAttackRange()
    {
        return Physics.CheckSphere(transform.position, attackRange, playerMask);
    }

    private IEnumerator attackCooldownTimer()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private bool IsMoving()
    {
        return _navMeshAgent.velocity.magnitude > 0;
    }

    private void InitializeComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerHealth = _player.GetComponent<PlayerHealth>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _audioSource = GetComponent<AudioSource>();
        _animationHandler = GetComponent<RatAnimationHandler>();
    }

    private void PerformGroundCheck()
    {
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, enemyHeight / 2 + 0.2f, groundMask);
    }

    private void HandleOutOfBounds()
    {
        if (transform.position.y < outOfBoundsY)
        {
            Destroy(gameObject);
        }
    }
    private void PursuePlayer()
    {
        if (IsRagdoll || IsDead) return;

        if (IsPlayerInSight && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.SetDestination(_player.transform.position);
        }
        else
        {
            _navMeshAgent.ResetPath();
        }
    }

    private void seekPlayer()
    {
        if (!_playerHealth.isAlive() || IsDead) return;
        IsPlayerInSight = Physics.CheckSphere(transform.position, viewRadius, playerMask);
    }

    private void Die()
    {
        if (!IsDead)
        {
            _audioSource.PlayOneShot(deathSound);
        }
        bloodParticles.Play();
        IsDead = true;
    }

    public void EnableRagdoll(float duration)
    {
        // Disable NavMeshAgent and enable Rigidbody physics
        IsRagdoll = true;
        _navMeshAgent.enabled = false;
        _rigidbody.isKinematic = false;

        Die();

        //StartCoroutine(ReenableNavMeshAgentAfterDuration(duration));
    }

    private IEnumerator ReenableNavMeshAgentAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        while (!IsGrounded)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Re-enable NavMeshAgent and make Rigidbody kinematic
        IsRagdoll = false;
        _navMeshAgent.enabled = true;
        _rigidbody.isKinematic = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsPlayerInSight ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * (enemyHeight / 2 + 0.2f));

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
