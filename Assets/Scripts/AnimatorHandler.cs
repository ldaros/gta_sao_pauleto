using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    [SerializeField] private Animator AnimatorRef;

    [Header("Settings")]
    [SerializeField] private float animationTransitionDuration = 0.2f;
    [SerializeField] private float animationDampTime = 0.1f;

    public bool CanRotate { get; private set; }
    public Animator Animator { get; private set; }

    private static readonly int VerticalHash = Animator.StringToHash("Vertical");
    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int IsInteractingHash = Animator.StringToHash("isInteracting");
    private static readonly int IsAimingHash = Animator.StringToHash("isAiming");

    private void Awake()
    {
        Animator = AnimatorRef ? AnimatorRef : GetComponent<Animator>();
    }

    public void PlayTargetAnimation(string animation, bool isInteracting)
    {
        Animator.SetBool(IsInteractingHash, isInteracting);
        Animator.CrossFade(animation, animationTransitionDuration);
    }

    public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting, bool isAiming, float delta)
    {
        float adjustedVerticalMovement = AdjustForSprinting(verticalMovement, horizontalMovement, isSprinting);
        Animator.SetFloat(VerticalHash, adjustedVerticalMovement, animationDampTime, delta);
        Animator.SetFloat(HorizontalHash, horizontalMovement, animationDampTime, delta);
        Animator.SetBool(IsAimingHash, isAiming);
    }

    private float AdjustForSprinting(float verticalMovement, float horizontalMovement, bool isSprinting)
    {
        bool isMoving = verticalMovement != 0 || horizontalMovement != 0;
        return isSprinting && isMoving ? 2.0f : verticalMovement;
    }

    public void EnableRotation() => CanRotate = true;

    public void DisableRotation() => CanRotate = false;

    public void SetAnimatorState(bool isEnabled) => Animator.enabled = isEnabled;
}
