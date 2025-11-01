using UnityEngine;
using System.Collections.Generic;
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
            
            GameObject obj = Instantiate(projectilePrefab, ToVector3(evt.Position), Quaternion.identity);
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
            // Create separate GameObject for particle system (can persist after projectile is destroyed)
            GameObject particleObj = new GameObject($"ParticleTrail_{evt.ProjectileId.Value}");
            particleObj.transform.SetParent(projectile.transform);
            particleObj.transform.localPosition = Vector3.zero;
            particleObj.transform.localRotation = Quaternion.identity;
            
            // Add ParticleSystem component to separate object
            ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
            
            // Apply configurable settings
            Vector3 direction = ToVector3(evt.Velocity).normalized;
            if (direction != Vector3.zero)
            {
                particleObj.transform.rotation = Quaternion.LookRotation(direction);
            }
            
            particleSettings.ApplyToParticleSystem(particles, direction);
            
            // Store particle system reference so we can detach it when projectile is destroyed
            projectileParticleSystems[evt.ProjectileId] = particles;
        }
        
        private void DestroyEntityView(EntityId id)
        {
            GameObject obj = null;
            if (entityViews.TryGetValue(id, out obj))
            {
                // If this is a projectile, detach particle system BEFORE destroying projectile
                if (projectileParticleSystems.TryGetValue(id, out ParticleSystem particles))
                {
                    DetachParticleSystem(particles, id);
                    projectileParticleSystems.Remove(id);
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
            particleObj.transform.SetParent(null);
            
            // Stop emitting new particles
            var emission = particles.emission;
            emission.enabled = false;
            
            // Rename
            particleObj.name = $"ParticleTrail_{projectileId.Value}_Fading";
            
            // Destroy after particles have completely faded
            ParticleSystem.MainModule main = particles.main;
            float maxLifetime = Mathf.Max(particleSettings.lifetimeMin, particleSettings.lifetimeMax);
            Destroy(particleObj, maxLifetime + 1f);
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

