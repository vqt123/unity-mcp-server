using UnityEngine;
using ArenaGame.Shared.Systems;
using System.Collections.Generic;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages level spawning by loading LevelConfigSO and initializing LevelSpawnSystem
    /// </summary>
    public class LevelSpawnManager : MonoBehaviour
    {
        [Header("Level Configuration")]
        [Tooltip("Level config asset to use. If null, will try to load from Resources.")]
        public LevelConfigSO levelConfig;
        
        [Tooltip("Level number to load from Resources if levelConfig is null")]
        public int levelNumber = 1;
        
        private void Start()
        {
            InitializeLevel();
        }
        
        private void InitializeLevel()
        {
            // Load level config if not assigned
            if (levelConfig == null)
            {
                // Try loading from LevelConfigs folder first
                levelConfig = Resources.Load<LevelConfigSO>($"LevelConfigs/LevelConfig_{levelNumber}");
                if (levelConfig == null)
                {
                    // Fallback to root Resources folder
                    levelConfig = Resources.Load<LevelConfigSO>($"LevelConfig_{levelNumber}");
                }
                if (levelConfig == null)
                {
                    Debug.LogError($"[spawn] Could not load LevelConfig_{levelNumber} from Resources/LevelConfigs/ or Resources/!");
                    return;
                }
            }
            
            Debug.Log($"[spawn] LevelSpawnManager: Loaded config '{levelConfig.name}', levelNumber={levelConfig.levelNumber}, schedules count={levelConfig.spawnSchedules.Count}");
            
            // Convert ScriptableObject schedules to system schedules
            List<LevelSpawnSystem.EnemySpawnSchedule> schedules = new List<LevelSpawnSystem.EnemySpawnSchedule>();
            
            foreach (var schedule in levelConfig.spawnSchedules)
            {
                Debug.Log($"[spawn] LevelSpawnManager: Processing schedule - enemyType={schedule.enemyType}, startTime={schedule.startTime}s, interval={schedule.initialSpawnInterval}s, healthMult={schedule.healthMultiplier}x, isActive={schedule.isActive}");
                
                if (schedule.isActive)
                {
                    schedules.Add(new LevelSpawnSystem.EnemySpawnSchedule
                    {
                        enemyType = schedule.enemyType,
                        startTime = schedule.startTime,
                        initialSpawnInterval = schedule.initialSpawnInterval,
                        healthMultiplier = schedule.healthMultiplier,
                        isActive = schedule.isActive
                    });
                    Debug.Log($"[spawn] LevelSpawnManager: Added active schedule for {schedule.enemyType}");
                }
                else
                {
                    Debug.LogWarning($"[spawn] LevelSpawnManager: Skipping inactive schedule for {schedule.enemyType}");
                }
            }
            
            Debug.Log($"[spawn] LevelSpawnManager: About to initialize LevelSpawnSystem with {schedules.Count} schedules, rateIncreaseInterval={levelConfig.spawnRateIncreaseInterval}s, rateIncreaseMult={levelConfig.spawnRateIncreaseMultiplier}");
            
            // Initialize level spawn system
            LevelSpawnSystem.InitializeLevel(
                levelConfig.levelNumber,
                schedules,
                levelConfig.spawnRateIncreaseInterval,
                levelConfig.spawnRateIncreaseMultiplier
            );
            
            Debug.Log($"[spawn] LevelSpawnManager: Initialized level {levelConfig.levelNumber}: {levelConfig.levelName} with {schedules.Count} spawn schedules");
        }
        
        private void OnDestroy()
        {
            LevelSpawnSystem.Reset();
        }
    }
}

