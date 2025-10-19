using UnityEngine;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Data;
using ArenaGame.Shared.Math;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Initializes the game simulation with starting heroes
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private int numberOfHeroes = 1;
        [SerializeField] private string heroType = "DefaultHero";
        
        private EntityId playerHeroId;
        
        void Start()
        {
            // GameInitializer is now obsolete - PartySpawner handles hero spawning
            // This script is kept for backwards compatibility/manual testing only
            Debug.Log("[GameInit] GameInitializer is obsolete - use PartySpawner instead");
        }
        
        private void InitializeGame()
        {
            if (GameSimulation.Instance == null)
            {
                Debug.LogError("[GameInit] GameSimulation not found!");
                return;
            }
            
            // Spawn heroes in circle formation
            for (int i = 0; i < numberOfHeroes; i++)
            {
                FixV2 spawnPos = GetHeroSpawnPosition(i, numberOfHeroes);
                
                SpawnHeroCommand spawnCmd = new SpawnHeroCommand
                {
                    HeroType = heroType,
                    Position = spawnPos
                };
                
                GameSimulation.Instance.QueueCommand(spawnCmd);
                
                // Track first hero as player
                if (i == 0)
                {
                    // Will get ID after first tick
                }
            }
            
            Debug.Log($"[GameInit] Spawned {numberOfHeroes} heroes");
            
            // Spawn initial enemies for testing
            SpawnTestEnemies();
        }
        
        private void SpawnTestEnemies()
        {
            // Don't spawn test enemies - let WaveManager handle it
            // This method is kept for manual testing if needed
        }
        
        private FixV2 GetHeroSpawnPosition(int index, int total)
        {
            if (total == 1)
            {
                return FixV2.Zero;
            }
            
            float angle = index * Mathf.PI * 2f / total;
            float radius = 2f;
            
            return FixV2.FromFloat(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
        }
    }
}

