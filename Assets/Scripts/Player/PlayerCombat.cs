using System;
using System.Collections;
using GTASP.AI;
using GTASP.Animation;
using UnityEngine;

namespace GTASP.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField]
        private BoxCollider attackCollider;

        [SerializeField] private float pushForce = 10f;
        [SerializeField] private float pushForceVehicle = 1000f;
        [SerializeField] private float pushForceUp = 5f;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private float shootingCooldown = 1f;

        [SerializeField] private float kickDamage = 10f;
        [SerializeField] private float shootDamage = 30f;

        [Header("Sound")] [SerializeField] private AudioClip[] attackSounds;
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip shootingSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip ricochetSound;

        [Header("Layers")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask rangedEnemyLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask objectLayer;
        [SerializeField] private LayerMask vehicleLayer;

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

            if (Physics.Raycast(ray, out var hit, 300f, enemyLayer | objectLayer | groundLayer))
            {
                ProcessRaycastHit(hit);
            }
        }

        private void ProcessRaycastHit(RaycastHit hit)
        {
            if ((enemyLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                if (enemy != null)
                    enemy.TakeHit(shootDamage);
                return;
            }

            if ((rangedEnemyLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.Log("Ranged enemy hit!");
                var enemy = hit.collider.GetComponent<RatoRangedAi>();
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
            Destroy(bulletHole, bulletHoleDespawnTime);
        }

        private void CreateBulletParticles(Vector3 position, Vector3 normal)
        {
            Quaternion particleRotation = Quaternion.LookRotation(normal);
            GameObject particles = Instantiate(bulletParticles, position, particleRotation);
            
            Destroy(particles, bulletHoleDespawnTime);
        }

        private void PerformAttack()
        {
            animatorHandler.PlayTargetAnimation("Kick", true);
            playerManager.canAttack = false;

            ProcessAttackHits(enemyLayer, TryAttackEnemy);
            ProcessAttackHits(objectLayer, TryAttackObject);
            ProcessAttackHits(vehicleLayer, TryAttackVehicle);
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
                enemyController.TakeHit(kickDamage);
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

        private void TryAttackVehicle(Collider hitCollider)
        {
            Rigidbody objectRigidbody = hitCollider.GetComponent<Rigidbody>();

            if (objectRigidbody)
            {
                ApplyForceToRigidbody(objectRigidbody, pushForceVehicle, 0);
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