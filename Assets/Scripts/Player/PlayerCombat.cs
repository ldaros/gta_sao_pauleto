using System;
using System.Collections;
using AI;
using Animation;
using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField]
        private BoxCollider attackCollider;

        [SerializeField] private float pushForce = 10f;
        [SerializeField] private float pushForceUp = 5f;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private float shootingCooldown = 1f;

        [Header("Sound")][SerializeField] private AudioClip[] attackSounds;
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip shootingSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip ricochetSound;

        [Header("Layers")][SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask objectLayer;

        [SerializeField] private GameObject weaponObject;
        [SerializeField] private GameObject bulletHolePrefab;
        [SerializeField] private GameObject bulletParticles;
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

            if (inputHandler.Attack && playerManager.canAttack)
            {
                PerformAttack();
                StartCooldown(ref attackCooldownCoroutine, cooldown, () => playerManager.canAttack = true);
            }

            if (inputHandler.Shoot && playerManager.canShoot
                                   && playerManager.isAiming && playerManager.isGrounded)
            {
                Shoot();
                StartCooldown(ref shootCooldownCoroutine, shootingCooldown, () =>
                {
                    playerManager.canShoot = true;
                    audioSource.PlayOneShot(reloadSound);
                });
            }
        }

        private void HandleWeaponVisibility()
        {
            weaponObject.SetActive(playerManager.isAiming);
        }

        private void Shoot()
        {
            Ray ray = cameraHandler.GetMainCamera().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            animatorHandler.PlayTargetAnimation("Firing", true);
            audioSource.PlayOneShot(shootingSound);
            playerManager.canShoot = false;

            if (Physics.Raycast(ray, out var hit, 100f, enemyLayer | objectLayer | groundLayer))
            {
                ProcessRaycastHit(hit);
            }
        }

        private void ProcessRaycastHit(RaycastHit hit)
        {
            if ((enemyLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                Debug.Log("Tomei bala - rato normal");
                if (enemy != null)
                    enemy.TakeShot();
                return;
            }
            audioSource.PlayOneShot(ricochetSound);
            if ((objectLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                ApplyForceToRigidbody(hit.collider.GetComponent<Rigidbody>(), pushForce, 0);
                CreateBulletParticles(hit.point, hit.normal);
                return;
            }

            if ((groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                CreateBulletHole(hit.point, hit.normal);
                CreateBulletParticles(hit.point, hit.normal);
            }
        }

        private void CreateBulletHole(Vector3 position, Vector3 normal)
        {
            Quaternion holeRotation = Quaternion.FromToRotation(Vector3.up, normal);
            GameObject bulletHole = Instantiate(bulletHolePrefab, position + (normal * .01f), holeRotation);

            // Start coroutine to despawn the bullet hole after a set duration
            StartCoroutine(ObjectUtils.DestroyAfter(bulletHole, bulletHoleDespawnTime));
        }

        private void CreateBulletParticles(Vector3 position, Vector3 normal)
        {
            Quaternion particleRotation = Quaternion.LookRotation(normal);

            GameObject particles = Instantiate(bulletParticles, position, particleRotation);

            // Start coroutine to despawn the bullet hole after a set duration
            StartCoroutine(ObjectUtils.DestroyAfter(particles, 1f));
        }

        private void PerformAttack()
        {
            animatorHandler.PlayTargetAnimation("Kick", true);
            playerManager.canAttack = false;

            ProcessAttackHits(enemyLayer, TryAttackEnemy);
            ProcessAttackHits(objectLayer, TryAttackObject);
        }

        private void ProcessAttackHits(LayerMask layer, Action<Collider> action)
        {
            var results = new Collider[10];
            var bounds = attackCollider.bounds;
            var size = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, results,
                attackCollider.transform.rotation, layer);

            for (int i = 0; i < size; i++)
                action.Invoke(results[i]);
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
            rb.AddForce((direction + upwardForce) * (force * 10f), ForceMode.Impulse);
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
}