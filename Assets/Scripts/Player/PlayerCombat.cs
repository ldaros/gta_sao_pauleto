using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private BoxCollider attackCollider;
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float pushForceUp = 5f;
    [SerializeField] private float cooldown = 1f;

    [Header("Sound")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] hitSounds;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask objectLayer;

    private Coroutine attackCooldownCoroutine;

    private PlayerManager playerManager;
    private PlayerInput inputHandler;
    private AudioSource audioSource;
    private AnimatorHandler animatorHandler;

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (inputHandler.Attack && playerManager.CanAttack)
        {
            PerformAttack();
            StartAttackCooldown();
        }
    }

    private void InitializeComponents()
    {
        playerManager = GetComponent<PlayerManager>();
        inputHandler = playerManager.GetPlayerInput();
        audioSource = playerManager.GetAudioSource();
        animatorHandler = playerManager.GetAnimatorHandler();
    }

    private void PerformAttack()
    {
        animatorHandler.PlayTargetAnimation("Kick", true);
        ProcessAttackHits(enemyLayer, TryAttackEnemy);
        ProcessAttackHits(objectLayer, TryAttackObject);
    }

    private void ProcessAttackHits(LayerMask layer, System.Action<Collider> action)
    {
        Collider[] hitColliders = Physics.OverlapBox(attackCollider.bounds.center,
            attackCollider.bounds.extents, attackCollider.transform.rotation, layer);

        foreach (Collider hitCollider in hitColliders)
        {
            action.Invoke(hitCollider);
        }
    }

    private void TryAttackEnemy(Collider hitCollider)
    {
        Rigidbody enemyRigidbody = hitCollider.GetComponent<Rigidbody>();
        EnemyController enemyController = hitCollider.GetComponent<EnemyController>();

        if (enemyRigidbody && enemyController)
        {
            enemyController.TakeHit();
            ApplyForceToRigidbody(enemyRigidbody);
            PlayRandomSound(attackSounds);
        }
    }

    private void TryAttackObject(Collider hitCollider)
    {
        Rigidbody objectRigidbody = hitCollider.GetComponent<Rigidbody>();

        if (objectRigidbody)
        {
            ApplyForceToRigidbody(objectRigidbody);
            PlayRandomSound(hitSounds);
        }
    }

    private void ApplyForceToRigidbody(Rigidbody rb)
    {
        Vector3 direction = (rb.transform.position - transform.position).normalized;
        Vector3 upwardForce = Vector3.up * pushForceUp;
        rb.AddForce((direction + upwardForce) * pushForce * 10f, ForceMode.Impulse);
    }

    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds.Length > 0)
        {
            AudioClip sound = sounds[Random.Range(0, sounds.Length)];
            audioSource.PlayOneShot(sound);
        }
    }

    private void StartAttackCooldown()
    {
        playerManager.CanAttack = false;
        if (attackCooldownCoroutine != null)
        {
            StopCoroutine(attackCooldownCoroutine);
        }
        attackCooldownCoroutine = StartCoroutine(ResetAttackAfterCooldown());
    }

    private IEnumerator ResetAttackAfterCooldown()
    {
        yield return new WaitForSeconds(cooldown);
        playerManager.CanAttack = true;
    }
}
