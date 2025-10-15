using System.Collections.Generic;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Systems
{
    /// <summary>
    /// Handles enemy AI - targeting and movement
    /// </summary>
    public static class AISystem
    {
        public static void UpdateEnemies(SimulationWorld world)
        {
            foreach (var enemyKvp in world.Enemies)
            {
                Enemy enemy = enemyKvp.Value;
                if (!enemy.IsAlive) continue;
                
                // Find or update target
                if (!enemy.TargetId.IsValid || !IsValidTarget(world, enemy.TargetId))
                {
                    enemy.TargetId = FindNearestHero(world, enemy.Position);
                }
                
                // Move towards target
                if (enemy.TargetId.IsValid && world.TryGetHero(enemy.TargetId, out Hero target))
                {
                    FixV2 direction = target.Position - enemy.Position;
                    Fix64 distance = direction.Magnitude;
                    
                    // Stop if in attack range
                    if (distance > enemy.AttackRange)
                    {
                        FixV2 normalized = direction / distance;
                        enemy.Velocity = normalized * enemy.MoveSpeed;
                    }
                    else
                    {
                        enemy.Velocity = FixV2.Zero;
                    }
                }
                else
                {
                    enemy.Velocity = FixV2.Zero;
                }
                
                world.UpdateEnemy(enemyKvp.Key, enemy);
            }
        }
        
        private static bool IsValidTarget(SimulationWorld world, EntityId targetId)
        {
            if (!world.TryGetHero(targetId, out Hero hero))
                return false;
            
            return hero.IsAlive;
        }
        
        private static EntityId FindNearestHero(SimulationWorld world, FixV2 position)
        {
            EntityId nearest = EntityId.Invalid;
            Fix64 nearestDist = Fix64.MaxValue;
            
            foreach (var heroKvp in world.Heroes)
            {
                Hero hero = heroKvp.Value;
                if (!hero.IsAlive) continue;
                
                Fix64 dist = FixV2.SqrDistance(position, hero.Position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = heroKvp.Key;
                }
            }
            
            return nearest;
        }
    }
}

