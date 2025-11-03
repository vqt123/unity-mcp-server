using ArenaGame.Shared.Core;
using ArenaGame.Shared.Data;
using ArenaGame.Shared.Math;
using ArenaGame.Shared.Events;

namespace ArenaGame.Shared.Systems
{
    /// <summary>
    /// Manages wave spawning deterministically based on simulation ticks
    /// All wave logic is deterministic and replayable
    /// </summary>
    public static class WaveSystem
    {
        // Wave state stored in SimulationWorld
        private static class WaveState
        {
            public static int currentWave = 0;
            public static bool waveActive = false;
            public static int enemiesPerWave = 5;
            public static int enemiesSpawnedThisWave = 0;
            public static int nextWaveStartTick = -1; // -1 means not scheduled
            public static bool hasStartedFirstWave = false;
        }

        // Constants
        private const int INITIAL_WAVE_DELAY_TICKS = 60; // 2 seconds at 30 tps
        private const int WAVE_COMPLETE_DELAY_TICKS = 90; // 3 seconds at 30 tps
        private const int INITIAL_ENEMIES_PER_WAVE = 5;
        private const int ENEMIES_INCREASE_PER_WAVE = 2;

        /// <summary>
        /// Update wave system - called every simulation tick
        /// </summary>
        public static void Update(SimulationWorld world)
        {
            int currentTick = world.CurrentTick;

            // Check if first wave should start (after first hero spawns or after delay)
            if (!WaveState.hasStartedFirstWave)
            {
                // Start first wave if heroes exist, or after initial delay
                if (world.HeroIds.Count > 0 || currentTick >= INITIAL_WAVE_DELAY_TICKS)
                {
                    StartWave(world, 1);
                }
                return;
            }

            // If wave is active, check if it's complete
            if (WaveState.waveActive)
            {
                // Wave is complete when all enemies are killed and we've spawned all planned enemies
                if (world.EnemyIds.Count == 0 && 
                    WaveState.enemiesSpawnedThisWave >= WaveState.enemiesPerWave)
                {
                    EndWave(world);
                }
            }
            else
            {
                // Wave is not active - check if next wave should start
                if (WaveState.nextWaveStartTick >= 0 && currentTick >= WaveState.nextWaveStartTick)
                {
                    int nextWave = WaveState.currentWave + 1;
                    StartWave(world, nextWave);
                }
            }
        }

        private static void StartWave(SimulationWorld world, int waveNumber)
        {
            WaveState.currentWave = waveNumber;
            WaveState.waveActive = true;
            WaveState.enemiesSpawnedThisWave = 0;
            WaveState.hasStartedFirstWave = true;

            // Calculate enemies for this wave
            WaveState.enemiesPerWave = INITIAL_ENEMIES_PER_WAVE + (waveNumber - 1) * ENEMIES_INCREASE_PER_WAVE;

            // Emit wave started event
            world.AddEvent(new WaveStartedEvent
            {
                Tick = world.CurrentTick,
                WaveNumber = waveNumber,
                LevelNumber = 1
            });

            // Spawn all enemies for this wave immediately (deterministic)
            SpawnEnemiesForWave(world, waveNumber);
        }

        private static void SpawnEnemiesForWave(SimulationWorld world, int waveNumber)
        {
            for (int i = 0; i < WaveState.enemiesPerWave; i++)
            {
                // Get deterministic spawn position
                FixV2 spawnPos = SpawnSystem.GetSpawnPositionOnCircle(
                    i,
                    WaveState.enemiesPerWave,
                    Fix64.FromInt(10)
                );

                // Determine enemy type based on wave (deterministic algorithm)
                string enemyType = GetEnemyTypeForWave(waveNumber, i);

                // Spawn enemy directly via SpawnSystem (deterministic)
                EnemyConfig enemyData = EnemyData.GetConfig(enemyType);
                SpawnSystem.SpawnEnemy(world, enemyData, spawnPos);
                
                WaveState.enemiesSpawnedThisWave++;
            }
        }

        private static string GetEnemyTypeForWave(int waveNumber, int enemyIndex)
        {
            // Deterministic enemy type selection
            // Every 5th wave has a boss
            if (waveNumber % 5 == 0 && enemyIndex == 0)
            {
                return "Boss";
            }

            // Every 3rd wave has a mini-boss
            if (waveNumber % 3 == 0 && enemyIndex == 0)
            {
                return "MiniBoss";
            }

            // Mix of enemy types for later waves
            if (waveNumber > 3)
            {
                if (enemyIndex % 3 == 0) return "FastRunner";
                if (enemyIndex % 3 == 1) return "Tank";
            }

            return "BasicGrunt";
        }

        private static void EndWave(SimulationWorld world)
        {
            WaveState.waveActive = false;

            // Emit wave completed event
            world.AddEvent(new WaveCompletedEvent
            {
                Tick = world.CurrentTick,
                WaveNumber = WaveState.currentWave,
                LevelNumber = 1
            });

            // Schedule next wave
            WaveState.nextWaveStartTick = world.CurrentTick + WAVE_COMPLETE_DELAY_TICKS;
        }

        /// <summary>
        /// Reset wave state (for new arena)
        /// </summary>
        public static void Reset()
        {
            WaveState.currentWave = 0;
            WaveState.waveActive = false;
            WaveState.enemiesPerWave = INITIAL_ENEMIES_PER_WAVE;
            WaveState.enemiesSpawnedThisWave = 0;
            WaveState.nextWaveStartTick = -1;
            WaveState.hasStartedFirstWave = false;
        }
    }
}
