using UnityEngine;

namespace GTASP.Animation
{
    public class ResetInteraction : StateMachineBehaviour
    {
        private static readonly int IsInteracting = Animator.StringToHash("isInteracting");

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool(IsInteracting, false);
        }
    }
}