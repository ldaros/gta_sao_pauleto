using System;
using UnityEngine;

namespace GTASP.UI
{
    public class MainMenu : MonoBehaviour
    {
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void PlayGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("World");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}