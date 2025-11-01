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
        
        [Header("Particle Settings")]
        [Tooltip("Configure projectile particle trail effects. Edit in Inspector to tune appearance.")]
        [SerializeField] private ProjectileParticleSettings particleSettings = new ProjectileParticleSettings();
        
        private Dictionary<EntityId, GameObject> entityViews = new Dictionary<EntityId, GameObject>();
        private Dictionary<EntityId, ParticleSystem> projectileParticleSystems = new Dictionary<EntityId, ParticleSystem>();
        private Dictionary<EntityId, GameObject> projectileParticleGameObjects = new Dictionary<EntityId, GameObject>(); // For orbiting emitters
        private Dictionary<EntityId, float> emitterStartTimes = new Dictionary<EntityId, float>(); // Track when emitter started for orbit angle
        private Dictionary<EntityId, Vector3> projectileLastPositions = new Dictionary<EntityId, Vector3>(); // Store last position when projectile destroyed
        private Dictionary<EntityId, Quaternion> projectileLastRotations = new Dictionary<EntityId, Quaternion>(); // Store last rotation when projectile destroyed
        
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
                RotateParticleEmitters(); // Rotate emitters for candy cane swirl
            }
        }
        
        private void RotateParticleEmitters()
        {
            // Emitter orbits in a circle around the projectile
            // 10 rotations per second = 3600 degrees per second
            float rotationSpeed = 3600f; // Degrees per second - 10 rotations per second
            float orbitRadius = 0.45f; // Increased by 50% (was 0.3f)
            
            foreach (var kvp in projectileParticleGameObjects.ToList())
            {
                if (kvp.Value == null || !emitterStartTimes.TryGetValue(kvp.Key, out float startTime)) continue;
                
                // CRITICAL: Ensure particle system stays active and emitting for full projectile lifetime
                if (projectileParticleSystems.TryGetValue(kvp.Key, out ParticleSystem particles) && particles != null)
                {
                    // Ensure GameObject is active
                    if (!kvp.Value.activeSelf)
                    {
                        kvp.Value.SetActive(true);
                    }
                    
                    // Ensure particle system is playing and emission is enabled
                    if (!particles.isPlaying)
                    {
                        particles.Play(true);
                    }
                    
                    var emission = particles.emission;
                    if (!emission.enabled)
                    {
                        emission.enabled = true;
                    }
                }
                
                // Try to get projectile - if it still exists, orbit around it
                // If projectile destroyed, keep orbiting around last known position for particles to fade
                Vector3 projectilePosition;
                Vector3 forward;
                Vector3 right;
                Vector3 up;
                
                if (entityViews.TryGetValue(kvp.Key, out GameObject projectile) && projectile != null)
                {
                    // Projectile still exists - orbit around it
                    projectilePosition = projectile.transform.position;
                    forward = projectile.transform.forward;
                    right = projectile.transform.right;
                    up = projectile.transform.up;
                    
                    // Update last known position/rotation
                    projectileLastPositions[kvp.Key] = projectilePosition;
                    projectileLastRotations[kvp.Key] = projectile.transform.rotation;
                }
                else if (projectileLastPositions.TryGetValue(kvp.Key, out Vector3 lastPos) 
                    && projectileLastRotations.TryGetValue(kvp.Key, out Quaternion lastRot))
                {
                    // Projectile destroyed but particles still fading - orbit around last position
                    projectilePosition = lastPos;
                    forward = lastRot * Vector3.forward;
                    right = lastRot * Vector3.right;
                    up = lastRot * Vector3.up;
                }
                else
                {
                    // No projectile and no stored position - skip
                    continue;
                }
                
                // Current angle based on time since emitter started
                float timeSinceStart = Time.time - startTime;
                float currentAngle = (timeSinceStart * rotationSpeed) % 360f;
                float angleRad = currentAngle * Mathf.Deg2Rad;
                
                // Position emitter in circle around projectile (or last position)
                Vector3 orbitPosition = projectilePosition + 
                    (right * Mathf.Cos(angleRad) + up * Mathf.Sin(angleRad)) * orbitRadius;
                
                kvp.Value.transform.position = orbitPosition;
                // Only ONE thing rotates: the emitter orbits (no rotation of emitter itself)
                kvp.Value.transform.rotation = Quaternion.LookRotation(forward, up);
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
            
            // Rotate projectile to face its direction for emitter orbit
            Vector3 direction = ToVector3(evt.Velocity).normalized;
            Quaternion rotation = direction != Vector3.zero ? Quaternion.LookRotation(direction) : Quaternion.identity;
            
            GameObject obj = Instantiate(projectilePrefab, ToVector3(evt.Position), rotation);
            obj.name = $"Projectile_{evt.ProjectileId.Value}";
            
            // Store entity ID for reference
            var view = obj.AddComponent<EntityView>();
            view.EntityId = evt.ProjectileId;
            view.IsHero = false;
            
            // Add particle system for projectile effects
            AddProjectileParticles(obj, evt);
            
            entityViews[evt.ProjectileId] = obj;
        }
        
        private void AddProjectileParticles(GameObject projectile, ProjectileSpawnedEvent evt)
        {
            // Create SEPARATE GameObject for particle emitter (orbits around projectile)
            // Don't parent it - it orbits independently
            GameObject particleObj = new GameObject($"ParticleEmitter_{evt.ProjectileId.Value}");
            particleObj.transform.SetParent(null); // Separate GameObject, not parented
            particleObj.transform.position = projectile.transform.position;
            
            // For candy cane swirl: emitter orbits in a giant circle around projectile
            // Set up initial rotation - align forward with projectile direction
            Vector3 direction = ToVector3(evt.Velocity).normalized;
            if (direction != Vector3.zero)
            {
                particleObj.transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // Add ParticleSystem component to separate object
            ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
            
            // Ensure GameObject stays active
            particleObj.SetActive(true);
            
            // Get hero type from owner to determine particle effect
            string heroType = GetHeroTypeFromOwner(evt.OwnerId);
            ParticleEffectType effectType = ProjectileParticleSettings.GetEffectTypeForHero(heroType);
            
            // Stop particle system before applying settings (in case it auto-started)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            // Apply hero-specific particle preset (configures all settings)
            ProjectileParticleSettings.ApplyPreset(particles, effectType, direction);
            
            // CRITICAL: Ensure emission is enabled and particle system is playing
            var emission = particles.emission;
            emission.enabled = true; // Explicitly enable emission
            
            // Now start playing after all settings are configured
            particles.Play(true); // Start emitting continuously (withChildren = true)
            
            // Verify it's actually playing
            if (!particles.isPlaying)
            {
                Debug.LogWarning($"[EntityVisualizer] Particle system for projectile {evt.ProjectileId.Value} failed to start playing!");
            }
            
            // Store particle system reference and GameObject for orbiting
            projectileParticleSystems[evt.ProjectileId] = particles;
            projectileParticleGameObjects[evt.ProjectileId] = particleObj; // Store for orbiting
            emitterStartTimes[evt.ProjectileId] = Time.time; // Track start time for orbit angle
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
                // If this is a projectile, detach particle system BEFORE destroying projectile
                // Store last position/rotation for orbiting after projectile destroyed
                if (projectileParticleSystems.TryGetValue(id, out ParticleSystem particles))
                {
                    // Store last position and rotation before destroying projectile
                    if (entityViews.TryGetValue(id, out GameObject proj))
                    {
                        projectileLastPositions[id] = proj.transform.position;
                        projectileLastRotations[id] = proj.transform.rotation;
                    }
                    
                    DetachParticleSystem(particles, id);
                    projectileParticleSystems.Remove(id); // Remove particle system reference
                    // DON'T remove emitter GameObject or start time yet - keep orbiting until particles fade
                }
                
                // Now safe to destroy projectile
                Destroy(obj);
                entityViews.Remove(id);
            }
        }
        
        private void DetachParticleSystem(ParticleSystem particles, EntityId projectileId)
        {
            if (particles == null || particles.gameObject == null) return;
            
            GameObject particleObj = particles.gameObject;
            
            // CRITICAL: Unparent FIRST (before projectile is destroyed)
            // But keep emitter in dictionaries so it can keep orbiting for particles to fade
            particleObj.transform.SetParent(null);
            
            // Stop emitting new particles
            var emission = particles.emission;
            emission.enabled = false;
            
            // Rename
            particleObj.name = $"ParticleTrail_{projectileId.Value}_Fading";
            
            // Destroy after particles have completely faded
            // Keep orbiting until particles fade
            ParticleSystem.MainModule main = particles.main;
            float maxLifetime = Mathf.Max(particleSettings.lifetimeMin, particleSettings.lifetimeMax);
            Destroy(particleObj, maxLifetime + 1f);
            
            // Clean up dictionaries after particles fade (but keep orbiting until then)
            StartCoroutine(CleanupParticleEmitterAfterDelay(projectileId, maxLifetime + 1f));
        }
        
        private System.Collections.IEnumerator CleanupParticleEmitterAfterDelay(EntityId projectileId, float delay)
        {
            yield return new WaitForSeconds(delay);
            projectileParticleGameObjects.Remove(projectileId);
            emitterStartTimes.Remove(projectileId);
            projectileLastPositions.Remove(projectileId);
            projectileLastRotations.Remove(projectileId);
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

