using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private Transform orientation;
    [SerializeField] private AnimatorHandler animatorHandler;
    [SerializeField] private GameObject rootBone;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCooldown = .25f;
    [SerializeField] private float jumpAirMultiplier = .4f;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float stepRate = 0.5f;
    [SerializeField] private float sprintStepRate = 0.3f;
    private float stepTimer = 0;
    private int stepCounter = 0;

    private float _horizontalInput;
    private float _verticalInput;
    private float _airTime = 0f;
    private Vector3 _movementDirection;
    private Rigidbody _rigidBody;
    private AudioSource _audioSource;

    public bool IsDead { get; set; } = false;
    public bool IsGrounded { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsRagdoll { get; private set; } = false;
    public bool CanJump { get; private set; } = true;
    public float MoveAmount { get; private set; }


    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        CheckGroundStatus(delta);
        HandleInput();
        LimitHorizontalVelocity();
        ApplyGroundDrag();
        UpdateAnimator(delta);
        HandleSteps(delta);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void InitializeComponents()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.freezeRotation = true;

        _audioSource = GetComponent<AudioSource>();

        ConfigureRagdoll(false);
    }

    private void CheckGroundStatus(float delta)
    {
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.2f, groundMask);
        if (IsGrounded && _airTime > 0.25f)
        {
            animatorHandler.PlayTargetAnimation("Land", true);
        }
        if (IsGrounded)
        {
            _airTime = 0f;
        }
        else
        {
            _airTime += delta;
        }
    }

    private void HandleInput()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");

        MoveAmount = Mathf.Clamp01(Mathf.Abs(_horizontalInput) + Mathf.Abs(_verticalInput));

        handleSprint();
        HandleJump();
    }

    private void HandleJump()
    {
        if (IsDead) return;

        if (Input.GetKey(jumpKey) && CanJump && IsGrounded)
        {
            CanJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void handleSprint()
    {
        if (IsDead) return;


        if (Input.GetKeyDown(sprintKey) && IsGrounded)
        {
            IsSprinting = true;
        }
        else if (Input.GetKeyUp(sprintKey))
        {
            IsSprinting = false;
        }
    }

    private void ApplyGroundDrag()
    {
        _rigidBody.drag = IsGrounded ? groundDrag : 0;
    }

    private void MovePlayer()
    {
        _movementDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;
        _rigidBody.AddForce(CalculateMovementForce(), ForceMode.Force);
    }

    private void HandleSteps(float delta)
    {
        if (IsMoving() && IsGrounded)
        {
            stepTimer += delta;

            float rate = IsSprinting ? sprintStepRate : stepRate;

            if (stepTimer >= rate)
            {
                PlayFootstepSound();
                stepTimer = 0;
            }
        }
    }

    private bool IsMoving()
    {
        if (IsRagdoll) return false;
        return (Mathf.Abs(_horizontalInput) > 0.1f || Mathf.Abs(_verticalInput) > 0.1f);
    }

    private Vector3 CalculateMovementForce()
    {
        float multiplier = IsGrounded ? 1f : jumpAirMultiplier;
        return _movementDirection.normalized * GetCurrentMaxSpeed() * multiplier * 10f;
    }

    private void LimitHorizontalVelocity()
    {
        // Extract horizontal velocity
        Vector3 horizontalVelocity = GetHorizontalVelocity();

        // Determine the maximum speed
        float maxSpeed = GetCurrentMaxSpeed();

        // Limit the velocity if it exceeds max speed
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 normalizedVelocity = horizontalVelocity.normalized * maxSpeed;
            _rigidBody.velocity = new Vector3(normalizedVelocity.x, _rigidBody.velocity.y, normalizedVelocity.z);
        }
    }

    private Vector3 GetHorizontalVelocity()
    {
        return new Vector3(_rigidBody.velocity.x, 0f, _rigidBody.velocity.z);
    }

    private float GetCurrentMaxSpeed()
    {
        return IsSprinting ? sprintSpeed : movementSpeed;
    }

    private void Jump()
    {
        _rigidBody.velocity = new Vector3(_rigidBody.velocity.x, 0, _rigidBody.velocity.z);
        _rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        animatorHandler.PlayTargetAnimation("Jumping", true);
    }

    private void ResetJump()
    {
        CanJump = true;
    }

    private void UpdateAnimator(float delta)
    {
        animatorHandler.UpdateAnimatorValues(MoveAmount, 0, IsSprinting, delta);
    }

    private void PlayFootstepSound()
    {
        if (footstepSounds.Length > 0)
        {
            if (stepCounter >= footstepSounds.Length)
            {
                stepCounter = 0;
            }

            AudioClip footstep = footstepSounds[stepCounter];

            // Play the selected sound
            _audioSource.PlayOneShot(footstep);

            stepCounter++;
        }
    }
    public void ToggleRagdoll()
    {
        IsRagdoll = !IsRagdoll;
        ConfigureRagdoll(IsRagdoll);
    }

    private void ConfigureRagdoll(bool enable)
    {
        Rigidbody[] rigidBodies = rootBone.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = rootBone.GetComponentsInChildren<Collider>();

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = !enable;
        }
        foreach (Collider col in colliders)
        {
            col.enabled = enable;
        }

        _rigidBody.isKinematic = enable;
        _rigidBody.useGravity = !enable;
        thirdPersonCamera.lockRotation = enable;
        animatorHandler.animator.enabled = !enable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * (playerHeight / 2 + 0.2f));
    }
}
