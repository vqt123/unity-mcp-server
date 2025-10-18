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
            // Wait a frame for GameSimulation to be ready
            Invoke(nameof(SpawnParty), 0.1f);
        }
        
        private void SpawnParty()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogError("[PartySpawner] PlayerDataManager not found! Spawning default hero.");
                SpawnDefaultHero();
                return;
            }
            
            if (GameSimulation.Instance == null)
            {
                Debug.LogError("[PartySpawner] GameSimulation not found!");
                return;
            }
            
            var party = PlayerDataManager.Instance.HeroInventory.partyHeroes;
            
            if (party.Count == 0)
            {
                Debug.LogError("[PartySpawner] Party is empty! Spawning default hero.");
                SpawnDefaultHero();
                return;
            }
            
            Debug.Log($"[PartySpawner] Spawning {party.Count} heroes from party");
            
            // Spawn heroes in a formation
            for (int i = 0; i < party.Count; i++)
            {
                FixV2 position = CalculateSpawnPosition(i, party.Count);
                SpawnHero(party[i], position);
            }
            
            // Wait for heroes to spawn then notify systems
            StartCoroutine(WaitForHeroesSpawn(party.Count));
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

