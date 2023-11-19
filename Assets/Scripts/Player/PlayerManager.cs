using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Flags")]
    public bool isInteracting;
    public bool IsDead;
    public bool IsGrounded;
    public bool IsSprinting;
    public bool IsRagdoll;
    public bool CanJump;
    public bool CanAttack;

    private PlayerInput playerInput;
    private PlayerCamera cameraHandler;
    private PlayerLocomotion playerLocomotion;
    private PlayerCombat playerCombat;
    private PlayerOutOfBounds playerOutOfBounds;
    private PlayerHealth playerHealth;
    private Animator animator;
    private AnimatorHandler animatorHandler;
    private AudioSource audioSource;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        isInteracting = animator.GetBool("isInteracting");
    }

    private void InitializeComponents()
    {
        playerInput = GetComponent<PlayerInput>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        playerHealth = GetComponent<PlayerHealth>();
        playerCombat = GetComponent<PlayerCombat>();
        playerOutOfBounds = GetComponent<PlayerOutOfBounds>();
        cameraHandler = GetComponent<PlayerCamera>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
    }

    public AudioSource GetAudioSource() => audioSource;
    public PlayerHealth GetHealth() => playerHealth;
    public PlayerCamera GetCameraHandler() => cameraHandler;
    public AnimatorHandler GetAnimatorHandler() => animatorHandler;
    public PlayerInput GetPlayerInput() => playerInput;
    public PlayerLocomotion GetPlayerLocomotion() => playerLocomotion;
}
