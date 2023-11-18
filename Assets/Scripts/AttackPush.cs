using System.Collections;
using UnityEngine;

public class AttackPush : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float pushForceUp = 5f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float ragdollDuration = 5f;

    [Header("Keybinds")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    [Header("Sound")]
    [SerializeField] private AudioClip attackSound;

    [SerializeField] private AnimatorHandler animatorHandler;

    private AudioSource _audioSource;
    private Coroutine _attackCooldownCoroutine;

    public bool CanAttack { get; private set; } = true;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && CanAttack)
        {
            PerformAttack();
            StartCooldown();
        }
    }

    private void PerformAttack()
    {
        animatorHandler.PlayTargetAnimation("Kick", true);
        Collider[] colliders = Physics.OverlapBox(_collider.bounds.center,
            _collider.bounds.extents, _collider.transform.rotation, LayerMask.GetMask("Enemy"));

        foreach (Collider hitCollider in colliders)
        {
            Rigidbody enemyRigidbody = hitCollider.GetComponent<Rigidbody>();
            EnemyController enemyController = hitCollider.GetComponent<EnemyController>();


            if (enemyRigidbody != null && enemyController != null)
            {
                enemyController.EnableRagdoll(ragdollDuration);
                ApplyForceToEnemy(enemyRigidbody);
                _audioSource.PlayOneShot(attackSound);
            }
        }
    }

    private void ApplyForceToEnemy(Rigidbody enemyRigidbody)
    {
        Vector3 direction = (enemyRigidbody.transform.position - transform.position).normalized;
        Vector3 upwardForce = Vector3.up * pushForceUp;
        enemyRigidbody.AddForce((direction + upwardForce) * pushForce * 10f, ForceMode.Impulse);
    }

    private void StartCooldown()
    {
        CanAttack = false;
        if (_attackCooldownCoroutine != null)
        {
            StopCoroutine(_attackCooldownCoroutine);
        }
        _attackCooldownCoroutine = StartCoroutine(ResetAttackAfterCooldown());
    }

    private IEnumerator ResetAttackAfterCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        CanAttack = true;
    }
}
