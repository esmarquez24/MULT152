using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Spawn Points")]
    [Tooltip("Enemy prefab that includes HealthComponent, visuals, colliders, etc.")]
    public GameObject enemyPrefab;

    [Tooltip("If empty, spawns at this spawner's transform.")]
    public Transform[] spawnPoints;

    [Header("Wave Settings")]
    [Min(1)] public int spawnAmount = 5;       // how many per wave (Inspector-exposed, as requested) 
    [Min(0f)] public float spawnInterval = 0.5f;
    [Tooltip("Max number of enemies alive at once. New spawns wait until below this.")]
    [Min(1)] public int maxAlive = 10;

    [Header("Auto Start")]
    public bool spawnOnStart = true;

    // Runtime tracking 
    private int aliveCount = 0;
    private Coroutine spawnRoutine;

    void Start()
    {
        if (spawnOnStart) StartWave();
    }

    [ContextMenu("Start Wave")]

    public void StartWave()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Missing enemyPrefab.", this);
            return;
        }

        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            // Respect maxAlive 
            while (aliveCount >= maxAlive)
                yield return null;



            SpawnOne();
            if (spawnInterval > 0f)
                yield return new WaitForSeconds(spawnInterval);
        }

        spawnRoutine = null;

    }

    private void SpawnOne()
    {
        Transform point = ChooseSpawnPoint();
        GameObject go = Instantiate(enemyPrefab, point.position, point.rotation);

        // Wire health death event so we keep accurate alive counts 
        var hc = go.GetComponentInChildren<HealthComponent>();

        if (hc != null)
        {
            aliveCount++;

            void OnDiedHandler()
            {
                aliveCount = Mathf.Max(0, aliveCount - 1);
                hc.OnDied -= OnDiedHandler; // clean up 
            }
            hc.OnDied += OnDiedHandler;
        }

        else
        {
            Debug.LogWarning("[EnemySpawner] Spawned enemy has no HealthComponent; alive count won't track.", go);
        }
    }

    private Transform ChooseSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return transform;
        int idx = Random.Range(0, spawnPoints.Length);
        return spawnPoints[idx] ? spawnPoints[idx] : transform;
    }

    // Optional: expose alive count 
    public int AliveCount => aliveCount;
}