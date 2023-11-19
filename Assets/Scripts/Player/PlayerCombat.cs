using System;
using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private BoxCollider attackCollider;
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float pushForceUp = 5f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float shootingCooldown = 1f;

    [Header("Sound")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip shootingSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip ricochetSound;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask objectLayer;

    [SerializeField] private GameObject weaponObject;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private float bulletHoleDespawnTime = 10f;

    private Coroutine attackCooldownCoroutine;
    private Coroutine shootCooldownCoroutine;

    private PlayerManager playerManager;
    private PlayerInput inputHandler;
    private AudioSource audioSource;
    private AnimatorHandler animatorHandler;
    private PlayerCamera cameraHandler;

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        HandleWeaponVisibility();

        if (inputHandler.Attack && playerManager.CanAttack)
        {
            PerformAttack();
            StartCooldown(ref attackCooldownCoroutine, cooldown, () => playerManager.CanAttack = true);
        }

        if (inputHandler.Shoot && playerManager.CanShoot
            && playerManager.IsAiming && playerManager.IsGrounded)
        {
            Shoot();
            StartCooldown(ref shootCooldownCoroutine, shootingCooldown, () =>
            {
                playerManager.CanShoot = true;
                audioSource.PlayOneShot(reloadSound);
            });
        }
    }

    private void HandleWeaponVisibility()
    {
        weaponObject.SetActive(playerManager.IsAiming);
    }

    private void Shoot()
    {
        Ray ray = cameraHandler.GetMainCamera().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        audioSource.PlayOneShot(shootingSound);
        playerManager.CanShoot = false;

        if (Physics.Raycast(ray, out hit, 100f, enemyLayer | objectLayer | groundLayer))
        {
            ProcessRaycastHit(hit);
        }
    }

    private void ProcessRaycastHit(RaycastHit hit)
    {
        if ((enemyLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
        {
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();
            enemy?.TakeShot();
            return;
        }

        audioSource.PlayOneShot(ricochetSound);
        if ((objectLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
        {
            ApplyForceToRigidbody(hit.collider.GetComponent<Rigidbody>(), pushForce, 0);
            return;
        }

        if ((groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
        {
            CreateBulletHole(hit.point, hit.normal);
            return;
        }
    }

    private void CreateBulletHole(Vector3 position, Vector3 normal)
    {
        Quaternion holeRotation = Quaternion.LookRotation(normal);

        holeRotation *= Quaternion.Euler(90, 0, 0);

        GameObject bulletHole = Instantiate(bulletHolePrefab, position, holeRotation);

        // Start coroutine to despawn the bullet hole after a set duration
        StartCoroutine(Despawn(bulletHole));
    }

    private IEnumerator Despawn(GameObject obj)
    {
        yield return new WaitForSeconds(bulletHoleDespawnTime);
        Destroy(obj);
    }

    private void PerformAttack()
    {
        animatorHandler.PlayTargetAnimation("Kick", true);
        playerManager.CanAttack = false;

        ProcessAttackHits(enemyLayer, TryAttackEnemy);
        ProcessAttackHits(objectLayer, TryAttackObject);
    }

    private void ProcessAttackHits(LayerMask layer, System.Action<Collider> action)
    {
        Collider[] hitColliders = Physics.OverlapBox(attackCollider.bounds.center, attackCollider.bounds.extents, attackCollider.transform.rotation, layer);

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
            ApplyForceToRigidbody(enemyRigidbody, pushForce, pushForceUp);
            PlayRandomSound(attackSounds);
        }
    }

    private void TryAttackObject(Collider hitCollider)
    {
        Rigidbody objectRigidbody = hitCollider.GetComponent<Rigidbody>();

        if (objectRigidbody)
        {
            ApplyForceToRigidbody(objectRigidbody, pushForce, 0);
            PlayRandomSound(hitSounds);
        }
    }

    private void ApplyForceToRigidbody(Rigidbody rb, float force, float forceUp)
    {
        Vector3 direction = (rb.transform.position - transform.position).normalized;
        Vector3 upwardForce = Vector3.up * forceUp;
        rb.AddForce((direction + upwardForce) * force * 10f, ForceMode.Impulse);
    }

    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds.Length > 0)
        {
            AudioClip sound = sounds[UnityEngine.Random.Range(0, sounds.Length)];
            audioSource.PlayOneShot(sound);
        }
    }

    private void StartCooldown(ref Coroutine coroutine, float cooldownDuration, Action onComplete)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(CooldownTimer(cooldownDuration, onComplete));
    }

    private IEnumerator CooldownTimer(float cooldownDuration, Action onComplete)
    {
        yield return new WaitForSeconds(cooldownDuration);
        onComplete?.Invoke();
    }

    private void InitializeComponents()
    {
        playerManager = GetComponent<PlayerManager>();
        inputHandler = playerManager.GetPlayerInput();
        audioSource = playerManager.GetAudioSource();
        animatorHandler = playerManager.GetAnimatorHandler();
        cameraHandler = playerManager.GetCameraHandler();
    }
}
