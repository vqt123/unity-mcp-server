using UnityEngine;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Math;
using System.Collections.Generic;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Spawns the player's party heroes at the start of battle
    /// </summary>
    public class PartySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 2f;
        
        private List<EntityId> partyHeroIds = new List<EntityId>();
        private bool heroesSpawned = false;
        
        public static PartySpawner Instance { get; private set; }
        public bool HeroesSpawned => heroesSpawned;
        public List<EntityId> PartyHeroIds => partyHeroIds;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        void Start()
        {
            // Don't auto-spawn - wait for hero selection
            Debug.Log("[PartySpawner] Ready and waiting for hero selection");
        }
        
        public void SpawnSelectedHero(string heroType)
        {
            if (GameSimulation.Instance == null)
            {
                Debug.LogError("[PartySpawner] GameSimulation not found!");
                return;
            }
            
            Debug.Log($"[PartySpawner] Spawning selected hero: {heroType}");
            
            // Spawn hero at center
            SpawnHero(heroType, FixV2.Zero);
            
            // Wait for hero to spawn then start game
            StartCoroutine(WaitForHeroSpawn(heroType));
        }
        
        private System.Collections.IEnumerator WaitForHeroSpawn(string heroType)
        {
            yield return null;
            yield return null;
            
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                
                if (world.HeroIds.Count > 0)
                {
                    partyHeroIds.Clear();
                    partyHeroIds.Add(world.HeroIds[0]);
                    heroesSpawned = true;
                    
                    Debug.Log($"[PartySpawner] Hero {heroType} spawned, starting game");
                    
                    // Notify UpgradeUIManager
                    UpgradeUIManager upgradeUI = FindFirstObjectByType<UpgradeUIManager>();
                    if (upgradeUI != null)
                    {
                        upgradeUI.SetPlayerHero(partyHeroIds[0]);
                    }
                    
                    // Start waves
                    WaveManager waveManager = FindFirstObjectByType<WaveManager>();
                    if (waveManager != null)
                    {
                        waveManager.OnHeroSelected();
                    }
                }
            }
        }
        
        private FixV2 CalculateSpawnPosition(int index, int totalHeroes)
        {
            if (totalHeroes == 1)
            {
                return FixV2.Zero;
            }
            
            // Arrange heroes in a circle
            float angleStep = 360f / totalHeroes;
            float angle = angleStep * index * Mathf.Deg2Rad;
            
            float x = Mathf.Cos(angle) * spawnRadius;
            float y = Mathf.Sin(angle) * spawnRadius;
            
            return new FixV2(Fix64.FromFloat(x), Fix64.FromFloat(y));
        }
        
        private void SpawnHero(string heroType, FixV2 position)
        {
            SpawnHeroCommand cmd = new SpawnHeroCommand
            {
                HeroType = heroType,
                Position = position
            };
            
            GameSimulation.Instance.QueueCommand(cmd);
            Debug.Log($"[PartySpawner] Spawning {heroType} at {position}");
        }
        
        private void SpawnDefaultHero()
        {
            SpawnHero("DefaultHero", FixV2.Zero);
            StartCoroutine(WaitForHeroesSpawn(1));
        }
        
        private System.Collections.IEnumerator WaitForHeroesSpawn(int expectedCount)
        {
            yield return null; // Wait one frame
            yield return null; // Wait another frame for simulation to process
            
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                
                if (world.HeroIds.Count >= expectedCount)
                {
                    partyHeroIds.Clear();
                    
                    // Get all spawned heroes
                    foreach (var heroId in world.HeroIds)
                    {
                        partyHeroIds.Add(heroId);
                    }
                    
                    heroesSpawned = true;
                    Debug.Log($"[PartySpawner] {partyHeroIds.Count} heroes spawned successfully");
                    
                    // Notify UpgradeUIManager about the first hero (for level-up events)
                    if (partyHeroIds.Count > 0)
                    {
                        UpgradeUIManager upgradeUI = FindFirstObjectByType<UpgradeUIManager>();
                        if (upgradeUI != null)
                        {
                            upgradeUI.SetPlayerHero(partyHeroIds[0]);
                        }
                    }
                    
                    // Notify WaveManager to start waves
                    WaveManager waveManager = FindFirstObjectByType<WaveManager>();
                    if (waveManager != null)
                    {
                        waveManager.OnHeroSelected();
                    }
                }
                else
                {
                    Debug.LogWarning($"[PartySpawner] Expected {expectedCount} heroes but found {world.HeroIds.Count}");
                }
            }
        }
    }
}

