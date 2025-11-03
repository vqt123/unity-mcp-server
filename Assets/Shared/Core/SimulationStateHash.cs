using System.Collections.Generic;
using System.Text;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Core
{
    /// <summary>
    /// Simple state hash for detecting replay divergence
    /// Computes a deterministic hash of key simulation state
    /// </summary>
    public static class SimulationStateHash
    {
        /// <summary>
        /// Compute a hash of the current simulation state
        /// Returns a simple hash string (first 16 chars of full hash)
        /// </summary>
        public static string ComputeHash(SimulationWorld world)
        {
            var sb = new StringBuilder();
            
            // Tick number (most important - must match exactly)
            sb.Append($"T{world.CurrentTick}|");
            
            // Heroes (in deterministic order)
            sb.Append($"H{world.HeroIds.Count}:");
            foreach (var heroId in world.HeroIds)
            {
                if (world.TryGetHero(heroId, out Hero hero))
                {
                    // Key state: ID, position, health, level
                    sb.Append($"{heroId.Value}-{hero.Position.X.RawValue}-{hero.Position.Y.RawValue}-{hero.Health.RawValue}-{hero.Level},");
                }
            }
            sb.Append("|");
            
            // Enemies (in deterministic order)
            sb.Append($"E{world.EnemyIds.Count}:");
            foreach (var enemyId in world.EnemyIds)
            {
                if (world.TryGetEnemy(enemyId, out Enemy enemy))
                {
                    // Key state: ID, position, health
                    sb.Append($"{enemyId.Value}-{enemy.Position.X.RawValue}-{enemy.Position.Y.RawValue}-{enemy.Health.RawValue},");
                }
            }
            sb.Append("|");
            
            // Projectiles (in deterministic order)
            sb.Append($"P{world.ProjectileIds.Count}:");
            foreach (var projId in world.ProjectileIds)
            {
                if (world.TryGetProjectile(projId, out Projectile proj))
                {
                    // Key state: ID, position
                    sb.Append($"{projId.Value}-{proj.Position.X.RawValue}-{proj.Position.Y.RawValue},");
                }
            }
            
            // Simple hash: use GetHashCode() and convert to hex string
            string stateString = sb.ToString();
            int hash = stateString.GetHashCode();
            
            // Return as hex string (16 chars max for readability)
            return hash.ToString("X16");
        }
    }
}

