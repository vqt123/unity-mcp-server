using System.Collections.Generic;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Systems
{
    /// <summary>
    /// Handles all entity movement - completely deterministic
    /// </summary>
    public static class MovementSystem
    {
        public static void UpdateHeroes(SimulationWorld world)
        {
            foreach (var kvp in world.Heroes)
            {
                Hero hero = kvp.Value;
                if (!hero.IsAlive) continue;
                
                // Apply velocity
                hero.Position += hero.Velocity * world.FixedDeltaTime;
                
                // Clamp to arena bounds
                Fix64 distFromCenter = hero.Position.Magnitude;
                if (distFromCenter > SimulationConfig.ARENA_RADIUS)
                {
                    hero.Position = hero.Position.Normalized * SimulationConfig.ARENA_RADIUS;
                    hero.Velocity = FixV2.Zero; // Stop at edge
                }
                
                world.UpdateHero(kvp.Key, hero);
            }
        }
        
        public static void UpdateEnemies(SimulationWorld world)
        {
            foreach (var kvp in world.Enemies)
            {
                Enemy enemy = kvp.Value;
                if (!enemy.IsAlive) continue;
                
                // Apply velocity
                enemy.Position += enemy.Velocity * world.FixedDeltaTime;
                
                // Clamp to arena bounds
                Fix64 distFromCenter = enemy.Position.Magnitude;
                if (distFromCenter > SimulationConfig.ARENA_RADIUS)
                {
                    enemy.Position = enemy.Position.Normalized * SimulationConfig.ARENA_RADIUS;
                }
                
                world.UpdateEnemy(kvp.Key, enemy);
            }
        }
        
        public static void UpdateProjectiles(SimulationWorld world)
        {
            List<EntityId> toRemove = new List<EntityId>();
            
            foreach (var kvp in world.Projectiles)
            {
                Projectile proj = kvp.Value;
                if (!proj.IsActive) continue;
                
                // Check expiration
                if (proj.IsExpired(world.CurrentTick))
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                
                // Apply velocity
                proj.Position += proj.Velocity * world.FixedDeltaTime;
                
                // Remove if outside arena
                Fix64 distFromCenter = proj.Position.Magnitude;
                if (distFromCenter > SimulationConfig.ARENA_RADIUS)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                
                world.UpdateProjectile(kvp.Key, proj);
            }
            
            // Clean up expired projectiles
            foreach (var id in toRemove)
            {
                if (world.TryGetProjectile(id, out Projectile p))
                {
                    world.AddEvent(new Events.ProjectileDestroyedEvent
                    {
                        Tick = world.CurrentTick,
                        ProjectileId = id,
                        Position = p.Position,
                        HitTarget = false
                    });
                }
                world.RemoveProjectile(id);
            }
        }
        
        public static void SetHeroVelocity(SimulationWorld world, EntityId heroId, FixV2 moveDirection)
        {
            if (world.TryGetHero(heroId, out Hero hero))
            {
                // Normalize and apply move speed
                Fix64 magnitude = moveDirection.Magnitude;
                if (magnitude > Fix64.Zero)
                {
                    FixV2 normalized = moveDirection / magnitude;
                    hero.Velocity = normalized * hero.MoveSpeed;
                }
                else
                {
                    hero.Velocity = FixV2.Zero;
                }
                
                world.UpdateHero(heroId, hero);
            }
        }
    }
}

