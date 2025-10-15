using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.NonSerialized]
    public GameObject enemyPrefab;
    [System.NonSerialized]
    public float arenaRadius = 10f;
    [System.NonSerialized]
    public int maxEnemies = 20;
    
    // Wave configuration
    private WaveData currentWave;
    private float difficultyMultiplier = 1f;
    private float lastSpawnTime;
    private int currentEnemyIndex = 0;
    private List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();
    private int totalEnemiesSpawned = 0;
    private bool waveConfigured = false;
    
    // Reference to LevelManager
    private LevelManager levelManager;
    
    void Start()
    {
        // Load enemy prefab from Resources folder (non-serialized fields must be loaded in code)
        enemyPrefab = Resources.Load<GameObject>("Enemy");
        
        levelManager = GetComponent<LevelManager>();
        
        Debug.Log($"[EnemySpawner] Loaded enemyPrefab: {(enemyPrefab != null ? enemyPrefab.name : "NULL")}");
    }
    
    void Update()
    {
        if (!waveConfigured || currentWave == null)
            return;
        
        // Check if we should spawn an enemy
        if (Time.time - lastSpawnTime >= currentWave.spawnInterval)
        {
            SpawnNextEnemy();
            lastSpawnTime = Time.time;
        }
    }
    
    public void ConfigureForWave(WaveData wave, float difficulty)
    {
        currentWave = wave;
        difficultyMultiplier = difficulty;
        waveConfigured = true;
        lastSpawnTime = Time.time;
        currentEnemyIndex = 0;
        totalEnemiesSpawned = 0;
        
        // Build list of enemies to spawn
        enemiesToSpawn.Clear();
        foreach (var enemySpawn in wave.enemies)
        {
            for (int i = 0; i < enemySpawn.count; i++)
            {
                enemiesToSpawn.Add(enemySpawn);
            }
        }
        
        // Shuffle the list for variety
        ShuffleList(enemiesToSpawn);
        
        Debug.Log($"[EnemySpawner] Configured for wave with {enemiesToSpawn.Count} enemies, difficulty {difficulty}x");
    }
    
    void SpawnNextEnemy()
    {
        if (currentEnemyIndex >= enemiesToSpawn.Count)
        {
            // All enemies spawned for this wave
            return;
        }
        
        // Check max enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length >= maxEnemies)
        {
            Debug.Log($"[EnemySpawner] Max enemies reached, delaying spawn");
            return;
        }
        
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] EnemyPrefab is not assigned!");
            return;
        }
        
        // Get next enemy to spawn
        EnemySpawnData spawnData = enemiesToSpawn[currentEnemyIndex];
        
        // Spawn at random position on edge of arena
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 spawnPos = new Vector3(
            Mathf.Cos(angle) * arenaRadius,
            0.5f,
            Mathf.Sin(angle) * arenaRadius
        );
        
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        // Apply enemy stats with wave multipliers
        Enemy enemyScript = newEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.maxHealth *= spawnData.health * difficultyMultiplier;
            enemyScript.currentHealth = enemyScript.maxHealth;
            enemyScript.damage *= spawnData.damage * difficultyMultiplier;
            enemyScript.moveSpeed *= spawnData.moveSpeed;
            
            // Boss and mini-boss modifiers
            if (currentWave.isBossWave)
            {
                enemyScript.maxHealth *= 5f; // Bosses have 5x health
                enemyScript.currentHealth = enemyScript.maxHealth;
                enemyScript.damage *= 2f; // Bosses deal 2x damage
                
                // Make bosses visually larger
                newEnemy.transform.localScale = Vector3.one * 2f;
                
                // Change color to indicate boss
                Renderer renderer = newEnemy.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
            else if (currentWave.isMiniBossWave)
            {
                enemyScript.maxHealth *= 2.5f; // Mini-bosses have 2.5x health
                enemyScript.currentHealth = enemyScript.maxHealth;
                enemyScript.damage *= 1.5f; // Mini-bosses deal 1.5x damage
                
                // Make mini-bosses slightly larger
                newEnemy.transform.localScale = Vector3.one * 1.5f;
                
                // Change color to indicate mini-boss
                Renderer renderer = newEnemy.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.yellow;
                }
            }
        }
        
        currentEnemyIndex++;
        totalEnemiesSpawned++;
        
        // Notify LevelManager
        if (levelManager != null)
        {
            levelManager.OnEnemySpawned();
        }
        
        Debug.Log($"[EnemySpawner] Spawned enemy {totalEnemiesSpawned}/{enemiesToSpawn.Count} at {spawnPos}");
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}