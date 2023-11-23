using Animation;
using UnityEngine;
using Vehicle;

namespace Player
{
    public class PlayerLocomotion : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float movementSpeed = 7f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float groundDrag = 5f;
        [SerializeField] private Transform orientation;
        [SerializeField] private GameObject rootBone;

        [Header("Jump")] [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float jumpCooldown = .25f;
        [SerializeField] private float jumpAirMultiplier = .4f;

        [Header("Ground Check")] [SerializeField]
        private float playerHeight = 2f;

        [SerializeField] private float airTimeToFall = 2f;
        [SerializeField] private LayerMask groundMask;

        [Header("Sounds")] [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip landSound;
        [SerializeField] private float stepRate = 0.5f;
        [SerializeField] private float sprintStepRate = 0.3f;
        
        private Collider playerCollider;
        private Rigidbody rigidBody;
        private PlayerManager playerManager;
        private AudioSource audioSource;
        private AnimatorHandler animatorHandler;
        private PlayerCamera cameraHandler;
        private PlayerInput inputHandler;

        private Vector3 movementDirection;
        private float airTime;
        private float stepTimer;
        private int stepCounter;

        private VehicleController closeVehicle;
        private VehicleController currentVehicle;
        private SkinnedMeshRenderer meshRenderer;

        private void Start()
        {
            InitializeComponents();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            HandleEnterVehicle();
            if (playerManager.isDriving) return;

            CheckGroundStatus(delta);
            ApplyGroundDrag();
            HandleAiming();
            HandleSprint();
            HandleJump();
            UpdateAnimator(delta);
            HandleFootstepSounds(delta);
        }

        private void FixedUpdate()
        {
            if (playerManager.isDriving)
            {
                MoveWithVehicle();
                return;
            }

            MovePlayer();
            LimitHorizontalVelocity();
        }

        private void MoveWithVehicle()
        {
            rigidBody.MovePosition(closeVehicle.transform.position);
            rigidBody.MoveRotation(closeVehicle.transform.rotation);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Vehicle"))
            {
                closeVehicle = other.GetComponent<VehicleController>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Vehicle") && !playerManager.isDriving)
            {
                closeVehicle = null;
            }
        }

        private void HandleEnterVehicle()
        {
            if (inputHandler.EnterVehicle)
            {
                if (playerManager.isDriving)
                {
                    ExitVehicle();
                }
                else if (closeVehicle != null)
                {
                    EnterVehicle();
                }
            }
        }

        private void EnterVehicle()
        {
            currentVehicle = closeVehicle;
            ToggleVisibility(false);
            transform.parent = currentVehicle.transform;
            currentVehicle.SetPlayerInVehicle(true);
            playerManager.isDriving = true;
        }

        private void ExitVehicle()
        {
            playerManager.isDriving = false;
            ToggleVisibility(true);
            currentVehicle.SetPlayerInVehicle(false);
            var playerPosition = transform;
            playerPosition.parent = null;
            var exitPoint = currentVehicle.GetExitPoint();

            if (Physics.Raycast(exitPoint, Vector3.down,
                    out var hit, 1f, groundMask))
            {
                playerPosition.position = exitPoint;
            }
            else
            {
                playerPosition.position = currentVehicle.transform.position;
            }

            transform.rotation = Quaternion.Euler(0, 0, 0);
            currentVehicle = null;
        }

        private void ToggleVisibility(bool visible)
        {
            rigidBody.isKinematic = !visible;
            playerCollider.enabled = visible;
            meshRenderer.enabled = visible;
            animatorHandler.SetAnimatorState(visible);
        }

        private void InitializeComponents()
        {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.freezeRotation = true;

            playerManager = GetComponent<PlayerManager>();
            inputHandler = playerManager.GetPlayerInput();
            audioSource = playerManager.GetAudioSource();
            animatorHandler = playerManager.GetAnimatorHandler();
            cameraHandler = playerManager.GetCameraHandler();

            playerCollider = GetComponent<CapsuleCollider>();
            meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

            ConfigureRagdoll(false);
        }

        private void CheckGroundStatus(float delta)
        {
            playerManager.isGrounded =
                Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.2f, groundMask);

            if (!playerManager.isGrounded)
            {
                airTime += delta;
                if (airTime >= airTimeToFall)
                    animatorHandler.PlayTargetAnimation("Falling", true);
            }
            else
            {
                if (airTime > 0.25f)
                {
                    animatorHandler.PlayTargetAnimation("Land", true);
                    audioSource.PlayOneShot(landSound);
                }

                airTime = 0f;
            }
        }

        private void HandleJump()
        {
            if (playerManager.isDead || !inputHandler.Jump || !playerManager.canJump
                || !playerManager.isGrounded || playerManager.isAiming) return;

            playerManager.canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        private void HandleAiming()
        {
            if (playerManager.isDead) return;

            playerManager.isAiming = inputHandler.Aim;
        }

        private void HandleSprint()
        {
            if (playerManager.isDead) return;

            playerManager.isSprinting = inputHandler.Sprint && playerManager.isGrounded;
        }

        private void ApplyGroundDrag()
        {
            rigidBody.drag = playerManager.isGrounded ? groundDrag : 0;
        }

        private void MovePlayer()
        {
            if (playerManager.isDead || playerManager.isAiming) return;

            movementDirection = orientation.forward * inputHandler.Vertical +
                                orientation.right * inputHandler.Horizontal;
            rigidBody.AddForce(CalculateMovementForce(), ForceMode.Force);
        }

        private void HandleFootstepSounds(float delta)
        {
            if (IsMoving() && playerManager.isGrounded && !playerManager.isAiming)
            {
                stepTimer += delta;
                float rate = playerManager.isSprinting ? sprintStepRate : stepRate;
                if (stepTimer >= rate)
                {
                    PlayFootstepSound();
                    stepTimer = 0;
                }
            }
        }

        private bool IsMoving()
        {
            return !playerManager.isRagdoll && inputHandler.MoveAmount > 0.1f;
        }

        private Vector3 CalculateMovementForce()
        {
            float multiplier = playerManager.isGrounded ? 1f : jumpAirMultiplier;
            return movementDirection.normalized * (GetCurrentMaxSpeed() * multiplier * 10f);
        }

        private void LimitHorizontalVelocity()
        {
            if (playerManager.isDead) return;

            var velocity = rigidBody.velocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float maxSpeed = GetCurrentMaxSpeed();
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                rigidBody.velocity = new Vector3(horizontalVelocity.x, rigidBody.velocity.y, horizontalVelocity.z);
            }
        }

        private float GetCurrentMaxSpeed()
        {
            return playerManager.isSprinting ? sprintSpeed : movementSpeed;
        }

        private void Jump()
        {
            var velocity = rigidBody.velocity;
            velocity = new Vector3(velocity.x, 0, velocity.z);
            rigidBody.velocity = velocity;
            rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            animatorHandler.PlayTargetAnimation("Jumping", true);
            audioSource.PlayOneShot(jumpSound);
        }

        private void ResetJump()
        {
            playerManager.canJump = true;
        }

        private void UpdateAnimator(float delta)
        {
            animatorHandler.UpdateAnimatorValues(inputHandler.MoveAmount, 0,
                playerManager.isSprinting, playerManager.isAiming, delta);
        }

        private void PlayFootstepSound()
        {
            if (footstepSounds.Length > 0)
            {
                AudioClip footstep = footstepSounds[stepCounter % footstepSounds.Length];
                audioSource.PlayOneShot(footstep);
                stepCounter++;
            }
        }

        public void ToggleRagdoll()
        {
            playerManager.isRagdoll = !playerManager.isRagdoll;
            ConfigureRagdoll(playerManager.isRagdoll);
        }

        private void ConfigureRagdoll(bool enable)
        {
            Rigidbody[] rigidBodies = rootBone.GetComponentsInChildren<Rigidbody>();
            Collider[] colliders = rootBone.GetComponentsInChildren<Collider>();

            foreach (Rigidbody rb in rigidBodies)
                rb.isKinematic = !enable;
            foreach (Collider col in colliders)
                col.enabled = enable;

            if (enable)
            {
                Collider col = GetComponent<Collider>();
                col.isTrigger = true;
                rigidBody.isKinematic = true;
            }

            animatorHandler.SetAnimatorState(!enable);
            cameraHandler.SetCameraLock(enable);
            cameraHandler.SetCustomView(rootBone.transform);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.down * (playerHeight / 2 + 0.2f));
        }
    }
}