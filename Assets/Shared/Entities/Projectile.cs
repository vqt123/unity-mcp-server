using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Entities
{
    /// <summary>
    /// Projectile entity - bullets, spells, etc.
    /// </summary>
    public struct Projectile
    {
        public EntityId Id;
        public EntityId OwnerId; // Who fired this
        
        // Transform
        public FixV2 Position;
        public FixV2 Velocity;
        public Fix64 Speed;
        
        // Properties
        public Fix64 Damage;
        public bool IsActive;
        public int SpawnTick;
        public int MaxLifetimeTicks;
        
        // Special
        public bool Piercing;
        public Fix64 AoeRadius;
        
        public bool IsExpired(int currentTick)
        {
            return (currentTick - SpawnTick) >= MaxLifetimeTicks;
        }
    }
}

