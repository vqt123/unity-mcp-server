using UnityEngine;
using System.Collections.Generic;

namespace ArenaGame.Client
{
    /// <summary>
    /// Level configuration for time-based enemy spawning
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ArenaGame/Level Config", order = 1)]
    public class LevelConfigSO : ScriptableObject
    {
        [Header("Level Info")]
        public int levelNumber = 1;
        public string levelName = "Level 1";
        
        [Header("Spawn Schedules")]
        [Tooltip("List of enemy spawn schedules. Each schedule defines when and how enemies spawn.")]
        public List<EnemySpawnSchedule> spawnSchedules = new List<EnemySpawnSchedule>();
        
        [Header("Spawn Rate Scaling")]
        [Tooltip("How often spawn rate increases (in seconds)")]
        public float spawnRateIncreaseInterval = 5f;
        
        [Tooltip("Spawn rate multiplier increase per interval (e.g., 0.1 = 10% increase)")]
        public float spawnRateIncreaseMultiplier = 0.1f;
    }
    
    /// <summary>
    /// Defines a spawn schedule for a specific enemy type
    /// </summary>
    [System.Serializable]
    public class EnemySpawnSchedule
    {
        [Tooltip("Enemy type to spawn (must match EnemyData config)")]
        public string enemyType = "BasicGrunt";
        
        [Tooltip("When this schedule starts (in seconds from level start)")]
        public float startTime = 0f;
        
        [Tooltip("Initial spawn interval in seconds (e.g., 1.0 = 1 per second)")]
        public float initialSpawnInterval = 1f;
        
        [Tooltip("Health multiplier for this enemy type (e.g., 4.0 = 4x health)")]
        public float healthMultiplier = 1f;
        
        [Tooltip("Whether this schedule is active")]
        public bool isActive = true;
    }
}

