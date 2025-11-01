using System.Collections.Generic;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Systems
{
    /// <summary>
    /// Handles combat - auto-shooting, damage, death
    /// </summary>
    public static class CombatSystem
    {
        private const int PROJECTILE_LIFETIME_TICKS = 150; // 5 seconds at 30 tps
        
        /// <summary>
        /// Process all heroes - they auto-shoot at nearest enemy
        /// </summary>
        public static void ProcessHeroShooting(SimulationWorld world)
        {
            // Iterate over deterministic list
            foreach (var heroId in world.HeroIds)
            {
                if (!world.TryGetHero(heroId, out Hero hero)) continue;
                if (!hero.CanShoot(world.CurrentTick)) continue;
                
                // Find nearest enemy
                EntityId nearestEnemy = FindNearestEnemy(world, hero.Position);
                if (!nearestEnemy.IsValid) continue;
                
                if (world.TryGetEnemy(nearestEnemy, out Enemy target))
                {
                    // Calculate direction to enemy
                    FixV2 direction = (target.Position - hero.Position).Normalized;
                    HeroShoot(world, heroId, direction);
                }
            }
        }
        
        private static void HeroShoot(SimulationWorld world, EntityId heroId, FixV2 direction)
        {
            if (!world.TryGetHero(heroId, out Hero hero)) return;
            if (!hero.CanShoot(world.CurrentTick)) return;
            
            // Create projectile
            FixV2 normalizedDir = direction.Normalized;
            Fix64 projectileSpeed = GetProjectileSpeed(hero.WeaponType);
            
            Projectile projectile = new Projectile
            {
                OwnerId = heroId,
                Position = hero.Position,
                Velocity = normalizedDir * projectileSpeed,
                Speed = projectileSpeed,
                Damage = hero.Damage,
                IsActive = true,
                SpawnTick = world.CurrentTick,
                MaxLifetimeTicks = PROJECTILE_LIFETIME_TICKS,
                Piercing = true, // All projectiles pierce through enemies
                AoeRadius = GetAoeRadius(hero.WeaponType)
            };
            
            EntityId projectileId = world.CreateProjectile(projectile);
            
            // Update hero cooldown
            hero.LastShotTick = world.CurrentTick;
            world.UpdateHero(heroId, hero);
            
            // Generate events
            world.AddEvent(new Events.ProjectileSpawnedEvent
            {
                Tick = world.CurrentTick,
                ProjectileId = projectileId,
                OwnerId = heroId,
                Position = hero.Position,
                Velocity = normalizedDir * projectileSpeed,
                Damage = hero.Damage
            });
            
            world.AddEvent(new Events.HeroShootEvent
            {
                Tick = world.CurrentTick,
                HeroId = heroId,
                ProjectileId = projectileId,
                Direction = normalizedDir
            });
        }
        
        public static void ProcessCollisions(SimulationWorld world)
        {
            // Iterate over deterministic lists
            List<EntityId> projectilesToRemove = new List<EntityId>();
            List<EntityId> enemiesToRemove = new List<EntityId>();
            
            foreach (var projId in world.ProjectileIds)
            {
                if (!world.TryGetProjectile(projId, out Projectile proj)) continue;
                if (!proj.IsActive) continue;
                
                bool hitSomething = false;
                
                // Check collision with enemies
                foreach (var enemyId in world.EnemyIds)
                {
                    if (!world.TryGetEnemy(enemyId, out Enemy enemy)) continue;
                    if (!enemy.IsAlive) continue;
                    
                    // Simple circle collision (enemy has implicit radius)
                    Fix64 enemyRadius = Fix64.FromFloat(0.5f);
                    Fix64 dist = FixV2.Distance(proj.Position, enemy.Position);
                    
                    if (dist <= enemyRadius)
                    {
                        // Apply damage
                        Fix64 healthBefore = enemy.Health;
                        enemy.TakeDamage(proj.Damage);
                        world.UpdateEnemy(enemyId, enemy);
                        
                        // Generate damage event
                        world.AddEvent(new Events.EnemyDamagedEvent
                        {
                            Tick = world.CurrentTick,
                            EnemyId = enemyId,
                            AttackerId = proj.OwnerId,
                            Damage = proj.Damage,
                            RemainingHealth = enemy.Health
                        });
                        
                        if (!enemy.IsAlive)
                        {
                            enemiesToRemove.Add(enemyId);
                            
                            // Grant XP to killer
                            if (world.TryGetHero(proj.OwnerId, out Hero killer))
                            {
                                int xpGrant = enemy.IsBoss ? 100 : (enemy.IsMiniBoss ? 50 : 10);
                                killer.GainXP(xpGrant, out bool leveledUp);
                                world.UpdateHero(proj.OwnerId, killer);
                                
                                // Generate XP gained event
                                world.AddEvent(new Events.XPGainedEvent
                                {
                                    Tick = world.CurrentTick,
                                    HeroId = proj.OwnerId,
                                    XPGained = xpGrant,
                                    CurrentXP = killer.CurrentXP,
                                    Level = killer.Level
                                });
                                
                                if (leveledUp)
                                {
                                    world.AddEvent(new Events.HeroLevelUpEvent
                                    {
                                        Tick = world.CurrentTick,
                                        HeroId = proj.OwnerId,
                                        NewLevel = killer.Level
                                    });
                                }
                            }
                            
                            world.AddEvent(new Events.EnemyKilledEvent
                            {
                                Tick = world.CurrentTick,
                                EnemyId = enemyId,
                                KillerId = proj.OwnerId
                            });
                        }
                        
                        hitSomething = true;
                        
                        if (!proj.Piercing)
                            break;
                    }
                }
                
                if (hitSomething && !proj.Piercing)
                {
                    projectilesToRemove.Add(projId);
                }
            }
            
            // Clean up and generate events
            foreach (var id in projectilesToRemove)
            {
                if (world.TryGetProjectile(id, out Projectile p))
                {
                    world.AddEvent(new Events.ProjectileDestroyedEvent
                    {
                        Tick = world.CurrentTick,
                        ProjectileId = id,
                        Position = p.Position,
                        HitTarget = true
                    });
                }
                world.RemoveProjectile(id);
            }
            foreach (var id in enemiesToRemove)
            {
                world.RemoveEnemy(id);
            }
        }
        
        public static void ProcessEnemyAttacks(SimulationWorld world)
        {
            // Iterate over deterministic list
            foreach (var enemyId in world.EnemyIds)
            {
                if (!world.TryGetEnemy(enemyId, out Enemy enemy)) continue;
                if (!enemy.CanAttack(world.CurrentTick)) continue;
                if (!enemy.TargetId.IsValid) continue;
                
                // Check if target is in range
                if (world.TryGetHero(enemy.TargetId, out Hero target))
                {
                    Fix64 dist = FixV2.Distance(enemy.Position, target.Position);
                    
                    if (dist <= enemy.AttackRange)
                    {
                        // Deal damage
                        target.TakeDamage(enemy.Damage);
                        world.UpdateHero(enemy.TargetId, target);
                        
                        // Generate damage event
                        world.AddEvent(new Events.HeroDamagedEvent
                        {
                            Tick = world.CurrentTick,
                            HeroId = enemy.TargetId,
                            AttackerId = enemyId,
                            Damage = enemy.Damage,
                            RemainingHealth = target.Health
                        });
                        
                        // Update enemy cooldown
                        enemy.LastAttackTick = world.CurrentTick;
                        world.UpdateEnemy(enemyId, enemy);
                        
                        if (!target.IsAlive)
                        {
                            world.AddEvent(new Events.HeroKilledEvent
                            {
                                Tick = world.CurrentTick,
                                HeroId = enemy.TargetId,
                                KillerId = enemyId
                            });
                        }
                    }
                }
            }
        }
        
        // Weapon configurations
        private static Fix64 GetProjectileSpeed(string weaponType)
        {
            return Fix64.FromInt(15); // Default for now
        }
        
        private static bool IsPiercing(string weaponType)
        {
            return false; // Default for now
        }
        
        private static Fix64 GetAoeRadius(string weaponType)
        {
            return Fix64.Zero; // Default for now
        }
        
        private static EntityId FindNearestEnemy(SimulationWorld world, FixV2 position)
        {
            EntityId nearest = EntityId.Invalid;
            Fix64 nearestDist = Fix64.MaxValue;
            
            // Iterate over deterministic list
            foreach (var enemyId in world.EnemyIds)
            {
                if (!world.TryGetEnemy(enemyId, out Enemy enemy)) continue;
                if (!enemy.IsAlive) continue;
                
                Fix64 dist = FixV2.SqrDistance(position, enemy.Position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemyId;
                }
            }
            
            return nearest;
        }
    }
}

