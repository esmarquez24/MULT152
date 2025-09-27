using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class EnemyManager : MonoBehaviour
{
    [Header("Input (New Input System)")]
    [SerializeField] private Key key = Key.L; // or wire an InputActionReference if you prefer

    [Header("Damage")]
    [Tooltip("Damage dealt to ensure death. Should be >= max health.")]
    [SerializeField] private int lethalDamage = 9999;

    private int cursor = 0; // which enemy we target next

    void Update()
    {
        // Simple direct key read using the Input System's Keyboard
        if (Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame)
        {
            KillNextSpawnedEnemy();
        }
    }

    private void KillNextSpawnedEnemy()
    {
        // Clean up nulls (destroyed entries) in-place
        for (int i = SpawnedEnemy.Live.Count - 1; i >= 0; i--)
            if (SpawnedEnemy.Live[i] == null) SpawnedEnemy.Live.RemoveAt(i);

        if (SpawnedEnemy.Live.Count == 0)
        {
            Debug.Log("No runtime-spawned enemies left.");
            cursor = 0;
            return;
        }

        if (cursor >= SpawnedEnemy.Live.Count) cursor = 0;

        var hc = SpawnedEnemy.Live[cursor];
        cursor++;

        if (hc == null) return;

        // Use your existing health system so OnDamaged/OnDied fire correctly.
        // HealthComponent.Damage(int) will invoke OnDamaged/OnHealthChanged and OnDied at 0. :contentReference[oaicite:2]{index=2}
        hc.Damage(lethalDamage);
    }
}