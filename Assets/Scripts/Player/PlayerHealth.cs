using GTASP.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GTASP.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private ParticleSystem bloodParticles;
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI gameOverText;
    
        private PlayerManager playerManager;
        private PlayerLocomotion playerLocomotion;
        private AnimatorHandler animatorHandler;
        private AudioSource audioSource;

        public float CurrentHealth { get; private set; }

        private void Start()
        {
            InitializeComponents();
            CurrentHealth = maxHealth;
        }

        private void Update()
        {
            UpdateHealthDisplay();
        }

        private void InitializeComponents()
        {
            playerManager = GetComponent<PlayerManager>();
            playerLocomotion = playerManager.GetPlayerLocomotion();
            audioSource = playerManager.GetAudioSource();
            animatorHandler = playerManager.GetAnimatorHandler();
        }

        private void UpdateHealthDisplay()
        {
            healthBar.fillAmount = CurrentHealth / maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (playerManager.isDead) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(CurrentHealth, 0);

            HandleDamageEffects();

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void HandleDamageEffects()
        {
            PlaySound(damageSound);
            animatorHandler.PlayTargetAnimation("Hit", true);
            bloodParticles.Play();
        }

        private void Die()
        {
            playerManager.isDead = true;
            playerLocomotion.ToggleRagdoll();
            PlaySound(deathSound);
            Debug.Log("Player died");
            gameOverText.gameObject.SetActive(true);
            Invoke(nameof(ReturnToMainMenu), 5f);
        }

        public void Respawn()
        {
            playerManager.isDead = false;
            CurrentHealth = maxHealth;
            playerLocomotion.ToggleRagdoll();
        }

        public void Heal(float amount)
        {
            if (playerManager.isDead) return;

            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void ReturnToMainMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
