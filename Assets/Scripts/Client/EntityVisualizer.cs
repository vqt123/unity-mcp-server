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
        
        // Position buffer for interpolation - we need TWO positions:
        // - previousTickPos: position from tick N-1
        // - currentTickPos: position from tick N
        // We interpolate between these two positions throughout tick N
        private struct PositionBuffer
        {
            public Vector3 previousTickPos; // Position from tick N-1
            public Vector3 currentTickPos;  // Position from tick N
            public int previousTick;        // The tick number for previousTickPos
            public int currentTick;         // The tick number for currentTickPos
        }
        
        private Dictionary<EntityId, PositionBuffer> entityPositionBuffers = new Dictionary<EntityId, PositionBuffer>();
        
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
            // Sync positions from simulation with interpolation
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
            
            Vector3 initialPos = ToVector3(evt.Position);
            GameObject obj = Instantiate(heroPrefab, initialPos, Quaternion.identity);
            obj.name = $"Hero_{evt.HeroId.Value}_{evt.HeroType}";
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.HeroId;
            view.IsHero = true;
            
            // Initialize position buffer - both previous and current start at same position
            int currentTick = GameSimulation.Instance != null ? GameSimulation.Instance.Simulation.World.CurrentTick : 0;
            entityPositionBuffers[evt.HeroId] = new PositionBuffer
            {
                previousTickPos = initialPos,
                currentTickPos = initialPos,
                previousTick = currentTick,
                currentTick = currentTick
            };
            
            entityViews[evt.HeroId] = obj;
        }
        
        private void CreateEnemyView(EnemySpawnedEvent evt)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[EntityVisualizer] Enemy prefab is null!");
                return;
            }
            
            Vector3 initialPos = ToVector3(evt.Position);
            GameObject obj = Instantiate(enemyPrefab, initialPos, Quaternion.identity);
            obj.name = $"Enemy_{evt.EnemyId.Value}_{evt.EnemyType}";
            
            if (evt.IsBoss) obj.transform.localScale *= 2f;
            else if (evt.IsMiniBoss) obj.transform.localScale *= 1.5f;
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.EnemyId;
            view.IsHero = false;
            
            // Initialize position buffer
            int currentTick = GameSimulation.Instance != null ? GameSimulation.Instance.Simulation.World.CurrentTick : 0;
            entityPositionBuffers[evt.EnemyId] = new PositionBuffer
            {
                previousTickPos = initialPos,
                currentTickPos = initialPos,
                previousTick = currentTick,
                currentTick = currentTick
            };
            
            entityViews[evt.EnemyId] = obj;
        }
        
        private void CreateProjectileView(ProjectileSpawnedEvent evt)
        {
            // Use projectilePrefab as the base projectile
            if (projectilePrefab == null)
            {
                Debug.LogError("[EntityVisualizer] Projectile prefab is null!");
                return;
            }
            
            // Rotate projectile to face its direction
            Vector3 direction = ToVector3(evt.Velocity).normalized;
            Quaternion rotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : Quaternion.identity;
            
            // Clone projectilePrefab as the base projectile
            GameObject obj = Instantiate(projectilePrefab, ToVector3(evt.Position), rotation);
            obj.name = $"Projectile_{evt.ProjectileId.Value}";
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.ProjectileId;
            view.IsHero = false;
            
            // Initialize position buffer
            Vector3 initialPos = ToVector3(evt.Position);
            int currentTick = GameSimulation.Instance != null ? GameSimulation.Instance.Simulation.World.CurrentTick : 0;
            entityPositionBuffers[evt.ProjectileId] = new PositionBuffer
            {
                previousTickPos = initialPos,
                currentTickPos = initialPos,
                previousTick = currentTick,
                currentTick = currentTick
            };
            
            obj.transform.position = initialPos;
            
            // Clone ProjectileFX from scene and attach it to the projectile for particles
            GameObject projectileFXTemplate = GameObject.Find("ProjectileFX");
            if (projectileFXTemplate != null)
            {
                GameObject projectileFXObj = Instantiate(projectileFXTemplate, obj.transform.position, obj.transform.rotation);
                projectileFXObj.transform.SetParent(obj.transform); // Parented to projectile - moves with it
                projectileFXObj.name = $"ProjectileFX_{evt.ProjectileId.Value}";
                
                // Find and configure particle systems within ProjectileFX
                ParticleSystem[] particleSystems = projectileFXObj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particles in particleSystems)
                {
                    var main = particles.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World; // Particles stay in world space
                }
                
                // Track ProjectileFX for cleanup when projectile is destroyed
                projectileParticleEmitters[evt.ProjectileId] = projectileFXObj;
            }
            else
            {
                Debug.LogWarning($"[EntityVisualizer] ProjectileFX not found in scene for projectile {evt.ProjectileId.Value}!");
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
            // Clean up position buffer
            entityPositionBuffers.Remove(id);
            
            GameObject obj = null;
            if (entityViews.TryGetValue(id, out obj))
            {
                // If this is a projectile, detach particle emitter before destroying projectile
                if (projectileParticleEmitters.TryGetValue(id, out GameObject emitter))
                {
                    // Unparent emitter FIRST so it stays in world space and won't be destroyed with projectile
                    emitter.transform.SetParent(null);
                    
                    // Get particle system
                    ParticleSystem particles = emitter.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        // Stop emitting new particles, but keep existing particles alive
                        particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                        
                        // Ensure particles stay in world space and don't get cleared
                        var main = particles.main;
                        main.simulationSpace = ParticleSystemSimulationSpace.World;
                        
                        // Get particle lifetime to destroy emitter after all particles fade
                        float maxLifetime = 5f; // Default
                        maxLifetime = main.startLifetime.constantMax;
                        if (maxLifetime <= 0f) maxLifetime = main.startLifetime.constant;
                        if (maxLifetime <= 0f) maxLifetime = 5f; // Final fallback
                        
                        // Destroy emitter GameObject after all particles have faded
                        Destroy(emitter, maxLifetime + 1f);
                    }
                    else
                    {
                        // No particle system found, destroy immediately
                        Destroy(emitter);
                    }
                    
                    // Remove from tracking
                    projectileParticleEmitters.Remove(id);
                }
                
                // Now safe to destroy projectile (particles are already unparented)
                Destroy(obj);
                entityViews.Remove(id);
            }
        }
        
        private void SyncPositions(SimulationWorld world)
        {
            // For proper interpolation, we need at least 2 positions: previous tick and current tick
            // The key insight: we render one tick behind the simulation, storing positions as they arrive
            
            int currentTick = world.CurrentTick;
            
            // Calculate interpolation factor based on how far through the current tick we are
            // This tells us where we are between the previous and current tick positions
            float interpolationFactor = 0f;
            if (GameSimulation.Instance != null)
            {
                float tickInterval = GameSimulation.Instance.TickInterval;
                float tickAccumulator = GameSimulation.Instance.TickAccumulator;
                
                // Interpolation factor: 0.0 = at previous tick, 1.0 = at current tick
                interpolationFactor = Mathf.Clamp01(tickAccumulator / tickInterval);
                
            }
            
            // Proper interpolation requires buffering positions correctly:
            // - Stored position = position from previous tick (tick N-1) 
            // - Current simulation position = position from current tick (tick N)
            // - We interpolate between stored (N-1) and current (N)
            // - After interpolation, we update stored to current for next frame
            
            // For proper interpolation, we maintain TWO positions per entity:
            // - previousTickPos: position from tick N-1
            // - currentTickPos: position from tick N
            // When a new tick arrives, previousTickPos moves to what currentTickPos was, and currentTickPos becomes the new position
            
            // Sync and interpolate heroes
            foreach (var heroId in world.HeroIds)
            {
                if (world.TryGetHero(heroId, out var hero) && entityViews.TryGetValue(heroId, out GameObject obj))
                {
                    Vector3 currentSimPos = ToVector3(hero.Position);
                    
                    if (entityPositionBuffers.TryGetValue(heroId, out PositionBuffer buffer))
                    {
                        // Update buffer when tick changes
                        if (currentTick > buffer.currentTick)
                        {
                            // New tick! Move current -> previous, update current
                            buffer.previousTickPos = buffer.currentTickPos;
                            buffer.previousTick = buffer.currentTick;
                            buffer.currentTickPos = currentSimPos;
                            buffer.currentTick = currentTick;
                        }
                        else if (currentTick == buffer.currentTick)
                        {
                            // Same tick, just update current position (in case simulation updated)
                            buffer.currentTickPos = currentSimPos;
                        }
                        
                        // Interpolate between previous and current
                        Vector3 interpolatedPos = Vector3.Lerp(buffer.previousTickPos, buffer.currentTickPos, interpolationFactor);
                        obj.transform.position = interpolatedPos;
                        
                        entityPositionBuffers[heroId] = buffer;
                    }
                    else
                    {
                        obj.transform.position = currentSimPos;
                        entityPositionBuffers[heroId] = new PositionBuffer
                        {
                            previousTickPos = currentSimPos,
                            currentTickPos = currentSimPos,
                            previousTick = currentTick,
                            currentTick = currentTick
                        };
                    }
                }
            }
            
            // Sync and interpolate enemies
            foreach (var enemyId in world.EnemyIds)
            {
                if (world.TryGetEnemy(enemyId, out var enemy) && entityViews.TryGetValue(enemyId, out GameObject obj))
                {
                    Vector3 currentSimPos = ToVector3(enemy.Position);
                    
                    if (entityPositionBuffers.TryGetValue(enemyId, out PositionBuffer buffer))
                    {
                        if (currentTick > buffer.currentTick)
                        {
                            buffer.previousTickPos = buffer.currentTickPos;
                            buffer.previousTick = buffer.currentTick;
                            buffer.currentTickPos = currentSimPos;
                            buffer.currentTick = currentTick;
                        }
                        else if (currentTick == buffer.currentTick)
                        {
                            buffer.currentTickPos = currentSimPos;
                        }
                        
                        Vector3 interpolatedPos = Vector3.Lerp(buffer.previousTickPos, buffer.currentTickPos, interpolationFactor);
                        obj.transform.position = interpolatedPos;
                        
                        entityPositionBuffers[enemyId] = buffer;
                    }
                    else
                    {
                        obj.transform.position = currentSimPos;
                        entityPositionBuffers[enemyId] = new PositionBuffer
                        {
                            previousTickPos = currentSimPos,
                            currentTickPos = currentSimPos,
                            previousTick = currentTick,
                            currentTick = currentTick
                        };
                    }
                }
            }
            
            // Sync and interpolate projectiles
            foreach (var projId in world.ProjectileIds)
            {
                if (world.TryGetProjectile(projId, out var proj) && entityViews.TryGetValue(projId, out GameObject obj))
                {
                    Vector3 currentSimPos = ToVector3(proj.Position);
                    
                    if (entityPositionBuffers.TryGetValue(projId, out PositionBuffer buffer))
                    {
                        bool tickChanged = currentTick > buffer.currentTick;
                        
                        // Update buffer when tick changes
                        if (tickChanged)
                        {
                            // New tick! Move current -> previous, update current
                            buffer.previousTickPos = buffer.currentTickPos;
                            buffer.previousTick = buffer.currentTick;
                            buffer.currentTickPos = currentSimPos;
                            buffer.currentTick = currentTick;
                            
                        }
                        else if (currentTick == buffer.currentTick)
                        {
                            // Same tick, update current position
                            buffer.currentTickPos = currentSimPos;
                        }
                        
                        // Interpolate between previous (N-1) and current (N)
                        Vector3 interpolatedPos = Vector3.Lerp(buffer.previousTickPos, buffer.currentTickPos, interpolationFactor);
                        obj.transform.position = interpolatedPos;
                        
                        entityPositionBuffers[projId] = buffer;
                    }
                    else
                    {
                        obj.transform.position = currentSimPos;
                        entityPositionBuffers[projId] = new PositionBuffer
                        {
                            previousTickPos = currentSimPos,
                            currentTickPos = currentSimPos,
                            previousTick = currentTick,
                            currentTick = currentTick
                        };
                    }
                }
            }
        }
        
        private Vector3 ToVector3(FixV2 pos)
        {
            return new Vector3((float)pos.X.ToDouble(), 0f, (float)pos.Y.ToDouble());
        }
    }
}

