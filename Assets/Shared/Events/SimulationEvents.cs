using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Events
{
    /// <summary>
    /// Base class for all simulation events
    /// </summary>
    public abstract class SimulationEvent : ISimulationEvent
    {
        public int Tick { get; set; }
    }
    
    // === Entity Creation Events ===
    
    public class HeroSpawnedEvent : SimulationEvent
    {
        public EntityId HeroId;
        public string HeroType;
        public FixV2 Position;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 Damage;
        public Fix64 AttackSpeed;
        public string WeaponType;
        public int WeaponTier;
    }
    
    public class EnemySpawnedEvent : SimulationEvent
    {
        public EntityId EnemyId;
        public string EnemyType;
        public FixV2 Position;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 Damage;
        public bool IsBoss;
        public bool IsMiniBoss;
    }
    
    public class ProjectileSpawnedEvent : SimulationEvent
    {
        public EntityId ProjectileId;
        public EntityId OwnerId;
        public FixV2 Position;
        public FixV2 Velocity;
        public Fix64 Damage;
    }
    
    // === Movement Events ===
    
    public class EntityMovedEvent : SimulationEvent
    {
        public EntityId EntityId;
        public FixV2 Position;
        public FixV2 Velocity;
    }
    
    // === Combat Events ===
    
    public class HeroShootEvent : SimulationEvent
    {
        public EntityId HeroId;
        public EntityId ProjectileId;
        public FixV2 Direction;
    }
    
    public class HeroDamagedEvent : SimulationEvent
    {
        public EntityId HeroId;
        public EntityId AttackerId;
        public Fix64 Damage;
        public Fix64 RemainingHealth;
    }
    
    public class EnemyDamagedEvent : SimulationEvent
    {
        public EntityId EnemyId;
        public EntityId AttackerId;
        public Fix64 Damage;
        public Fix64 RemainingHealth;
    }
    
    public class HeroKilledEvent : SimulationEvent
    {
        public EntityId HeroId;
        public EntityId KillerId;
    }
    
    public class EnemyKilledEvent : SimulationEvent
    {
        public EntityId EnemyId;
        public EntityId KillerId;
    }
    
    // === Progression Events ===
    
    public class XPGainedEvent : SimulationEvent
    {
        public EntityId HeroId;
        public int XPGained;
        public int CurrentXP;
        public int Level;
    }
    
    public class HeroLevelUpEvent : SimulationEvent
    {
        public EntityId HeroId;
        public int NewLevel;
    }
    
    // === Entity Destruction Events ===
    
    public class ProjectileDestroyedEvent : SimulationEvent
    {
        public EntityId ProjectileId;
        public FixV2 Position;
        public bool HitTarget;
    }
    
    // === Game State Events ===
    
    public class WaveStartedEvent : SimulationEvent
    {
        public int WaveNumber;
        public int LevelNumber;
    }
    
    public class WaveCompletedEvent : SimulationEvent
    {
        public int WaveNumber;
        public int LevelNumber;
    }
    
    public class LevelCompletedEvent : SimulationEvent
    {
        public int LevelNumber;
    }
    
    public class GameOverEvent : SimulationEvent
    {
        public bool Victory;
    }
}

