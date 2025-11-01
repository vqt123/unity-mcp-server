using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using ArenaGame.Shared.Math;
using EntityId = ArenaGame.Shared.Entities.EntityId;

namespace ArenaGame.Client
{
    /// <summary>
    /// Creates and manages visual GameObjects for simulation entities
    /// </summary>
    public class EntityVisualizer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject heroPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject projectilePrefab;
        
        private Dictionary<EntityId, GameObject> entityViews = new Dictionary<EntityId, GameObject>();
        private Dictionary<EntityId, GameObject> projectileParticleEmitters = new Dictionary<EntityId, GameObject>(); // Track particle emitters for projectiles
        
        // Public setters for GameBootstrapper
        public void SetPrefabs(GameObject hero, GameObject enemy, GameObject projectile)
        {
            heroPrefab = hero;
            enemyPrefab = enemy;
            projectilePrefab = projectile;
            Debug.Log($"[EntityVisualizer] Prefabs set - Hero:{hero!=null}, Enemy:{enemy!=null}, Proj:{projectile!=null}");
        }
        
        void OnEnable()
        {
            EventBus.Subscribe<HeroSpawnedEvent>(OnEvent);
            EventBus.Subscribe<EnemySpawnedEvent>(OnEvent);
            EventBus.Subscribe<ProjectileSpawnedEvent>(OnEvent);
            EventBus.Subscribe<HeroKilledEvent>(OnEvent);
            EventBus.Subscribe<EnemyKilledEvent>(OnEvent);
            EventBus.Subscribe<ProjectileDestroyedEvent>(OnEvent);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<HeroSpawnedEvent>(OnEvent);
            EventBus.Unsubscribe<EnemySpawnedEvent>(OnEvent);
            EventBus.Subscribe<ProjectileSpawnedEvent>(OnEvent);
            EventBus.Unsubscribe<HeroKilledEvent>(OnEvent);
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEvent);
            EventBus.Unsubscribe<ProjectileDestroyedEvent>(OnEvent);
        }
        
        void LateUpdate()
        {
            // Sync positions from simulation
            if (GameSimulation.Instance != null)
            {
                SyncPositions(GameSimulation.Instance.Simulation.World);
            }
        }
        
        private void OnEvent(ISimulationEvent evt)
        {
            switch (evt)
            {
                case HeroSpawnedEvent heroSpawn:
                    CreateHeroView(heroSpawn);
                    break;
                case EnemySpawnedEvent enemySpawn:
                    CreateEnemyView(enemySpawn);
                    break;
                case ProjectileSpawnedEvent projSpawn:
                    CreateProjectileView(projSpawn);
                    break;
                case HeroKilledEvent heroKill:
                    DestroyEntityView(heroKill.HeroId);
                    break;
                case EnemyKilledEvent enemyKill:
                    DestroyEntityView(enemyKill.EnemyId);
                    break;
                case ProjectileDestroyedEvent projDestroy:
                    DestroyEntityView(projDestroy.ProjectileId);
                    break;
            }
        }
        
        private void CreateHeroView(HeroSpawnedEvent evt)
        {
            if (heroPrefab == null)
            {
                Debug.LogError("[EntityVisualizer] Hero prefab is null!");
                return;
            }
            
            GameObject obj = Instantiate(heroPrefab, ToVector3(evt.Position), Quaternion.identity);
            obj.name = $"Hero_{evt.HeroId.Value}_{evt.HeroType}";
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.HeroId;
            view.IsHero = true;
            
            entityViews[evt.HeroId] = obj;
        }
        
        private void CreateEnemyView(EnemySpawnedEvent evt)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[EntityVisualizer] Enemy prefab is null!");
                return;
            }
            
            GameObject obj = Instantiate(enemyPrefab, ToVector3(evt.Position), Quaternion.identity);
            obj.name = $"Enemy_{evt.EnemyId.Value}_{evt.EnemyType}";
            
            if (evt.IsBoss) obj.transform.localScale *= 2f;
            else if (evt.IsMiniBoss) obj.transform.localScale *= 1.5f;
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.EnemyId;
            view.IsHero = false;
            
            entityViews[evt.EnemyId] = obj;
        }
        
        private void CreateProjectileView(ProjectileSpawnedEvent evt)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[EntityVisualizer] Projectile prefab is null!");
                return;
            }
            
            // Rotate projectile to face its direction
            Vector3 direction = ToVector3(evt.Velocity).normalized;
            Quaternion rotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : Quaternion.identity;
            
            GameObject obj = Instantiate(projectilePrefab, ToVector3(evt.Position), rotation);
            obj.name = $"Projectile_{evt.ProjectileId.Value}";
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.ProjectileId;
            view.IsHero = false;
            
            // Clone ParticleTestObject from scene and parent it to projectile (moves with projectile)
            GameObject templateObject = GameObject.Find("ParticleTestObject");
            if (templateObject != null)
            {
                GameObject particleObj = Instantiate(templateObject, obj.transform.position, obj.transform.rotation);
                particleObj.transform.SetParent(obj.transform); // Parented - emitter moves with projectile
                particleObj.name = $"ParticleEmitter_{evt.ProjectileId.Value}";
                
                // Ensure particle system is in world space so particles stay where emitted
                ParticleSystem particles = particleObj.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var main = particles.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World; // Particles stay in world space
                }
                
                // Track emitter for cleanup when projectile is destroyed
                projectileParticleEmitters[evt.ProjectileId] = particleObj;
            }
            
            entityViews[evt.ProjectileId] = obj;
        }
        
        private string GetHeroTypeFromOwner(EntityId ownerId)
        {
            // Look up hero type from simulation world
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(ownerId, out var hero))
                {
                    return hero.HeroType;
                }
            }
            
            // Fallback: check entity views for hero type
            if (entityViews.TryGetValue(ownerId, out GameObject heroObj))
            {
                // Try to get hero type from GameObject name or component
                if (heroObj.name.StartsWith("Hero_"))
                {
                    // Extract hero type from name like "Hero_123_Mage"
                    var parts = heroObj.name.Split('_');
                    if (parts.Length >= 3)
                    {
                        return parts[2]; // Hero type is third part
                    }
                }
            }
            
            return "DefaultHero"; // Default fallback
        }
        
        private void DestroyEntityView(EntityId id)
        {
            GameObject obj = null;
            if (entityViews.TryGetValue(id, out obj))
            {
                // If this is a projectile, detach particle emitter before destroying projectile
                if (projectileParticleEmitters.TryGetValue(id, out GameObject emitter))
                {
                    // Unparent emitter so it stays in world space after projectile is destroyed
                    emitter.transform.SetParent(null);
                    
                    // Stop emitting new particles
                    ParticleSystem particles = emitter.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        var emission = particles.emission;
                        emission.enabled = false; // Stop emitting, but let existing particles fade
                    }
                    
                    // Get particle lifetime to destroy emitter after all particles fade
                    float maxLifetime = 5f; // Default
                    if (particles != null)
                    {
                        var main = particles.main;
                        maxLifetime = main.startLifetime.constantMax;
                        if (maxLifetime <= 0f) maxLifetime = main.startLifetime.constant;
                        if (maxLifetime <= 0f) maxLifetime = 5f; // Final fallback
                    }
                    
                    // Destroy emitter after all particles have faded
                    Destroy(emitter, maxLifetime + 1f);
                    
                    // Remove from tracking
                    projectileParticleEmitters.Remove(id);
                }
                
                // Now safe to destroy projectile
                Destroy(obj);
                entityViews.Remove(id);
            }
        }
        
        private void SyncPositions(SimulationWorld world)
        {
            foreach (var heroId in world.HeroIds)
            {
                if (world.TryGetHero(heroId, out var hero) && entityViews.TryGetValue(heroId, out GameObject obj))
                {
                    obj.transform.position = ToVector3(hero.Position);
                }
            }
            
            foreach (var enemyId in world.EnemyIds)
            {
                if (world.TryGetEnemy(enemyId, out var enemy) && entityViews.TryGetValue(enemyId, out GameObject obj))
                {
                    obj.transform.position = ToVector3(enemy.Position);
                }
            }
            
            foreach (var projId in world.ProjectileIds)
            {
                if (world.TryGetProjectile(projId, out var proj) && entityViews.TryGetValue(projId, out GameObject obj))
                {
                    obj.transform.position = ToVector3(proj.Position);
                }
            }
        }
        
        private Vector3 ToVector3(FixV2 pos)
        {
            return new Vector3((float)pos.X.ToDouble(), 0f, (float)pos.Y.ToDouble());
        }
    }
}

