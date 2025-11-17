using ArenaGame.Shared.Core;
using ArenaGame.Shared.Data;
using ArenaGame.Shared.Math;
using ArenaGame.Shared.Events;
using System.Collections.Generic;

namespace ArenaGame.Shared.Systems
{
    /// <summary>
    /// Time-based continuous enemy spawning system
    /// Handles multiple enemy types with different spawn rates that scale over time
    /// </summary>
    public static class LevelSpawnSystem
    {
        private static class SpawnState
        {
            public static bool isActive = false;
            public static int levelNumber = 1;
            public static float currentTime = 0f; // Time in seconds
            public static float spawnRateMultiplier = 1f; // Global spawn rate multiplier
            public static Dictionary<string, SpawnTimer> spawnTimers = new Dictionary<string, SpawnTimer>();
            public static List<EnemySpawnSchedule> activeSchedules = new List<EnemySpawnSchedule>();
        }
        
        private class SpawnTimer
        {
            public string enemyType;
            public float spawnInterval; // Current spawn interval (scaled)
            public float nextSpawnTime; // When to spawn next (in seconds)
            public float healthMultiplier;
        }
        
        public struct EnemySpawnSchedule
        {
            public string enemyType;
            public float startTime;
            public float initialSpawnInterval;
            public float healthMultiplier;
            public bool isActive;
        }
        
        /// <summary>
        /// Initialize level spawning with configuration
        /// </summary>
        public static void InitializeLevel(int levelNumber, List<EnemySpawnSchedule> schedules, float spawnRateIncreaseInterval, float spawnRateIncreaseMultiplier)
        {
            SpawnState.isActive = true;
            SpawnState.levelNumber = levelNumber;
            SpawnState.currentTime = 0f;
            SpawnState.spawnRateMultiplier = 1f;
            SpawnState.spawnTimers.Clear();
            SpawnState.activeSchedules.Clear();
            
            // Add all schedules
            foreach (var schedule in schedules)
            {
                if (schedule.isActive)
                {
                    SpawnState.activeSchedules.Add(schedule);
                    System.Diagnostics.Debug.WriteLine($"[spawn] Schedule added: {schedule.enemyType}, startTime={schedule.startTime}s, interval={schedule.initialSpawnInterval}s, healthMult={schedule.healthMultiplier}x");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[spawn] Initialized level {levelNumber} with {SpawnState.activeSchedules.Count} spawn schedules");
        }
        
        /// <summary>
        /// Update spawn system - called every simulation tick
        /// </summary>
        public static void Update(SimulationWorld world)
        {
            if (!SpawnState.isActive)
            {
                // Log once that system is not active
                if (world.CurrentTick == 0 || world.CurrentTick % 300 == 0) // Log every 10 seconds
                {
                    System.Diagnostics.Debug.WriteLine($"[spawn] LevelSpawnSystem.Update: System is NOT active (tick={world.CurrentTick})");
                }
                return;
            }
            
            // Update current time (30 ticks per second)
            SpawnState.currentTime = world.CurrentTick * SimulationConfig.FIXED_DELTA_TIME.ToFloat();
            
            // Log every second to verify Update is being called
            if (world.CurrentTick % 30 == 0) // Every 1 second at 30 tps
            {
                System.Diagnostics.Debug.WriteLine($"[spawn] LevelSpawnSystem.Update: tick={world.CurrentTick}, time={SpawnState.currentTime:F2}s, activeSchedules={SpawnState.activeSchedules.Count}, timers={SpawnState.spawnTimers.Count}");
            }
            
            // Check for spawn rate increase (every 5 seconds by default)
            // This is handled by scaling the spawn intervals dynamically
            
            // Check each active schedule
            foreach (var schedule in SpawnState.activeSchedules)
            {
                // Check if schedule should start
                if (SpawnState.currentTime >= schedule.startTime)
                {
                    // Get or create spawn timer for this enemy type
                    if (!SpawnState.spawnTimers.TryGetValue(schedule.enemyType, out SpawnTimer timer))
                    {
                        // Calculate current spawn interval with scaling
                        float scaledInterval = CalculateScaledSpawnInterval(schedule.initialSpawnInterval);
                        
                        // First spawn should happen immediately when schedule starts
                        timer = new SpawnTimer
                        {
                            enemyType = schedule.enemyType,
                            spawnInterval = scaledInterval,
                            nextSpawnTime = SpawnState.currentTime, // Spawn immediately
                            healthMultiplier = schedule.healthMultiplier
                        };
                        SpawnState.spawnTimers[schedule.enemyType] = timer;
                        System.Diagnostics.Debug.WriteLine($"[spawn] Created timer for {schedule.enemyType}: first spawn at {timer.nextSpawnTime:F2}s, interval={timer.spawnInterval:F3}s");
                    }
                    
                    // Check if it's time to spawn
                    if (SpawnState.currentTime >= timer.nextSpawnTime)
                    {
                        // Spawn enemy
                        SpawnEnemy(world, timer.enemyType, timer.healthMultiplier);
                        
                        // Calculate next spawn time with updated scaling
                        float oldInterval = timer.spawnInterval;
                        timer.spawnInterval = CalculateScaledSpawnInterval(schedule.initialSpawnInterval);
                        timer.nextSpawnTime = SpawnState.currentTime + timer.spawnInterval;
                        System.Diagnostics.Debug.WriteLine($"[spawn] Spawned {timer.enemyType} at {SpawnState.currentTime:F2}s, nextSpawn={timer.nextSpawnTime:F2}s (interval: {oldInterval:F3}s -> {timer.spawnInterval:F3}s)");
                    }
                }
            }
        }
        
        /// <summary>
        /// Calculate spawn interval with time-based scaling
        /// Spawn rate increases by 10% every 5 seconds
        /// </summary>
        private static float CalculateScaledSpawnInterval(float baseInterval)
        {
            // Spawn rate multiplier increases every 5 seconds
            // After 5s: 1.1x, after 10s: 1.21x, after 15s: 1.331x, etc.
            int intervalsPassed = (int)(SpawnState.currentTime / 5f);
            float multiplier = 1f;
            for (int i = 0; i < intervalsPassed; i++)
            {
                multiplier *= 1.1f; // 10% increase
            }
            
            // Spawn interval decreases as rate increases (faster spawning)
            float scaledInterval = baseInterval / multiplier;
            
            // Debug log every 5 seconds to track scaling
            if (intervalsPassed > 0 && SpawnState.currentTime % 5f < 0.1f)
            {
                System.Diagnostics.Debug.WriteLine($"[spawn] Spawn rate scaling: time={SpawnState.currentTime:F2}s, intervals={intervalsPassed}, multiplier={multiplier:F3}x, baseInterval={baseInterval:F3}s -> scaled={scaledInterval:F3}s");
            }
            
            return scaledInterval;
        }
        
        /// <summary>
        /// Spawn a single enemy
        /// </summary>
        private static void SpawnEnemy(SimulationWorld world, string enemyType, float healthMultiplier)
        {
            // Get base enemy config
            EnemyConfig baseConfig = EnemyData.GetConfig(enemyType);
            
            // Apply health multiplier
            EnemyConfig modifiedConfig = new EnemyConfig
            {
                EnemyType = baseConfig.EnemyType,
                MaxHealth = baseConfig.MaxHealth * Fix64.FromFloat(healthMultiplier),
                MoveSpeed = baseConfig.MoveSpeed,
                Damage = baseConfig.Damage,
                AttackRange = baseConfig.AttackRange,
                AttackSpeed = baseConfig.AttackSpeed,
                IsBoss = baseConfig.IsBoss,
                IsMiniBoss = baseConfig.IsMiniBoss
            };
            
            // Get spawn position (deterministic based on current time and enemy type)
            FixV2 spawnPos = GetSpawnPosition(world, enemyType);
            
            // Spawn enemy
            SpawnSystem.SpawnEnemy(world, modifiedConfig, spawnPos);
            
            System.Diagnostics.Debug.WriteLine($"[spawn] ✓ Spawned {enemyType} at time {SpawnState.currentTime:F2}s (health: {modifiedConfig.MaxHealth.ToFloat():F1}, pos: {spawnPos.X.ToFloat():F1}, {spawnPos.Y.ToFloat():F1})");
        }
        
        /// <summary>
        /// Get deterministic spawn position
        /// </summary>
        private static FixV2 GetSpawnPosition(SimulationWorld world, string enemyType)
        {
            // Use deterministic circular spawn based on current time and enemy type
            int spawnIndex = (int)(SpawnState.currentTime * 10) + enemyType.GetHashCode();
            FixV2 spawnPos = SpawnSystem.GetSpawnPositionOnCircle(
                spawnIndex,
                100, // Total positions (doesn't matter for deterministic calculation)
                Fix64.FromInt(10) // Spawn radius
            );
            return spawnPos;
        }
        
        /// <summary>
        /// Reset spawn system
        /// </summary>
        public static void Reset()
        {
            SpawnState.isActive = false;
            SpawnState.levelNumber = 1;
            SpawnState.currentTime = 0f;
            SpawnState.spawnRateMultiplier = 1f;
            SpawnState.spawnTimers.Clear();
            SpawnState.activeSchedules.Clear();
        }
        
        /// <summary>
        /// Get current level time
        /// </summary>
        public static float GetCurrentTime()
        {
            return SpawnState.currentTime;
        }
    }
}

