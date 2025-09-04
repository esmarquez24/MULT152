    using UnityEngine;

    public class PlayerAttack : MonoBehaviour
    {
        public int attackDamage = 10;

        void OnTriggerEnter2D(Collider2D other) // Or OnCollisionEnter2D, depending on your setup
        {
            HealthSystem enemyHealth = other.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }
