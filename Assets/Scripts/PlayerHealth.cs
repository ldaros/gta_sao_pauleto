using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private ParticleSystem bloodParticles;
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    [SerializeField] private AnimatorHandler animatorHandler;

    private float _currentHealth;
    private PlayerMovement _playerMovement;
    private AudioSource _audioSource;

    private void Start()
    {
        _currentHealth = maxHealth;
        _playerMovement = GetComponent<PlayerMovement>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        healthText.text = _currentHealth.ToString();
    }

    public void TakeDamage(float damage)
    {
        if (_playerMovement.IsDead) return;

        _currentHealth -= damage;

        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }

        _audioSource.PlayOneShot(damageSound);
        animatorHandler.PlayTargetAnimation("Hit", true);
        bloodParticles.Play();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _playerMovement.IsDead = true;
        _playerMovement.ToggleRagdoll();
        _audioSource.PlayOneShot(deathSound);
        Debug.Log("Player died");
    }

    public void Respawn()
    {
        _playerMovement.IsDead = false;
        _currentHealth = maxHealth;
        _playerMovement.ToggleRagdoll();
    }

    public void Heal(float amount)
    {
        if (_playerMovement.IsDead)
        {
            return;
        }

        _currentHealth += amount;

        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
    }

    public bool isAlive()
    {
        return _currentHealth > 0;
    }
}
