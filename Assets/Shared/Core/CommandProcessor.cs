using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Data;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Systems;
using ArenaGame.Shared.Math;
using ArenaGame.Shared.Events;
using System.Collections.Generic;

namespace ArenaGame.Shared.Core
{
    /// <summary>
    /// Processes commands and applies them to the simulation
    /// </summary>
    public class CommandProcessor
    {
        private SimulationWorld world;
        
        public CommandProcessor(SimulationWorld world)
        {
            this.world = world;
        }
        
        public void ProcessCommands(List<ISimulationCommand> commands)
        {
            foreach (var command in commands)
            {
                ProcessCommand(command);
            }
        }
        
        private void ProcessCommand(ISimulationCommand command)
        {
            switch (command)
            {
                case ChooseUpgradeCommand upgrade:
                    ProcessUpgrade(upgrade);
                    break;
                    
                case ChooseWeaponCommand weapon:
                    ProcessWeapon(weapon);
                    break;
                    
                case StartWaveCommand startWave:
                    ProcessStartWave(startWave);
                    break;
                    
                case SpawnHeroCommand spawnHero:
                    ProcessSpawnHero(spawnHero);
                    break;
                    
                case SpawnEnemyCommand spawnEnemy:
                    ProcessSpawnEnemy(spawnEnemy);
                    break;
            }
        }
        
        private void ProcessUpgrade(ChooseUpgradeCommand cmd)
        {
            if (!world.TryGetHero(cmd.HeroId, out Hero hero)) return;
            
            // Apply upgrade based on type
            switch (cmd.UpgradeType)
            {
                case "Damage":
                    hero.Damage += Fix64.FromInt(10);
                    break;
                case "AttackSpeed":
                    hero.AttackSpeed += Fix64.FromFloat(0.5f);
                    hero.ShotCooldownTicks = (int)(SimulationConfig.TICKS_PER_SECOND / hero.AttackSpeed).ToLong();
                    break;
                case "MoveSpeed":
                    hero.MoveSpeed += Fix64.FromInt(1);
                    break;
                case "Health":
                    hero.MaxHealth += Fix64.FromInt(30);
                    hero.Health = Fix64.Min(hero.Health + Fix64.FromInt(30), hero.MaxHealth);
                    break;
                case "Star":
                    // Star upgrade - only for Archer and IceArcher, max 3 stars
                    if ((hero.HeroType == "Archer" || hero.HeroType == "IceArcher") && hero.Stars < 3)
                    {
                        hero.Stars++;
                        
                        // Apply star 3 bonuses immediately if applicable
                        if (hero.Stars >= 3)
                        {
                            // Star 3: faster attack speed (for both Archer and IceArcher)
                            Fix64 speedMultiplier = Fix64.FromFloat(1.5f);
                            hero.AttackSpeed = hero.AttackSpeed * speedMultiplier;
                            hero.ShotCooldownTicks = (int)(SimulationConfig.TICKS_PER_SECOND / hero.AttackSpeed).ToLong();
                        }
                    }
                    break;
            }
            
            world.UpdateHero(cmd.HeroId, hero);
            
            // Generate upgrade event
            world.AddEvent(new Events.UpgradeChosenEvent
            {
                Tick = world.CurrentTick,
                HeroId = cmd.HeroId,
                UpgradeType = cmd.UpgradeType
            });
        }
        
        private void ProcessWeapon(ChooseWeaponCommand cmd)
        {
            if (!world.TryGetHero(cmd.HeroId, out Hero hero)) return;
            
            hero.WeaponType = cmd.WeaponType;
            hero.WeaponTier++;
            
            world.UpdateHero(cmd.HeroId, hero);
            
            // TODO: Generate WeaponChangedEvent
        }
        
        private void ProcessStartWave(StartWaveCommand cmd)
        {
            // TODO: Integrate with wave system
            // For now, just generate event
            world.AddEvent(new Events.WaveStartedEvent
            {
                Tick = world.CurrentTick,
                WaveNumber = cmd.WaveNumber,
                LevelNumber = cmd.LevelNumber
            });
        }
        
        private void ProcessSpawnHero(SpawnHeroCommand cmd)
        {
            // Shared assembly logging - using System.Diagnostics (no Unity dependencies)
            System.Diagnostics.Debug.WriteLine($"[CommandProcessor] ProcessSpawnHero called: {cmd.HeroType} at tick {cmd.Tick}");
            
            HeroConfig data = GetHeroData(cmd.HeroType);
            System.Diagnostics.Debug.WriteLine($"[CommandProcessor] HeroConfig retrieved: {data.HeroType}, MaxHealth: {data.MaxHealth.ToInt()}");
            
            // Heroes always spawn at level 0 in arena (ignoring persistent level)
            // Persistent hero level only affects stat bonuses, not arena starting level
            HeroLevelBonuses? persistentBonuses = GetHeroLevelBonuses(cmd.HeroType);
            
            // Create bonuses with level 0 for arena, but keep stat bonuses from persistent level
            HeroLevelBonuses? bonuses = null;
            if (persistentBonuses.HasValue)
            {
                var pb = persistentBonuses.Value;
                // Keep stat bonuses but set arena level to 0
                bonuses = new HeroLevelBonuses
                {
                    Level = 0, // Always start at level 0 in arena
                    HealthBonus = pb.HealthBonus,
                    DamageBonus = pb.DamageBonus,
                    MoveSpeedBonus = pb.MoveSpeedBonus,
                    AttackSpeedBonus = pb.AttackSpeedBonus
                };
                System.Diagnostics.Debug.WriteLine($"[CommandProcessor] Using persistent bonuses, level set to 0 for arena");
            }
            else
            {
                // No persistent bonuses, but still start at level 0
                bonuses = new HeroLevelBonuses
                {
                    Level = 0,
                    HealthBonus = Fix64.Zero,
                    DamageBonus = Fix64.Zero,
                    MoveSpeedBonus = Fix64.Zero,
                    AttackSpeedBonus = Fix64.Zero
                };
                System.Diagnostics.Debug.WriteLine($"[CommandProcessor] No persistent bonuses, using default (level 0)");
            }
            
            System.Diagnostics.Debug.WriteLine($"[CommandProcessor] Calling SpawnSystem.SpawnHero");
            EntityId heroId = SpawnSystem.SpawnHero(world, data, cmd.Position, bonuses);
            System.Diagnostics.Debug.WriteLine($"[CommandProcessor] Hero spawned with EntityId: {heroId.Value}");
        }
        
        /// <summary>
        /// Gets hero level bonuses from Client assembly via bridge
        /// </summary>
        private HeroLevelBonuses? GetHeroLevelBonuses(string heroType)
        {
            // Try to get from bridge (Client assembly will set this)
            if (HeroLevelBridge != null)
            {
                return HeroLevelBridge(heroType);
            }
            return null;
        }
        
        /// <summary>
        /// Bridge function from Client assembly to get hero level bonuses
        /// Set by Client assembly during initialization
        /// </summary>
        public static System.Func<string, HeroLevelBonuses?> HeroLevelBridge { get; set; }
        
        private void ProcessSpawnEnemy(SpawnEnemyCommand cmd)
        {
            EnemyConfig data = GetEnemyData(cmd.EnemyType);
            SpawnSystem.SpawnEnemy(world, data, cmd.Position);
        }
        
        private HeroConfig GetHeroData(string heroType)
        {
            // Try Client-side config first (if bridge is available)
            if (HeroData.ClientConfigBridge != null)
            {
                HeroConfig? config = HeroData.ClientConfigBridge(heroType);
                if (config.HasValue)
                {
                    return config.Value;
                }
            }
            
            // Fallback to hardcoded configs
            return HeroData.GetConfig(heroType);
        }
        
        private EnemyConfig GetEnemyData(string enemyType)
        {
            return EnemyData.GetConfig(enemyType);
        }
    }
}

