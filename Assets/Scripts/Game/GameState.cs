using System;
using TMPro;
using UnityEngine;

namespace GTASP.Game
{
    public class GameState : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ObjectiveText;
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
    }
}