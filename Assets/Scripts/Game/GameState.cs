using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GTASP.Game
{
    public class GameState : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ObjectiveText;
        [SerializeField] private TextMeshProUGUI GameOverText;
        [SerializeField] private GameObject Boss;

        public delegate void RatKilledEventHandler();

        public delegate void RatSpawnedEventHandler();

        public event RatKilledEventHandler OnRatKilled;
        public event RatSpawnedEventHandler OnRatSpawned;

        private int ratsKilled;
        private int ratsSpawned;
        private bool bossSpawned;

        public void RatKilled()
        {
            ratsKilled++;
            OnRatKilled?.Invoke();

            if (ratsKilled == ratsSpawned)
            {
                Boss.SetActive(true);
                bossSpawned = true;
            }
        }

        public void RatSpawned()
        {
            ratsSpawned++;
            OnRatSpawned?.Invoke();
        }

        private void Update()
        {
            ObjectiveText.text = GetObjectiveText();
        }

        private string GetObjectiveText()
        {
            return $"• Exterminar ratos. ({ratsKilled}/{ratsSpawned})" +
                   (bossSpawned ? "\n• Derrotar o chefe." : "");
        }
        
        
        public void EndGame()
        {
            StartCoroutine(EndGameCoroutine());
        }
        
        private IEnumerator EndGameCoroutine()
        {   
            // wait 3 seconds
            yield return new WaitForSeconds(3f);
            
            // show game over text
            GameOverText.gameObject.SetActive(true);
            GameOverText.text = "Você venceu!";
            GameOverText.color = Color.green;
            
            // wait 30 seconds
            yield return new WaitForSeconds(30f);
            
            // switch to credits scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
        }
    }
}