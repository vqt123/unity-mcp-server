using System.Collections.Generic;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Core
{
    /// <summary>
    /// The main simulation state - completely deterministic
    /// Uses separate data structures for fast lookups and deterministic iteration
    /// </summary>
    public class SimulationWorld
    {
        // Simulation time
        public int CurrentTick { get; private set; }
        public Fix64 FixedDeltaTime => SimulationConfig.FIXED_DELTA_TIME;
        
        // Fast O(1) lookups
        private Dictionary<EntityId, Hero> heroLookup = new Dictionary<EntityId, Hero>();
        private Dictionary<EntityId, Enemy> enemyLookup = new Dictionary<EntityId, Enemy>();
        private Dictionary<EntityId, Projectile> projectileLookup = new Dictionary<EntityId, Projectile>();
        
        // Deterministic iteration (maintains insertion order)
        private List<EntityId> heroList = new List<EntityId>();
        private List<EntityId> enemyList = new List<EntityId>();
        private List<EntityId> projectileList = new List<EntityId>();
        
        // Public read-only access to lists for iteration
        public IReadOnlyList<EntityId> HeroIds => heroList;
        public IReadOnlyList<EntityId> EnemyIds => enemyList;
        public IReadOnlyList<EntityId> ProjectileIds => projectileList;
        
        // Entity ID generation
        private int nextEntityId = 1;
        
        // Event buffer for this tick
        private List<ISimulationEvent> eventBuffer = new List<ISimulationEvent>();
        
        public SimulationWorld()
        {
            CurrentTick = 0;
        }
        
        // Hero management
        public EntityId CreateHero(Hero hero)
        {
            EntityId id = new EntityId(nextEntityId++);
            hero.Id = id;
            heroLookup[id] = hero;
            heroList.Add(id);
            return id;
        }
        
        public bool TryGetHero(EntityId id, out Hero hero)
        {
            return heroLookup.TryGetValue(id, out hero);
        }
        
        public void UpdateHero(EntityId id, Hero hero)
        {
            if (heroLookup.ContainsKey(id))
            {
                heroLookup[id] = hero;
            }
        }
        
        public void RemoveHero(EntityId id)
        {
            heroLookup.Remove(id);
            heroList.Remove(id);
        }
        
        // Enemy management
        public EntityId CreateEnemy(Enemy enemy)
        {
            EntityId id = new EntityId(nextEntityId++);
            enemy.Id = id;
            enemyLookup[id] = enemy;
            enemyList.Add(id);
            return id;
        }
        
        public bool TryGetEnemy(EntityId id, out Enemy enemy)
        {
            return enemyLookup.TryGetValue(id, out enemy);
        }
        
        public void UpdateEnemy(EntityId id, Enemy enemy)
        {
            if (enemyLookup.ContainsKey(id))
            {
                enemyLookup[id] = enemy;
            }
        }
        
        public void RemoveEnemy(EntityId id)
        {
            enemyLookup.Remove(id);
            enemyList.Remove(id);
        }
        
        // Projectile management
        public EntityId CreateProjectile(Projectile projectile)
        {
            EntityId id = new EntityId(nextEntityId++);
            projectile.Id = id;
            projectileLookup[id] = projectile;
            projectileList.Add(id);
            return id;
        }
        
        public bool TryGetProjectile(EntityId id, out Projectile projectile)
        {
            return projectileLookup.TryGetValue(id, out projectile);
        }
        
        public void UpdateProjectile(EntityId id, Projectile projectile)
        {
            if (projectileLookup.ContainsKey(id))
            {
                projectileLookup[id] = projectile;
            }
        }
        
        public void RemoveProjectile(EntityId id)
        {
            projectileLookup.Remove(id);
            projectileList.Remove(id);
        }
        
        // Event management
        public void AddEvent(ISimulationEvent evt)
        {
            eventBuffer.Add(evt);
        }
        
        public List<ISimulationEvent> GetAndClearEvents()
        {
            List<ISimulationEvent> events = new List<ISimulationEvent>(eventBuffer);
            eventBuffer.Clear();
            return events;
        }
        
        // Main simulation tick
        public void Tick()
        {
            CurrentTick++;
        }
    }
    
    /// <summary>
    /// Base interface for all simulation events
    /// </summary>
    public interface ISimulationEvent
    {
        int Tick { get; }
    }
}
