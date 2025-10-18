using ArenaGame.Shared.Core;
using ArenaGame.Shared.Data;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Systems
{
    /// <summary>
    /// Handles entity spawning with deterministic positioning
    /// </summary>
    public static class SpawnSystem
    {
        public static EntityId SpawnHero(SimulationWorld world, HeroConfig data, FixV2 position)
        {
            // Calculate shot cooldown ticks from attacks per second
            Fix64 ticksPerAttack = SimulationConfig.TICKS_PER_SECOND / data.AttackSpeed;
            int cooldownTicks = (int)ticksPerAttack.ToLong();
            
            Hero hero = new Hero
            {
                HeroType = data.HeroType,
                Position = position,
                Velocity = FixV2.Zero,
                Rotation = Fix64.Zero,
                Health = data.MaxHealth,
                MaxHealth = data.MaxHealth,
                MoveSpeed = data.MoveSpeed,
                Damage = data.Damage,
                AttackSpeed = data.AttackSpeed,
                LastShotTick = -cooldownTicks, // Can shoot immediately
                ShotCooldownTicks = cooldownTicks,
                WeaponType = data.WeaponType,
                WeaponTier = data.WeaponTier,
                Level = 1,
                CurrentXP = 0,
                XPToNextLevel = 100,
                IsAlive = true
            };
            
            EntityId id = world.CreateHero(hero);
            
            // Generate spawn event
            world.AddEvent(new Events.HeroSpawnedEvent
            {
                Tick = world.CurrentTick,
                HeroId = id,
                HeroType = data.HeroType,
                Position = position,
                MaxHealth = data.MaxHealth,
                MoveSpeed = data.MoveSpeed,
                Damage = data.Damage,
                AttackSpeed = data.AttackSpeed,
                WeaponType = data.WeaponType,
                WeaponTier = data.WeaponTier
            });
            
            return id;
        }
        
        public static EntityId SpawnEnemy(SimulationWorld world, EnemyConfig data, FixV2 position)
        {
            Enemy enemy = new Enemy
            {
                EnemyType = data.EnemyType,
                Position = position,
                Velocity = FixV2.Zero,
                Health = data.MaxHealth,
                MaxHealth = data.MaxHealth,
                MoveSpeed = data.MoveSpeed,
                Damage = data.Damage,
                TargetId = EntityId.Invalid,
                AttackRange = data.AttackRange,
                LastAttackTick = 0,
                AttackCooldownTicks = (int)(SimulationConfig.TICKS_PER_SECOND / data.AttackSpeed).ToLong(),
                IsAlive = true,
                IsBoss = data.IsBoss,
                IsMiniBoss = data.IsMiniBoss
            };
            
            EntityId id = world.CreateEnemy(enemy);
            
            // Generate spawn event
            world.AddEvent(new Events.EnemySpawnedEvent
            {
                Tick = world.CurrentTick,
                EnemyId = id,
                EnemyType = data.EnemyType,
                Position = position,
                MaxHealth = data.MaxHealth,
                MoveSpeed = data.MoveSpeed,
                Damage = data.Damage,
                IsBoss = data.IsBoss,
                IsMiniBoss = data.IsMiniBoss
            });
            
            return id;
        }
        
        // Deterministic spawn position generator
        public static FixV2 GetSpawnPositionOnCircle(int index, int total, Fix64 radius)
        {
            // Simple deterministic circular placement
            // In a real implementation, you'd use fixed-point trigonometry
            // For now, using a simple radial distribution
            
            Fix64 angleStep = Fix64.FromFloat(6.283185f) / Fix64.FromInt(total); // 2*PI / total
            Fix64 angle = angleStep * Fix64.FromInt(index);
            
            // Approximate sin/cos with simple formula (good enough for spawning)
            // In production, use lookup tables for fixed-point trig
            Fix64 x = radius * Fix64.FromFloat((float)System.Math.Cos((double)angle.ToFloat()));
            Fix64 y = radius * Fix64.FromFloat((float)System.Math.Sin((double)angle.ToFloat()));
            
            return new FixV2(x, y);
        }
    }
}

