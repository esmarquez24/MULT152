using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("Prefab & Points")]
    [Tooltip("Enemy prefab that already includes HealthComponent, colliders, AI, etc.")]
    public GameObject enemyPrefab;

    [Tooltip("If empty, spawns at this spawner's transform.")]
    public Transform[] spawnPoints;

    [Header("Waves")]
    [Min(1)] public int initialSpawnAmount = 5;   // how many to spawn in wave 1
    [Min(0)] public int addPerWave = 2;           // how much to add each wave
    [Min(1)] public int maxAlive = 10;            // cap simultaneous enemies
    [Min(0f)] public float spawnInterval = 0.35f; // delay between individual spawns
    [Min(0f)] public float timeBetweenWaves = 2f; // delay after a wave is cleared

    [Header("Run Mode")]
    public bool autoStart = true;
    public bool infiniteWaves = true;
    [Min(1)] public int totalWaves = 5;           // only used if infiniteWaves == false

    // Runtime
    int currentWaveIndex = 0; // 0-based, wave # = currentWaveIndex + 1
    int aliveCount = 0;
    int spawnedThisWave = 0;
    int targetThisWave = 0;
    
     [Header("Spawn")]
    public bool spawning = false;

    Coroutine waveRoutine;

    // Keep handlers to unsubscribe safely
    readonly Dictionary<HealthComponent, System.Action> deathHandlers = new();

    void Start()
    {
        if (autoStart) StartWaves();
    }

    [ContextMenu("Start Waves")]
    public void StartWaves()
    {
        if (!enemyPrefab)
        {
            Debug.LogError("[EnemyWaveSpawner] Missing enemyPrefab.", this);
            return;
        }
        if (waveRoutine == null)
            waveRoutine = StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        currentWaveIndex = 0;

        while (true)
        {
            // Stop if we're in finite mode and already ran required waves
            if (!infiniteWaves && currentWaveIndex >= totalWaves)
                break;

            // Set target size for this wave
            targetThisWave = initialSpawnAmount + addPerWave * currentWaveIndex;
            spawnedThisWave = 0;

            // Small pre-wave buffer
            if (currentWaveIndex > 0 && timeBetweenWaves > 0f)
                yield return new WaitForSeconds(timeBetweenWaves);

            // Spawn the wave
            spawning = true;
            while (spawnedThisWave < targetThisWave)
            {
                // respect maxAlive cap
                while (aliveCount >= maxAlive) yield return null;

                SpawnOne();
                spawnedThisWave++;

                if (spawnInterval > 0f)
                    yield return new WaitForSeconds(spawnInterval);
            }
            spawning = false;

            // Wait until all spawned enemies in this wave are dead
            while (aliveCount > 0)
                yield return null;

            currentWaveIndex++;
        }

        waveRoutine = null;
        Debug.Log("[EnemyWaveSpawner] All waves complete.");
    }

    void SpawnOne()
    {
        Transform point = ChooseSpawnPoint();
        GameObject go = Instantiate(enemyPrefab, point.position, point.rotation);

        // Wire up death tracking via HealthComponent
        var hc = go.GetComponentInChildren<HealthComponent>();
        if (hc != null)
        {
            aliveCount++;

            // Capture local handler so we can remove it safely
            System.Action handler = null;
            handler = () =>
            {
                aliveCount = Mathf.Max(0, aliveCount - 1);
                // Unsubscribe and drop reference
                if (hc != null) hc.OnDied -= handler;
                deathHandlers.Remove(hc);
            };

            // subscribe
            hc.OnDied += handler;
            deathHandlers[hc] = handler;
        }
        else
        {
            Debug.LogWarning("[EnemyWaveSpawner] Spawned enemy missing HealthComponent; alive tracking will be off.", go);
        }
    }

    Transform ChooseSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || spawnPoints[0] == null)
            return transform;
        int i = Random.Range(0, spawnPoints.Length);
        return spawnPoints[i] ? spawnPoints[i] : transform;
    }

    void OnDisable()
    {
        // Defensive: clean up any remaining subscriptions (e.g., if spawner is disabled during combat)
        foreach (var kvp in deathHandlers)
        {
            if (kvp.Key != null) kvp.Key.OnDied -= kvp.Value;
        }
        deathHandlers.Clear();
    }

    // Optional: quick accessors
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int AliveCount => aliveCount;

    [ContextMenu("Force Next Wave")]
    public void ForceNextWave()
    {
        // nukes alive count so the loop advances
        aliveCount = 0;
    }
}