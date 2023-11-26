using GTASP.Player;
using UnityEngine;

namespace GTASP.AI
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
                if (playerHealth == null) return;
                
                playerHealth.TakeDamage(damage);
                used = true;
            }
            else if (other.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<EnemyController>();
                if (enemy == null) return;
                
                enemy.TakeHit(10f);
                used = true;
            }
        }
    }
}