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

        [SerializeField] private VehicleController vehicle;

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

        private bool insideVehicle;

        private void Start()
        {
            InitializeComponents();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            HandleEnterVehicle();
            if (insideVehicle) return;

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
            MovePlayer();
            LimitHorizontalVelocity();
        }

        private void HandleEnterVehicle()
        {
            if (inputHandler.EnterVehicle)
            {
                if (insideVehicle)
                {
                    ExitVehicle();
                }
                else
                {
                    EnterVehicle();
                }
            }
        }

        private void EnterVehicle()
        {
            insideVehicle = true;
            ToggleVisibility(false);
            FollowVehicle();
            vehicle.SetPlayerInVehicle(true);
        }

        private void ExitVehicle()
        {
            insideVehicle = false;
            ToggleVisibility(true);
            vehicle.SetPlayerInVehicle(false);
            var playerPosition = transform;
            playerPosition.parent = null;

            var vehicleTransform = vehicle.transform;
            var vehiclePosition = vehicleTransform.position;
            playerPosition.position = new Vector3(vehiclePosition.x, vehiclePosition.y + 1,
                vehiclePosition.z);

            transform.rotation = Quaternion.Euler(0, vehicleTransform.rotation.eulerAngles.y, 0);
        }

        private void ToggleVisibility(bool visible)
        {
            rigidBody.isKinematic = !visible;
            GetComponent<Collider>().enabled = visible;
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = visible;
        }


        private void FollowVehicle()
        {
            var playerTransform = transform;
            playerTransform.parent = vehicle.transform;
            playerTransform.localPosition = new Vector3(0, 0, 0);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
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