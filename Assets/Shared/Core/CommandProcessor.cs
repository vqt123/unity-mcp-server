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
            }
        }
        
        private void ProcessUpgrade(ChooseUpgradeCommand cmd)
        {
            if (!world.TryGetHero(cmd.HeroId, out Hero hero)) return;
            
            // Apply upgrade based on type
            switch (cmd.UpgradeType)
            {
                case "Damage":
                    hero.Damage += Fix64.FromInt(5);
                    break;
                case "AttackSpeed":
                    hero.AttackSpeed += Fix64.FromFloat(0.5f);
                    hero.ShotCooldownTicks = (int)(SimulationConfig.TICKS_PER_SECOND / hero.AttackSpeed).ToLong();
                    break;
                case "MoveSpeed":
                    hero.MoveSpeed += Fix64.FromInt(1);
                    break;
                case "Health":
                    hero.MaxHealth += Fix64.FromInt(20);
                    hero.Health = Fix64.Min(hero.Health + Fix64.FromInt(20), hero.MaxHealth);
                    break;
            }
            
            world.UpdateHero(cmd.HeroId, hero);
            
            // TODO: Generate UpgradeChosenEvent
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
            HeroData data = GetHeroData(cmd.HeroType);
            SpawnSystem.SpawnHero(world, data, cmd.Position);
        }
        
        private HeroData GetHeroData(string heroType)
        {
            switch (heroType)
            {
                case "Fast": return HeroData.FastHero;
                case "Tank": return HeroData.TankHero;
                default: return HeroData.DefaultHero;
            }
        }
        
        private EnemyData GetEnemyData(string enemyType)
        {
            switch (enemyType)
            {
                case "Fast": return EnemyData.FastEnemy;
                case "Tank": return EnemyData.TankEnemy;
                case "MiniBoss": return EnemyData.MiniBoss;
                case "Boss": return EnemyData.Boss;
                default: return EnemyData.BasicEnemy;
            }
        }
    }
}

