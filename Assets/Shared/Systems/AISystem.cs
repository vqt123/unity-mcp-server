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
            // Iterate over deterministic list
            foreach (var enemyId in world.EnemyIds)
            {
                if (!world.TryGetEnemy(enemyId, out Enemy enemy)) continue;
                if (!enemy.IsAlive) continue;
                
                Core.GameLogger.Log($"[AISystem] Tick {world.CurrentTick} - Processing Enemy {enemyId.Value} at ({enemy.Position.X.ToInt()},{enemy.Position.Y.ToInt()})");
                
                // Find or update target
                if (!enemy.TargetId.IsValid || !IsValidTarget(world, enemy.TargetId))
                {
                    enemy.TargetId = FindNearestHero(world, enemy.Position);
                    Core.GameLogger.Log($"[AISystem] Enemy {enemyId.Value} found target: Hero {enemy.TargetId.Value}");
                }
                
                // Move towards target
                if (enemy.TargetId.IsValid && world.TryGetHero(enemy.TargetId, out Hero target))
                {
                    FixV2 direction = target.Position - enemy.Position;
                    Fix64 distance = direction.Magnitude;
                    
                    Core.GameLogger.Log($"[AISystem] Enemy {enemyId.Value} -> Hero {enemy.TargetId.Value}: distance={distance.ToFloat():F2}, attackRange={enemy.AttackRange.ToFloat():F2}");
                    
                    // Stop if in attack range
                    if (distance > enemy.AttackRange)
                    {
                        FixV2 normalized = direction / distance;
                        enemy.Velocity = normalized * enemy.MoveSpeed;
                        Core.GameLogger.Log($"[AISystem] Enemy {enemyId.Value} moving with velocity ({enemy.Velocity.X.ToFloat():F2}, {enemy.Velocity.Y.ToFloat():F2})");
                    }
                    else
                    {
                        enemy.Velocity = FixV2.Zero;
                        Core.GameLogger.Log($"[AISystem] Enemy {enemyId.Value} in attack range - stopping");
                    }
                }
                else
                {
                    enemy.Velocity = FixV2.Zero;
                    Core.GameLogger.Log($"[AISystem] Enemy {enemyId.Value} has no valid target - stopping");
                }
                
                world.UpdateEnemy(enemyId, enemy);
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
            
            // Iterate over deterministic list
            foreach (var heroId in world.HeroIds)
            {
                if (!world.TryGetHero(heroId, out Hero hero)) continue;
                if (!hero.IsAlive) continue;
                
                Fix64 dist = FixV2.SqrDistance(position, hero.Position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = heroId;
                }
            }
            
            return nearest;
        }
    }
}

