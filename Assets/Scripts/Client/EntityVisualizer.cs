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
        
        [Header("Projectile FX Prefabs")]
        [Tooltip("Default projectile FX - used for Bow, Sword, and other default weapons")]
        [SerializeField] private GameObject projectileFXDefault;
        [Tooltip("Fireball FX - used for Firewand weapon")]
        [SerializeField] private GameObject projectileFXFireball;
        
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
        
        /// <summary>
        /// Sets the ProjectileFX prefab references
        /// </summary>
        public void SetProjectileFXPrefabs(GameObject defaultFX, GameObject fireballFX)
        {
            projectileFXDefault = defaultFX;
            projectileFXFireball = fireballFX;
            Debug.Log($"[EntityVisualizer] ProjectileFX prefabs set - Default:{defaultFX!=null}, Fireball:{fireballFX!=null}");
        }
        
        /// <summary>
        /// Gets the visual position of an entity by ID (for damage numbers, etc.)
        /// Returns world position if found, otherwise Vector3.zero
        /// </summary>
        public Vector3 GetEntityPosition(EntityId id)
        {
            // Try to get from visual GameObject first (most accurate)
            if (entityViews.TryGetValue(id, out GameObject obj) && obj != null)
            {
                return obj.transform.position;
            }
            
            // Try to get from position buffer (last known position)
            if (entityPositionBuffers.TryGetValue(id, out PositionBuffer buffer))
            {
                return buffer.currentTickPos;
            }
            
            return Vector3.zero;
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
            // Get weapon type from owner
            string weaponType = GetWeaponTypeFromOwner(evt.OwnerId);
            
            // Try to get projectile prefab from weapon config, fallback to default
            GameObject projectileToUse = GetProjectilePrefabForWeapon(weaponType);
            if (projectileToUse == null)
            {
                projectileToUse = projectilePrefab;
            }
            
            // Fallback: try to load from Resources if still null
            if (projectileToUse == null)
            {
                projectileToUse = Resources.Load<GameObject>("Projectile");
                if (projectileToUse != null)
                {
                    Debug.Log("[EntityVisualizer] Loaded Projectile prefab from Resources");
                }
            }
            
            if (projectileToUse == null)
            {
                Debug.LogError("[EntityVisualizer] Projectile prefab is null! Make sure to assign it in GameBootstrapper or place 'Projectile.prefab' in Assets/Resources/");
                return;
            }
            
            // Rotate projectile to face its direction
            Vector3 direction = ToVector3(evt.Velocity).normalized;
            Quaternion rotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : Quaternion.identity;
            
            // Clone projectile prefab
            GameObject obj = Instantiate(projectileToUse, ToVector3(evt.Position), rotation);
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
            
            // Get ProjectileFX from weapon config or fallback to weapon-based lookup
            GameObject projectileFXTemplate = GetProjectileFXForWeapon(weaponType);
            
            // Clone ProjectileFX from scene and attach it to the projectile for particles
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
                Debug.LogWarning($"[EntityVisualizer] ProjectileFX not found for weapon type '{weaponType}' (projectile {evt.ProjectileId.Value})!");
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
        
        private string GetWeaponTypeFromOwner(EntityId ownerId)
        {
            // Look up weapon type from simulation world
            if (GameSimulation.Instance != null)
            {
                var world = GameSimulation.Instance.Simulation.World;
                if (world.TryGetHero(ownerId, out var hero))
                {
                    return hero.WeaponType;
                }
            }
            
            return "Bow"; // Default fallback
        }
        
        private GameObject GetProjectilePrefabForWeapon(string weaponType)
        {
            // Try to get from weapon config first
            if (WeaponConfigDatabase.Instance != null)
            {
                WeaponConfig config = WeaponConfigDatabase.Instance.GetWeaponConfig(weaponType);
                if (config != null && config.projectilePrefab != null)
                {
                    return config.projectilePrefab;
                }
            }
            
            // Fallback to default
            return null;
        }
        
        private GameObject GetProjectileFXForWeapon(string weaponType)
        {
            // Try to get from weapon config first
            if (WeaponConfigDatabase.Instance != null)
            {
                WeaponConfig config = WeaponConfigDatabase.Instance.GetWeaponConfig(weaponType);
                if (config != null && config.projectileFXPrefab != null)
                {
                    return config.projectileFXPrefab;
                }
            }
            
            // Fallback to old system: Map weapon types to ProjectileFX prefabs
            switch (weaponType)
            {
                case "Firewand":
                    if (projectileFXFireball != null)
                    {
                        return projectileFXFireball;
                    }
                    Debug.LogWarning($"[EntityVisualizer] ProjectileFXFireball prefab not assigned, falling back to default for weapon '{weaponType}'");
                    break;
                case "Bow":
                case "Sword":
                default:
                    // Use default FX
                    break;
            }
            
            // Fallback to default ProjectileFX
            if (projectileFXDefault != null)
            {
                return projectileFXDefault;
            }
            
            // Fallback: try to load from Resources
            GameObject fxFromResources = Resources.Load<GameObject>("ProjectileFX");
            if (fxFromResources != null)
            {
                return fxFromResources;
            }
            
            // Last resort: try to find in scene (backward compatibility)
            GameObject fx = GameObject.Find("ProjectileFX");
            if (fx != null)
            {
                Debug.LogWarning($"[EntityVisualizer] ProjectileFX prefabs not assigned and found '{fx.name}' in scene (consider assigning prefabs in weapon config or GameBootstrapper)");
            }
            else
            {
                Debug.LogError($"[EntityVisualizer] No ProjectileFX prefab assigned and none found in scene for weapon '{weaponType}'!");
            }
            
            return fx;
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
                    // CRITICAL: Unparent emitter FIRST so it stays in world space and won't be destroyed with projectile
                    emitter.transform.SetParent(null);
                    
                    // Find ALL particle systems within ProjectileFX (they're in children like ParticleEmitter)
                    ParticleSystem[] particleSystems = emitter.GetComponentsInChildren<ParticleSystem>();
                    
                    if (particleSystems != null && particleSystems.Length > 0)
                    {
                        float maxLifetime = 5f; // Default fallback
                        
                        // Stop all particle systems and ensure they're in world space
                        foreach (ParticleSystem particles in particleSystems)
                    {
                        // Stop emitting new particles, but keep existing particles alive
                        particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                        
                            // Ensure particles stay in world space
                        var main = particles.main;
                        main.simulationSpace = ParticleSystemSimulationSpace.World;
                        
                            // Track maximum lifetime across all systems
                            float lifetime = main.startLifetime.constantMax;
                            if (lifetime <= 0f) lifetime = main.startLifetime.constant;
                            if (lifetime > maxLifetime) maxLifetime = lifetime;
                        }
                        
                        // Destroy emitter GameObject after all particles have faded
                        Destroy(emitter, maxLifetime + 1f);
                    }
                    else
                    {
                        // No particle systems found, destroy immediately
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

