using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Core
{
    /// <summary>
    /// Configuration constants for the simulation
    /// </summary>
    public static class SimulationConfig
    {
        // Timing
        public const int TICKS_PER_SECOND = 30;
        public static readonly Fix64 FIXED_DELTA_TIME = Fix64.FromFloat(1f / TICKS_PER_SECOND);
        
        // Arena
        public static readonly Fix64 ARENA_RADIUS = Fix64.FromInt(10);
        public static readonly Fix64 PROJECTILE_MAX_RADIUS = ARENA_RADIUS * Fix64.FromInt(2); // Projectiles can travel twice as far
        
        // Entity limits
        public const int MAX_HEROES = 10;
        public const int MAX_ENEMIES = 100;
        public const int MAX_PROJECTILES = 200;
    }
}

