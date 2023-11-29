using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTASP
{
    public class CreditsExit : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}