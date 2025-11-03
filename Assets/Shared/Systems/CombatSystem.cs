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
            
            // Calculate projectile count and modifiers based on hero type and stars
            int projectileCount = GetProjectileCount(hero);
            Fix64 damage = GetProjectileDamage(hero);
            Fix64 projectileSpeed = GetProjectileSpeed(hero.WeaponType);
            bool isPiercing = IsPiercing(hero.WeaponType, hero.Stars);
            
            FixV2 normalizedDir = direction.Normalized;
            
            // Create projectiles based on star level
            for (int i = 0; i < projectileCount; i++)
            {
                // Calculate angle offset for this projectile (spread them)
                FixV2 projectileDir = normalizedDir;
                if (projectileCount > 1)
                {
                    // Spread angle: 15 degrees per projectile
                    // Convert degrees to radians and compute rotation
                    float angleDegrees = (i - (projectileCount - 1) / 2f) * 15f;
                    double angleRadians = angleDegrees * System.Math.PI / 180.0;
                    
                    // Use System.Math for trig (deterministic enough for spread calculation)
                    double cos = System.Math.Cos(angleRadians);
                    double sin = System.Math.Sin(angleRadians);
                    
                    // Rotate direction by angle offset
                    double dirX = normalizedDir.X.ToDouble();
                    double dirY = normalizedDir.Y.ToDouble();
                    double rotatedX = dirX * cos - dirY * sin;
                    double rotatedY = dirX * sin + dirY * cos;
                    
                    projectileDir = new FixV2(
                        Fix64.FromDouble(rotatedX),
                        Fix64.FromDouble(rotatedY)
                    ).Normalized;
                }
                
                Projectile projectile = new Projectile
                {
                    OwnerId = heroId,
                    Position = hero.Position,
                    Velocity = projectileDir * projectileSpeed,
                    Speed = projectileSpeed,
                    Damage = damage,
                    IsActive = true,
                    SpawnTick = world.CurrentTick,
                    MaxLifetimeTicks = PROJECTILE_LIFETIME_TICKS,
                    Piercing = isPiercing,
                    AoeRadius = GetAoeRadius(hero.WeaponType)
                };
                
                EntityId projectileId = world.CreateProjectile(projectile);
                
                // Generate events
                world.AddEvent(new Events.ProjectileSpawnedEvent
                {
                    Tick = world.CurrentTick,
                    ProjectileId = projectileId,
                    OwnerId = heroId,
                    Position = hero.Position,
                    Velocity = projectileDir * projectileSpeed,
                    Damage = damage
                });
            }
            
            // Update hero cooldown
            hero.LastShotTick = world.CurrentTick;
            world.UpdateHero(heroId, hero);
            
            // Generate shoot event (single event for the shot)
            world.AddEvent(new Events.HeroShootEvent
            {
                Tick = world.CurrentTick,
                HeroId = heroId,
                ProjectileId = EntityId.Invalid, // Not applicable for multiple projectiles
                Direction = normalizedDir
            });
        }
        
        /// <summary>
        /// Gets the number of projectiles to fire based on hero type and stars
        /// </summary>
        private static int GetProjectileCount(Hero hero)
        {
            // Base projectile count
            int count = 1;
            
            // Apply star upgrades
            if (hero.HeroType == "Archer")
            {
                // Archer: star 1 = 2 arrows, star 2 = 3 arrows, star 3 = 4 arrows
                if (hero.Stars >= 1) count = 2;
                if (hero.Stars >= 2) count = 3;
                if (hero.Stars >= 3) count = 4;
            }
            else if (hero.HeroType == "IceArcher")
            {
                // IceArcher: star 1 = 2 arrows, star 2 = 3 arrows, star 3 = 4 arrows
                if (hero.Stars >= 1) count = 2;
                if (hero.Stars >= 2) count = 3;
                if (hero.Stars >= 3) count = 4;
            }
            
            return count;
        }
        
        /// <summary>
        /// Gets projectile damage with star modifiers
        /// </summary>
        private static Fix64 GetProjectileDamage(Hero hero)
        {
            Fix64 damage = hero.Damage;
            
            // Archer star 3: double damage
            if (hero.HeroType == "Archer" && hero.Stars >= 3)
            {
                damage = damage * Fix64.FromInt(2);
            }
            
            return damage;
        }
        
        /// <summary>
        /// Gets effective attack speed with star modifiers
        /// This is called when the hero shoots to check cooldown
        /// Star 3 bonuses are applied in CommandProcessor when star is upgraded
        /// </summary>
        private static Fix64 GetEffectiveAttackSpeed(Hero hero)
        {
            // Attack speed multiplier is already applied when star 3 is upgraded
            // This method is kept for future use if needed for dynamic speed changes
            return hero.AttackSpeed;
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
                        // Store enemy position BEFORE applying damage (in case enemy dies and gets removed)
                        FixV2 enemyPosAtHit = enemy.Position;
                        
                        // Apply damage
                        Fix64 healthBefore = enemy.Health;
                        enemy.TakeDamage(proj.Damage);
                        world.UpdateEnemy(enemyId, enemy);
                        
                        // Generate damage event (store position BEFORE damage was applied)
                        world.AddEvent(new Events.EnemyDamagedEvent
                        {
                            Tick = world.CurrentTick,
                            EnemyId = enemyId,
                            AttackerId = proj.OwnerId,
                            Damage = proj.Damage,
                            RemainingHealth = enemy.Health,
                            EnemyPosition = enemyPosAtHit // Use position BEFORE damage (when hit occurred)
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
                                KillerId = proj.OwnerId,
                                IsBoss = enemy.IsBoss,
                                IsMiniBoss = enemy.IsMiniBoss
                            });
                        }
                        
                        hitSomething = true;
                        
                        // ALWAYS destroy projectile on hit (unless piercing - only ice arrow)
                        // Break immediately so projectile doesn't hit multiple enemies
                            break;
                    }
                }
                
                // Destroy projectile if it hit something (unless it's piercing)
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
            return Fix64.FromFloat(45f); // 3x faster: 15 * 3 = 45
        }
        
        private static bool IsPiercing(string weaponType, int stars)
        {
            // IceArcher is always piercing (base and all stars)
            if (weaponType != null && weaponType.ToLower().Contains("ice"))
            {
                return true;
            }
            
            // Try bridge function if available (set by Client assembly)
            if (WeaponPiercingBridge != null)
            {
                bool? result = WeaponPiercingBridge(weaponType);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }
            
            // Default: non-piercing (projectiles destroy on hit)
            return false;
        }
        
        /// <summary>
        /// Bridge function from Client assembly to check if weapon is piercing
        /// Set by WeaponConfigBridge in Client assembly
        /// </summary>
        public static System.Func<string, bool?> WeaponPiercingBridge { get; set; }
        
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

