using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public int currentLevelNumber = 1;
    public int currentWaveNumber = 0;
    
    private LevelData currentLevel;
    private WaveData currentWave;
    private float waveStartTime;
    private bool waveActive = false;
    private int enemiesSpawnedInWave = 0;
    private int totalEnemiesToSpawn = 0;
    
    // UI References
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI waveText;
    
    // Reference to spawner
    private EnemySpawner enemySpawner;
    
    void Start()
    {
        enemySpawner = GetComponent<EnemySpawner>();
        if (enemySpawner == null)
        {
            Debug.LogError("[LevelManager] No EnemySpawner component found!");
        }
        
        // Find UI elements
        GameObject levelTextObj = GameObject.Find("LevelText");
        if (levelTextObj != null)
        {
            levelText = levelTextObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("[LevelManager] Found LevelText UI");
        }
        
        GameObject waveTextObj = GameObject.Find("WaveText");
        if (waveTextObj != null)
        {
            waveText = waveTextObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("[LevelManager] Found WaveText UI");
        }
        
        LoadLevel(currentLevelNumber);
    }
    
    void Update()
    {
        if (waveActive)
        {
            // Check if wave duration has elapsed
            float waveElapsed = Time.time - waveStartTime;
            if (waveElapsed >= currentWave.duration && enemiesSpawnedInWave >= totalEnemiesToSpawn)
            {
                EndWave();
            }
        }
    }
    
    public void LoadLevel(int levelNumber)
    {
        currentLevel = ConfigManager.Instance.GetLevelData(levelNumber);
        if (currentLevel == null)
        {
            Debug.LogError($"[LevelManager] Level {levelNumber} not found!");
            return;
        }
        
        currentLevelNumber = levelNumber;
        currentWaveNumber = 0;
        
        Debug.Log($"[LevelManager] Loaded Level {levelNumber}: {currentLevel.levelName} (Difficulty: {currentLevel.difficultyMultiplier}x)");
        
        UpdateLevelUI();
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        if (currentLevel == null || currentWaveNumber >= currentLevel.waves.Count)
        {
            // Level complete!
            OnLevelComplete();
            return;
        }
        
        currentWave = currentLevel.waves[currentWaveNumber];
        waveStartTime = Time.time;
        waveActive = true;
        enemiesSpawnedInWave = 0;
        
        // Calculate total enemies to spawn
        totalEnemiesToSpawn = 0;
        foreach (var enemySpawn in currentWave.enemies)
        {
            totalEnemiesToSpawn += enemySpawn.count;
        }
        
        // Configure spawner for this wave
        if (enemySpawner != null)
        {
            enemySpawner.ConfigureForWave(currentWave, currentLevel.difficultyMultiplier);
        }
        
        string waveType = currentWave.isBossWave ? "BOSS" : (currentWave.isMiniBossWave ? "MINI-BOSS" : "");
        Debug.Log($"[LevelManager] Starting Wave {currentWaveNumber + 1}/{currentLevel.waves.Count} {waveType}");
        Debug.Log($"[LevelManager] - Duration: {currentWave.duration}s, Spawn Interval: {currentWave.spawnInterval}s");
        Debug.Log($"[LevelManager] - Total Enemies: {totalEnemiesToSpawn}");
        
        UpdateWaveUI();
    }
    
    void EndWave()
    {
        waveActive = false;
        currentWaveNumber++;
        
        Debug.Log($"[LevelManager] Wave {currentWaveNumber} complete!");
        
        // Short delay before next wave
        Invoke(nameof(StartNextWave), 3f);
    }
    
    void OnLevelComplete()
    {
        Debug.Log($"[LevelManager] Level {currentLevelNumber} complete!");
        
        // Load next level after delay
        int nextLevel = currentLevelNumber + 1;
        LevelData nextLevelData = ConfigManager.Instance.GetLevelData(nextLevel);
        
        if (nextLevelData != null)
        {
            Invoke(nameof(LoadNextLevel), 5f);
        }
        else
        {
            Debug.Log("[LevelManager] All levels complete! Game finished!");
        }
    }
    
    void LoadNextLevel()
    {
        LoadLevel(currentLevelNumber + 1);
    }
    
    public void OnEnemySpawned()
    {
        enemiesSpawnedInWave++;
    }
    
    void UpdateLevelUI()
    {
        if (levelText != null && currentLevel != null)
        {
            levelText.text = $"Level {currentLevelNumber}: {currentLevel.levelName}";
        }
    }
    
    void UpdateWaveUI()
    {
        if (waveText != null && currentLevel != null)
        {
            string waveType = "";
            if (currentWave.isBossWave)
                waveType = " [BOSS]";
            else if (currentWave.isMiniBossWave)
                waveType = " [MINI-BOSS]";
                
            waveText.text = $"Wave {currentWaveNumber + 1}/{currentLevel.waves.Count}{waveType}";
        }
    }
    
    public bool IsWaveActive()
    {
        return waveActive;
    }
    
    public WaveData GetCurrentWave()
    {
        return currentWave;
    }
    
    public LevelData GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public float GetDifficultyMultiplier()
    {
        return currentLevel?.difficultyMultiplier ?? 1f;
    }
}

