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

    public bool IsGrounded { get; private set; }
    public bool IsPlayerInSight { get; private set; }
    public bool IsRagdoll { get; private set; } = false;

    private Rigidbody _rigidbody;
    private GameObject _player;
    private NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        HandleOutOfBounds();
        PerformGroundCheck();
        seekPlayer();
    }

    private void FixedUpdate()
    {
        PursuePlayer();
    }

    private void InitializeComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _player = GameObject.FindGameObjectWithTag("Player");
        _navMeshAgent = GetComponent<NavMeshAgent>();
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
        if (IsRagdoll) return;

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
        IsPlayerInSight = Physics.CheckSphere(transform.position, viewRadius, playerMask);
    }

    public void EnableRagdoll(float duration)
    {
        // Disable NavMeshAgent and enable Rigidbody physics
        IsRagdoll = true;
        _navMeshAgent.enabled = false;
        _rigidbody.isKinematic = false;

        StartCoroutine(ReenableNavMeshAgentAfterDuration(duration));
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
    }
}
