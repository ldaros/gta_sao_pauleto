using UnityEngine;
using Player;
using AI;

namespace AI
{
    public class BulletDamage : MonoBehaviour
    {
        [SerializeField] private float damage = 10f;

        private bool used;

        private void OnCollisionEnter(Collision collision)
        {
            var other = collision.collider;

            if (used) return;

            if (other.CompareTag("Player"))
            {
                var playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    used = true;
                }
            }
            else if (other.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeHit();
                    used = true;
                }
            }
        }
    }
}