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
        
        private List<string> spawnedHeroTypes = new List<string>();
        private bool heroesSpawned = false;
        
        public static PartySpawner Instance { get; private set; }
        public bool HeroesSpawned => heroesSpawned;
        public List<string> SpawnedHeroTypes => spawnedHeroTypes;
        
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
            spawnedHeroTypes.Add(heroType);
            
            // Waves will start automatically from WaveSystem when hero spawns
            // No need to manually trigger - WaveSystem checks for heroes each tick
            
            // Track that we've spawned the first hero
            heroesSpawned = true;
            
            Debug.Log($"[PartySpawner] Hero {heroType} spawn command queued, game started");
        }
        
        public void SpawnAdditionalHero(string heroType)
        {
            if (GameSimulation.Instance == null)
            {
                Debug.LogError("[PartySpawner] GameSimulation not found!");
                return;
            }
            
            Debug.Log($"[PartySpawner] Spawning additional hero: {heroType}");
            
            // Calculate spawn position based on number of spawned heroes
            int heroCount = spawnedHeroTypes.Count;
            FixV2 spawnPos = CalculateSpawnPosition(heroCount, heroCount + 1);
            
            // Spawn hero
            SpawnHero(heroType, spawnPos);
            spawnedHeroTypes.Add(heroType);
            
            Debug.Log($"[PartySpawner] Additional hero {heroType} spawn command queued at position {spawnPos}");
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
            
            Debug.Log($"[PartySpawner] Creating SpawnHeroCommand: {heroType} at {position}");
            
            if (GameSimulation.Instance == null)
            {
                Debug.LogError("[PartySpawner] GameSimulation.Instance is null - cannot queue command!");
                return;
            }
            
            int tickBefore = GameSimulation.Instance.Simulation.World.CurrentTick;
            GameSimulation.Instance.QueueCommand(cmd);
            int tickAfter = GameSimulation.Instance.Simulation.World.CurrentTick;
            
            Debug.Log($"[PartySpawner] Command queued - tick before: {tickBefore}, tick after: {tickAfter}");
            Debug.Log($"[PartySpawner] Command will be processed on next simulation tick");
        }
        
    }
}

