using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.NonSerialized]
    public GameObject enemyPrefab;
    [System.NonSerialized]
    public float spawnInterval = 3f;
    [System.NonSerialized]
    public float arenaRadius = 8f;
    [System.NonSerialized]
    public int maxEnemies = 20;
    
    private float lastSpawnTime;
    
    void Start()
    {
        lastSpawnTime = Time.time;
        
        // Load enemy prefab from Resources folder (non-serialized fields must be loaded in code)
        enemyPrefab = Resources.Load<GameObject>("Enemy");
        
        Debug.Log($"[EnemySpawner] Loaded enemyPrefab: {(enemyPrefab != null ? enemyPrefab.name : "NULL")}");
    }
    
    void Update()
    {
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnEnemy();
            lastSpawnTime = Time.time;
        }
    }
    
    void SpawnEnemy()
    {
        // Check max enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"[EnemySpawner] Found {enemies.Length} enemies (max: {maxEnemies}), enemyPrefab: {(enemyPrefab != null ? enemyPrefab.name : "NULL")}");
        
        if (enemies.Length >= maxEnemies)
        {
            Debug.Log($"[EnemySpawner] Max enemies reached, not spawning");
            return;
        }
        
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] EnemyPrefab is not assigned!");
            return;
        }
        
        // Spawn at random position on edge of arena
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 spawnPos = new Vector3(
            Mathf.Cos(angle) * arenaRadius,
            0.5f,
            Mathf.Sin(angle) * arenaRadius
        );
        
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"[EnemySpawner] Spawned enemy at {spawnPos}");
    }
}