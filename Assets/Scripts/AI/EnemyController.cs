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
    [SerializeField] private AudioClip gibSound;
    [SerializeField] private float stepRate = 0.5f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int gibAfter = 3;

    [SerializeField] private ParticleSystem bloodParticles;
    [SerializeField] private ParticleSystem gibParticles;

    [SerializeField] private MeshRenderer enemyMesh;

    public bool IsDead { get; private set; } = false;

    public bool IsGrounded { get; private set; }
    public bool IsPlayerInSight { get; private set; }
    public bool IsRagdoll { get; private set; } = false;
    public bool IsGibbed { get; private set; } = false;
    public bool canAttack { get; private set; } = true;

    private Rigidbody _rigidbody;
    private GameObject _player;
    private NavMeshAgent _navMeshAgent;
    private AudioSource _audioSource;
    private float stepTimer;
    private PlayerHealth _playerHealth;
    private AnimatorHandler _animationHandler;
    private SpawnGibs _spawnGibs;
    private int hitCount = 0;

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
        _animationHandler.UpdateAnimatorValues(speed, 0, false, false, Time.deltaTime);
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
        _spawnGibs.SpawnMultipleGibs(transform.position, 5);
        _audioSource.PlayOneShot(gibSound);

        StartCoroutine(destroyAfterTimer(2f));
    }

    private void HideMesh()
    {
        enemyMesh.enabled = false;
    }

    private IEnumerator destroyAfterTimer(float timer)
    {
        yield return new WaitForSeconds(5.1f);
        Destroy(gameObject);
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
        if (isInAttackRange() && canAttack && playerIsAlive() && !IsDead)
        {
            _audioSource.PlayOneShot(attackSound);
            _animationHandler.PlayTargetAnimation("Bite", true);
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
        _animationHandler = GetComponent<AnimatorHandler>();
        _spawnGibs = GetComponent<SpawnGibs>();
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
        if (!playerIsAlive() || IsDead) return;
        IsPlayerInSight = Physics.CheckSphere(transform.position, viewRadius, playerMask);
    }

    private bool playerIsAlive()
    {
        return _playerHealth.CurrentHealth > 0;
    }

    private void Die()
    {
        if (IsDead) return;
        _audioSource.PlayOneShot(deathSound);
        IsDead = true;
    }

    public void EnableRagdoll()
    {
        IsRagdoll = true;
        _navMeshAgent.enabled = false;
        _rigidbody.isKinematic = false;
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
