using UnityEngine;


public class RatAnimationHandler : MonoBehaviour
{
    public Animator animator;

    // Private variables
    private int _speedHash;

    private void Awake()
    {
        _speedHash = Animator.StringToHash("Speed");
    }

    public void UpdateAnimatorValues(float speed, float delta)
    {
        animator.SetFloat(_speedHash, speed, 0.1f, delta);
    }

    public void PlayBite()
    {
        animator.CrossFade("Bite", 0.2f);
    }
}
