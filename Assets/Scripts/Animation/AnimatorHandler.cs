using UnityEngine;
using UnityEngine.Serialization;

namespace GTASP.Animation
{
    public class AnimatorHandler : MonoBehaviour
    {
        [FormerlySerializedAs("AnimatorRef")] [SerializeField]
        private Animator animatorRef;

        [Header("Settings")] [SerializeField] private float animationTransitionDuration = 0.2f;
        [SerializeField] private float animationDampTime = 0.1f;

        private Animator Animator { get; set; }

        private static readonly int VerticalHash = Animator.StringToHash("Vertical");
        private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
        private static readonly int IsInteractingHash = Animator.StringToHash("isInteracting");
        private static readonly int IsAimingHash = Animator.StringToHash("isAiming");

        private void Awake()
        {
            Animator = animatorRef ? animatorRef : GetComponent<Animator>();
        }

        public void PlayTargetAnimation(string animationStateName, bool isInteracting)
        {
            Animator.SetBool(IsInteractingHash, isInteracting);
            Animator.CrossFade(animationStateName, animationTransitionDuration);
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting,
            bool isAiming,
            float delta)
        {
            float adjustedVerticalMovement = AdjustForSprinting(verticalMovement, horizontalMovement, isSprinting);
            Animator.SetFloat(VerticalHash, adjustedVerticalMovement, animationDampTime, delta);
            Animator.SetFloat(HorizontalHash, horizontalMovement, animationDampTime, delta);
            Animator.SetBool(IsAimingHash, isAiming);
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting,
            bool isWalking,
            bool isAiming,
            float delta)
        {
            float adjustedVerticalMovement = AdjustForSprinting(verticalMovement, horizontalMovement, isSprinting);
            if (isWalking)
            {
                adjustedVerticalMovement = 0.5f;
            }
            Animator.SetFloat(VerticalHash, adjustedVerticalMovement, animationDampTime, delta);
            Animator.SetFloat(HorizontalHash, horizontalMovement, animationDampTime, delta);
            Animator.SetBool(IsAimingHash, isAiming);
        }

        private float AdjustForSprinting(float verticalMovement, float horizontalMovement, bool isSprinting)
        {
            bool isMoving = verticalMovement != 0 || horizontalMovement != 0;
            return isSprinting && isMoving ? 2.0f : verticalMovement;
        }

        public void SetAnimatorState(bool isEnabled) => Animator.enabled = isEnabled;
    }
}