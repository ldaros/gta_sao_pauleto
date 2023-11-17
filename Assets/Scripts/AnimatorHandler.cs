using UnityEngine;


public class AnimatorHandler : MonoBehaviour
{
    public float AnimationTransitionDuration = 0.2f;
    public float AnimationDampTime = 0.1f;

    // Public variables
    public Animator animator;
    public bool canRotate;

    // Private variables
    private int _verticalHash;
    private int _horizontalHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _verticalHash = Animator.StringToHash("Vertical");
        _horizontalHash = Animator.StringToHash("Horizontal");
    }

    public void PlayTargetAnimation(string animation, bool isInteracting)
    {
        animator.SetBool("isInteracting", isInteracting);
        animator.CrossFade(animation, AnimationTransitionDuration);
    }

    public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting, float delta)
    {
        bool isMoving = verticalMovement != 0 || horizontalMovement != 0;

        if (isSprinting && isMoving) { verticalMovement = 2; }

        animator.SetFloat(_verticalHash, verticalMovement, AnimationDampTime, delta);
        animator.SetFloat(_horizontalHash, horizontalMovement, AnimationDampTime, delta);
    }

    public void EnableRotation() { canRotate = true; }

    public void DisableRotation() { canRotate = false; }
}
