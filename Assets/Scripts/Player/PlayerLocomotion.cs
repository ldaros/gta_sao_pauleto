using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private Transform orientation;
    [SerializeField] private GameObject rootBone;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCooldown = .25f;
    [SerializeField] private float jumpAirMultiplier = .4f;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float airTimeToFall = 2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float stepRate = 0.5f;
    [SerializeField] private float sprintStepRate = 0.3f;

    // References
    private Rigidbody rigidBody;
    private PlayerManager playerManager;
    private AudioSource audioSource;
    private AnimatorHandler animatorHandler;
    private PlayerCamera cameraHandler;
    private PlayerInput inputHandler;

    private Vector3 movementDirection;
    private float airTime = 0f;
    private float stepTimer = 0;
    private int stepCounter = 0;

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        CheckGroundStatus(delta);
        ApplyGroundDrag();
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
        playerManager.IsGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.2f, groundMask);

        if (!playerManager.IsGrounded)
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
        if (playerManager.IsDead || !inputHandler.Jump || !playerManager.CanJump || !playerManager.IsGrounded) return;

        playerManager.CanJump = false;
        Jump();
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void HandleSprint()
    {
        if (playerManager.IsDead) return;

        playerManager.IsSprinting = inputHandler.Sprint && playerManager.IsGrounded;
    }

    private void ApplyGroundDrag()
    {
        rigidBody.drag = playerManager.IsGrounded ? groundDrag : 0;
    }

    private void MovePlayer()
    {
        movementDirection = orientation.forward * inputHandler.Vertical + orientation.right * inputHandler.Horizontal;
        rigidBody.AddForce(CalculateMovementForce(), ForceMode.Force);
    }

    private void HandleFootstepSounds(float delta)
    {
        if (IsMoving() && playerManager.IsGrounded)
        {
            stepTimer += delta;
            float rate = playerManager.IsSprinting ? sprintStepRate : stepRate;
            if (stepTimer >= rate)
            {
                PlayFootstepSound();
                stepTimer = 0;
            }
        }
    }

    private bool IsMoving()
    {
        return !playerManager.IsRagdoll && inputHandler.MoveAmount > 0.1f;
    }

    private Vector3 CalculateMovementForce()
    {
        float multiplier = playerManager.IsGrounded ? 1f : jumpAirMultiplier;
        return movementDirection.normalized * GetCurrentMaxSpeed() * multiplier * 10f;
    }

    private void LimitHorizontalVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
        float maxSpeed = GetCurrentMaxSpeed();
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rigidBody.velocity = new Vector3(horizontalVelocity.x, rigidBody.velocity.y, horizontalVelocity.z);
        }
    }

    private float GetCurrentMaxSpeed()
    {
        return playerManager.IsSprinting ? sprintSpeed : movementSpeed;
    }

    private void Jump()
    {
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        animatorHandler.PlayTargetAnimation("Jumping", true);
        audioSource.PlayOneShot(jumpSound);
    }

    private void ResetJump()
    {
        playerManager.CanJump = true;
    }

    private void UpdateAnimator(float delta)
    {
        animatorHandler.UpdateAnimatorValues(inputHandler.MoveAmount, 0, playerManager.IsSprinting, delta);
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
        playerManager.IsRagdoll = !playerManager.IsRagdoll;
        ConfigureRagdoll(playerManager.IsRagdoll);
    }

    private void ConfigureRagdoll(bool enable)
    {
        Rigidbody[] rigidBodies = rootBone.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = rootBone.GetComponentsInChildren<Collider>();

        foreach (Rigidbody rb in rigidBodies)
            rb.isKinematic = !enable;
        foreach (Collider col in colliders)
            col.enabled = enable;

        rigidBody.isKinematic = enable;
        rigidBody.useGravity = !enable;

        animatorHandler.SetAnimatorState(!enable);
        cameraHandler.SetCameraLock(enable);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * (playerHeight / 2 + 0.2f));
    }
}
