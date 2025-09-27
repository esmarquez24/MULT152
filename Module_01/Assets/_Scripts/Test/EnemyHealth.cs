using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Slider healthSlider;   // Drag your Slider here in the Inspector
    public bool hideWhenFull = true;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        // optional fade control
        canvasGroup = healthSlider ? healthSlider.GetComponentInParent<CanvasGroup>() : null;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        UpdateHealthBar();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (!healthSlider) return;

        float t = currentHealth / maxHealth;
        healthSlider.value = t;

        if (hideWhenFull && canvasGroup != null)
            canvasGroup.alpha = Mathf.Approximately(t, 1f) ? 0f : 1f;
    }

    void Die()
    {
        // death logic
        Debug.Log($"{gameObject.name} died.");
        Destroy(gameObject); // or trigger animations/effects instead
    }
}