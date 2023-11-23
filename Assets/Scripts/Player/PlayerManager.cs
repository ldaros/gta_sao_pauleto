using Animation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Flags")] 
        public bool isInteracting;
        public bool isDead;
        public bool isGrounded;
        public bool isSprinting;
        public bool isAiming;
        public bool isRagdoll;
        public bool isDriving;
        public bool canJump;
        public bool canShoot;
        public bool canAttack;

        private PlayerInput playerInput;
        private PlayerCamera cameraHandler;
        private PlayerLocomotion playerLocomotion;
        private PlayerHealth playerHealth;
        private Animator animator;
        private AnimatorHandler animatorHandler;
        private AudioSource audioSource;

        private static readonly int IsInteracting = Animator.StringToHash("isInteracting");

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            isInteracting = animator.GetBool(IsInteracting);
        }

        private void InitializeComponents()
        {
            playerInput = GetComponent<PlayerInput>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            playerHealth = GetComponent<PlayerHealth>();
            GetComponent<PlayerCombat>();
            GetComponent<PlayerOutOfBounds>();
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
}