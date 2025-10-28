using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int startHealth = 100;
    [SerializeField] private HealthConfig config; //drag an SO asset here

    [Header("UI (optional)")]
    [SerializeField] private UITK_HUD hud;
    [SerializeField] private RestartUI restartUI; // Assign in scene or auto-find

    public int Current { get; private set; }
    public bool IsDead => Current <= 0;

    // Events (publisher)
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action<int> OnDamaged;           // amount
    public event Action<int> OnHealed;            // amount
    public event Action OnDied;

    private void Awake()
    {
        int max = config != null ? config.maxHealth : maxHealth;
        int start = config != null ? config.startHealth : startHealth;

        // Keep serialized fallbacks for demo, but prefer SO if assigned
        maxHealth = max;
        Current = Mathf.Clamp(start, 0, maxHealth);
        
        // Prefer singleton if available
        if (!restartUI) restartUI = RestartUI.Instance;
        
        // Fallback find (Unity 6.2 safe API)
        if (!restartUI) restartUI = UnityEngine.Object.FindAnyObjectByType<RestartUI>(FindObjectsInactive.Include);

        RaiseChanged();
    }

    public void Damage(int amount)
    {
        if (amount <= 0 || IsDead) return;
        Current = Mathf.Max(0, Current - amount);
        OnDamaged?.Invoke(amount);
        RaiseChanged();

        if (IsDead) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || Current <= 0) return;
        Current = Mathf.Min(maxHealth, Current + amount);
        OnHealed?.Invoke(amount);
        RaiseChanged();
    }

    private void Die()
    {
        OnDied?.Invoke();
        Time.timeScale = 0f;            // pause world
        if (restartUI) restartUI.ShowPanel();
        else Debug.LogWarning("[HealthComponent] Died but RestartUI not found.");
    }


    private void RaiseChanged()
    {
        OnHealthChanged?.Invoke(Current, maxHealth);
        
        if (hud != null)
        {
            hud.SetHP(Current, maxHealth);
        }
    }
}