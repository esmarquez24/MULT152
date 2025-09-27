using System.Collections.Generic;
using UnityEngine;

/// Attach this to your enemy prefab (or add it at spawn time).
/// It auto-registers live runtime spawns so other systems can find them.
public class SpawnedEnemy : MonoBehaviour
{
    public static readonly List<HealthComponent> Live = new List<HealthComponent>();

    private HealthComponent hc;

    private void Awake()
    {
        // Grab the existing HealthComponent on the enemy
        hc = GetComponentInChildren<HealthComponent>();
        if (hc == null)
        {
            Debug.LogWarning($"{name} spawned without HealthComponent; not tracked.");
        }
    }

    private void OnEnable()
    {
        if (hc != null && !Live.Contains(hc))
            Live.Add(hc);
    }

    private void OnDisable()
    {
        if (hc != null)
            Live.Remove(hc);
    }
}