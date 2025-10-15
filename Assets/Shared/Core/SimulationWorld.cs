using System.Collections.Generic;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Core
{
    /// <summary>
    /// The main simulation state - completely deterministic
    /// </summary>
    public class SimulationWorld
    {
        // Simulation time
        public int CurrentTick { get; private set; }
        public Fix64 FixedDeltaTime => SimulationConfig.FIXED_DELTA_TIME;
        
        // Entity storage
        private Dictionary<EntityId, Hero> heroes = new Dictionary<EntityId, Hero>();
        private Dictionary<EntityId, Enemy> enemies = new Dictionary<EntityId, Enemy>();
        private Dictionary<EntityId, Projectile> projectiles = new Dictionary<EntityId, Projectile>();
        
        // Entity ID generation
        private int nextEntityId = 1;
        
        // Event buffer for this tick
        private List<ISimulationEvent> eventBuffer = new List<ISimulationEvent>();
        
        public SimulationWorld()
        {
            CurrentTick = 0;
        }
        
        // Entity management
        public EntityId CreateHero(Hero hero)
        {
            EntityId id = new EntityId(nextEntityId++);
            hero.Id = id;
            heroes[id] = hero;
            return id;
        }
        
        public EntityId CreateEnemy(Enemy enemy)
        {
            EntityId id = new EntityId(nextEntityId++);
            enemy.Id = id;
            enemies[id] = enemy;
            return id;
        }
        
        public EntityId CreateProjectile(Projectile projectile)
        {
            EntityId id = new EntityId(nextEntityId++);
            projectile.Id = id;
            projectiles[id] = projectile;
            return id;
        }
        
        public bool TryGetHero(EntityId id, out Hero hero)
        {
            return heroes.TryGetValue(id, out hero);
        }
        
        public bool TryGetEnemy(EntityId id, out Enemy enemy)
        {
            return enemies.TryGetValue(id, out enemy);
        }
        
        public bool TryGetProjectile(EntityId id, out Projectile projectile)
        {
            return projectiles.TryGetValue(id, out projectile);
        }
        
        public void UpdateHero(EntityId id, Hero hero)
        {
            heroes[id] = hero;
        }
        
        public void UpdateEnemy(EntityId id, Enemy enemy)
        {
            enemies[id] = enemy;
        }
        
        public void UpdateProjectile(EntityId id, Projectile projectile)
        {
            projectiles[id] = projectile;
        }
        
        public void RemoveHero(EntityId id)
        {
            heroes.Remove(id);
        }
        
        public void RemoveEnemy(EntityId id)
        {
            enemies.Remove(id);
        }
        
        public void RemoveProjectile(EntityId id)
        {
            projectiles.Remove(id);
        }
        
        // Collections
        public IReadOnlyDictionary<EntityId, Hero> Heroes => heroes;
        public IReadOnlyDictionary<EntityId, Enemy> Enemies => enemies;
        public IReadOnlyDictionary<EntityId, Projectile> Projectiles => projectiles;
        
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
            eventBuffer.Clear();
            
            // Systems will be called here in Phase 2
            // For now, this is the structure
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

