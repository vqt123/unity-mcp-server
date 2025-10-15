using UnityEngine;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// Simple test spawner - spawns ONE enemy at a known location when hero is ready
    /// </summary>
    public class TestEnemySpawner : MonoBehaviour
    {
        [SerializeField] private bool enableTestSpawn = true;
        [SerializeField] private float spawnDelay = 3f; // Wait 3 seconds after hero spawns
        
        private bool hasSpawned = false;
        
        void Update()
        {
            if (!enableTestSpawn || hasSpawned) return;
            if (GameSimulation.Instance == null) return;
            
            // Check if hero exists
            if (GameSimulation.Instance.Simulation.World.HeroIds.Count > 0)
            {
                if (spawnDelay > 0)
                {
                    spawnDelay -= Time.deltaTime;
                }
                else
                {
                    SpawnTestEnemy();
                    hasSpawned = true;
                }
            }
        }
        
        private void SpawnTestEnemy()
        {
            GameLogger.Log("[TestSpawner] ========== SPAWNING TEST ENEMY ==========");
            GameLogger.Log("[TestSpawner] Position: (8, 0) - should be 8 units to the right of center");
            GameLogger.Log("[TestSpawner] Note: Arena radius is 10, so spawning at 8 to stay well inside bounds");
            
            // Spawn enemy at (8, 0) - to the right of the hero, well inside arena radius of 10
            FixV2 spawnPos = new FixV2(Fix64.FromInt(8), Fix64.Zero);
            
            SpawnEnemyCommand cmd = new SpawnEnemyCommand
            {
                EnemyType = "BasicGrunt",
                Position = spawnPos
            };
            
            GameSimulation.Instance.QueueCommand(cmd);
            GameLogger.Log("[TestSpawner] SpawnEnemyCommand queued to simulation");
        }
    }
}

