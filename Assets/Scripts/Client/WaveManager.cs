using UnityEngine;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Systems;
using ArenaGame.Shared.Data;
using ArenaGame.Shared.Math;
using EnemyConfig = ArenaGame.Shared.Data.EnemyConfig;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages wave spawning by sending spawn commands to simulation
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Config")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private float timeBetweenWaves = 3f;
        
        private bool waveActive = false;
        private float waveTimer = 0f;
        private int enemiesSpawnedThisWave = 0;
        private int enemiesPerWave = 5;
        
        void OnEnable()
        {
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }
        
        void Start()
        {
            // Wait for hero selection - PartySpawner will call OnHeroSelected()
            Debug.Log("[Wave] Waiting for hero selection...");
        }
        
        public void OnHeroSelected()
        {
            // Hero selected, start first wave
            Debug.Log("[Wave] Hero selected, starting first wave...");
            Invoke(nameof(StartNextWave), 2f);
        }
        
        void Update()
        {
            if (waveActive)
            {
                waveTimer += Time.deltaTime;
                
                // Check if wave is complete
                if (GameSimulation.Instance != null)
                {
                    int enemyCount = GameSimulation.Instance.Simulation.World.EnemyIds.Count;
                    if (enemyCount == 0 && enemiesSpawnedThisWave >= enemiesPerWave)
                    {
                        EndWave();
                    }
                }
            }
        }
        
        private void StartNextWave()
        {
            currentWave++;
            waveActive = true;
            waveTimer = 0f;
            enemiesSpawnedThisWave = 0;
            
            // Increase difficulty
            enemiesPerWave = 5 + (currentWave - 1) * 2;
            
            Debug.Log($"[Wave] Starting wave {currentWave} with {enemiesPerWave} enemies");
            
            // Send wave start command
            StartWaveCommand cmd = new StartWaveCommand
            {
                WaveNumber = currentWave,
                LevelNumber = 1
            };
            GameSimulation.Instance.QueueCommand(cmd);
            
            // Spawn enemies
            SpawnEnemiesForWave();
        }
        
        private void SpawnEnemiesForWave()
        {
            // DISABLED: No enemies for interpolation debugging
            // for (int i = 0; i < enemiesPerWave; i++)
            // {
            //     // Get deterministic spawn position
            //     FixV2 spawnPos = SpawnSystem.GetSpawnPositionOnCircle(
            //         i,
            //         enemiesPerWave,
            //         Fix64.FromInt(10)
            //     );
            //     
            //     // Determine enemy type based on wave
            //     string enemyType = GetEnemyTypeForWave(i);
            //     
            //     // Spawn via simulation
            //     SpawnEnemy(enemyType, spawnPos);
            //     enemiesSpawnedThisWave++;
            // }
        }
        
        private void SpawnEnemy(string enemyType, FixV2 position)
        {
            if (GameSimulation.Instance != null)
            {
                SpawnEnemyCommand cmd = new SpawnEnemyCommand
                {
                    EnemyType = enemyType,
                    Position = position
                };
                GameSimulation.Instance.QueueCommand(cmd);
                Debug.Log($"[Wave] Spawning {enemyType} at {position.X.ToInt()},{position.Y.ToInt()}");
            }
        }
        
        private string GetEnemyTypeForWave(int enemyIndex)
        {
            // Every 5th wave is boss
            if (currentWave % 5 == 0 && enemyIndex == 0)
            {
                return "Boss";
            }
            
            // Every 3rd wave has mini-boss
            if (currentWave % 3 == 0 && enemyIndex == 0)
            {
                return "MiniBoss";
            }
            
            // Mix of enemy types
            if (currentWave > 3)
            {
                if (enemyIndex % 3 == 0) return "FastRunner";
                if (enemyIndex % 3 == 1) return "Tank";
            }
            
            return "BasicGrunt";
        }
        
        private void EndWave()
        {
            waveActive = false;
            Debug.Log($"[Wave] Wave {currentWave} complete!");
            
            // TODO: Show upgrade screen
            // For now, just start next wave
            Invoke(nameof(StartNextWave), timeBetweenWaves);
        }
        
        private void OnEnemyKilled(ISimulationEvent evt)
        {
            // Enemy killed, wave might be complete
        }
    }
}

