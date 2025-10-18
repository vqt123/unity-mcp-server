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
            HeroConfig data = GetHeroData(cmd.HeroType);
            SpawnSystem.SpawnHero(world, data, cmd.Position);
        }
        
        private void ProcessSpawnEnemy(SpawnEnemyCommand cmd)
        {
            EnemyConfig data = GetEnemyData(cmd.EnemyType);
            SpawnSystem.SpawnEnemy(world, data, cmd.Position);
        }
        
        private HeroConfig GetHeroData(string heroType)
        {
            return HeroData.GetConfig(heroType);
        }
        
        private EnemyConfig GetEnemyData(string enemyType)
        {
            return EnemyData.GetConfig(enemyType);
        }
    }
}

