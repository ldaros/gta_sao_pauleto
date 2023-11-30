using System;
using System.Collections;
using GTASP.Animation;
using TMPro;
using UnityEngine;

namespace GTASP.Game
{
    public class GameState : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ObjectiveText;
        [SerializeField] private TextMeshProUGUI GameOverText;
        [SerializeField] private GameObject Boss;

        [SerializeField] private GameObject bossHealthBar;
        [SerializeField] private AudioClip bossMusicClip;
        [SerializeField] private AudioSource bossMusic;
        [SerializeField] private AudioClip missionPassedClip;

        [SerializeField] private AnimatorHandler playerAnimatorHandler;

        [SerializeField] private Transform siloPosition;
        [SerializeField] private Transform playerPosition;
        [SerializeField] private float bossSpawnDistance = 50f;
        [SerializeField] private ParticleSystem bossSpawnParticles;

        public delegate void RatKilledEventHandler();

        public delegate void RatSpawnedEventHandler();

        public event RatKilledEventHandler OnRatKilled;
        public event RatSpawnedEventHandler OnRatSpawned;

        public bool hasRifle;
        public bool hasCarBattery;

        private int ratsKilled;
        private int ratsSpawned;
        private bool bossSpawned;
        private bool canSpawnBoss;


        public void RatKilled()
        {
            ratsKilled++;
            OnRatKilled?.Invoke();

            if (ratsKilled == ratsSpawned)
            {
                canSpawnBoss = true;
                bossSpawnParticles.Play();
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

            if (canSpawnBoss && !bossSpawned && PlayerIsNearSilo())
            {
                SpawnBoss();
            }
        }

        private bool PlayerIsNearSilo()
        {
            return Vector3.Distance(playerPosition.position, siloPosition.position) < bossSpawnDistance;
        }

        private void SpawnBoss()
        {
            Boss.SetActive(true);
            bossHealthBar.SetActive(true);

            bossMusic.clip = bossMusicClip;
            bossMusic.volume = 0.5f;
            bossMusic.Play();

            bossSpawned = true;
        }

        private string GetObjectiveText()
        {
            return "" +
                   (!canSpawnBoss ? $"• Exterminar ratos. ({ratsKilled}/{ratsSpawned})" : "") +
                   (canSpawnBoss && !bossSpawned ? "\n• Investigue o silo." : "") +
                   (bossSpawned ? "\n• Derrotar o chefe." : "") +
                   (!hasRifle ? "\n• Encontrar a espingarda." : "") +
                   (!hasCarBattery ? "\n• Reparar o carro." : "");
        }


        public void EndGame()
        {
            bossMusic.Stop();
            bossMusic.clip = missionPassedClip;
            bossMusic.volume = 1f;
            bossMusic.loop = false;
            bossMusic.Play();
            playerAnimatorHandler.PlayTargetAnimation("Dance", true);
            StartCoroutine(EndGameCoroutine());
        }

        private IEnumerator EndGameCoroutine()
        {
            yield return new WaitForSeconds(3f);

            GameOverText.gameObject.SetActive(true);
            GameOverText.text = "Você venceu!";
            GameOverText.color = Color.green;

            yield return new WaitForSeconds(20f);

            UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(siloPosition.position, bossSpawnDistance);
        }
    }
}