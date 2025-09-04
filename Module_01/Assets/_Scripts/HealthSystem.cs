    using UnityEngine;
    using UnityEngine.UI; // Required for UI elements

    public class HealthSystem : MonoBehaviour
    {
        public int maxHealth = 100;
        public int currentHealth;
        public Slider healthBarSlider; // Assign your UI Slider here

        void Start()
        {
            currentHealth = maxHealth;
            UpdateHealthBar();
        }

        public void TakeDamage(int damageAmount)
        {
            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(0, currentHealth); // Ensure health doesn't go below zero
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                Die();
            }
            // Optional: Play a "hurt" animation or sound
        }

	void UpdateHealthBar()
        {
            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = maxHealth;
                healthBarSlider.value = currentHealth;
            }
        }

        void Die()
        {
            // Handle death logic (e.g., destroy GameObject, play death animation, respawn)
		Debug.Log(gameObject.name + " has died!");           
		Destroy(gameObject);
        }
    }