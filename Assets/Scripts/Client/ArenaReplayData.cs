using System;
using System.Collections.Generic;
using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// Serializable replay data for arena matches
    /// Contains all commands needed to replay a match deterministically
    /// </summary>
    [Serializable]
    public class ArenaReplayData
    {
        public List<SerializableCommand> commands = new List<SerializableCommand>();
        public SerializableStateHashes stateHashes = new SerializableStateHashes();
        
        /// <summary>
        /// Convert ISimulationCommand to serializable format
        /// </summary>
        public static ArenaReplayData FromCommands(List<ISimulationCommand> commands, Dictionary<int, string> stateHashes = null)
        {
            var replay = new ArenaReplayData();
            foreach (var cmd in commands)
            {
                replay.commands.Add(SerializableCommand.FromCommand(cmd));
            }
            
            if (stateHashes != null)
            {
                replay.stateHashes = SerializableStateHashes.FromDictionary(stateHashes);
            }
            
            return replay;
        }
        
        /// <summary>
        /// Convert back to ISimulationCommand list for replay
        /// </summary>
        public List<ISimulationCommand> ToCommands()
        {
            var result = new List<ISimulationCommand>();
            foreach (var serialized in commands)
            {
                var cmd = serialized.ToCommand();
                if (cmd != null)
                {
                    result.Add(cmd);
                }
            }
            return result;
        }
    }
    
    /// <summary>
    /// Serializable wrapper for commands - uses specific fields instead of Dictionary
    /// </summary>
    [Serializable]
    public class SerializableCommand
    {
        public string commandType;
        public int tick;
        
        // SpawnHeroCommand fields
        public string heroType;
        public float positionX, positionY;
        
        // SpawnEnemyCommand fields
        public string enemyType;
        
        // ChooseUpgradeCommand fields
        public int heroId;
        public string upgradeType;
        public int upgradeTier;
        
        // ChooseWeaponCommand fields
        public string weaponType;
        
        // StartWaveCommand fields
        public int waveNumber;
        public int levelNumber;
        
        public static SerializableCommand FromCommand(ISimulationCommand cmd)
        {
            var serialized = new SerializableCommand
            {
                commandType = cmd.GetType().Name,
                tick = cmd.Tick
            };
            
            // Serialize command-specific data
            switch (cmd)
            {
                case SpawnHeroCommand spawnHero:
                    serialized.heroType = spawnHero.HeroType;
                    serialized.positionX = spawnHero.Position.X.ToFloat();
                    serialized.positionY = spawnHero.Position.Y.ToFloat();
                    break;
                    
                case SpawnEnemyCommand spawnEnemy:
                    serialized.enemyType = spawnEnemy.EnemyType;
                    serialized.positionX = spawnEnemy.Position.X.ToFloat();
                    serialized.positionY = spawnEnemy.Position.Y.ToFloat();
                    break;
                    
                case ChooseUpgradeCommand upgrade:
                    serialized.heroId = upgrade.HeroId.Value;
                    serialized.upgradeType = upgrade.UpgradeType;
                    serialized.upgradeTier = upgrade.UpgradeTier;
                    break;
                    
                case ChooseWeaponCommand weapon:
                    serialized.heroId = weapon.HeroId.Value;
                    serialized.weaponType = weapon.WeaponType;
                    break;
                    
                case StartWaveCommand wave:
                    serialized.waveNumber = wave.WaveNumber;
                    serialized.levelNumber = wave.LevelNumber;
                    break;
            }
            
            return serialized;
        }
        
        public ISimulationCommand ToCommand()
        {
            switch (commandType)
            {
                case nameof(SpawnHeroCommand):
                    return new SpawnHeroCommand
                    {
                        Tick = tick,
                        HeroType = heroType,
                        Position = new FixV2(
                            Fix64.FromFloat(positionX),
                            Fix64.FromFloat(positionY)
                        )
                    };
                    
                case nameof(SpawnEnemyCommand):
                    return new SpawnEnemyCommand
                    {
                        Tick = tick,
                        EnemyType = enemyType,
                        Position = new FixV2(
                            Fix64.FromFloat(positionX),
                            Fix64.FromFloat(positionY)
                        )
                    };
                    
                case nameof(ChooseUpgradeCommand):
                    return new ChooseUpgradeCommand
                    {
                        Tick = tick,
                        HeroId = new EntityId(heroId),
                        UpgradeType = upgradeType,
                        UpgradeTier = upgradeTier
                    };
                    
                case nameof(ChooseWeaponCommand):
                    return new ChooseWeaponCommand
                    {
                        Tick = tick,
                        HeroId = new EntityId(heroId),
                        WeaponType = weaponType
                    };
                    
                case nameof(StartWaveCommand):
                    return new StartWaveCommand
                    {
                        Tick = tick,
                        WaveNumber = waveNumber,
                        LevelNumber = levelNumber
                    };
                    
                default:
                    return null;
            }
        }
    }
    
    /// <summary>
    /// Serializable state hashes for divergence detection
    /// </summary>
    [Serializable]
    public class SerializableStateHashes
    {
        [Serializable]
        public class HashEntry
        {
            public int tick;
            public string hash;
        }
        
        public List<HashEntry> hashes = new List<HashEntry>();
        
        public static SerializableStateHashes FromDictionary(Dictionary<int, string> dict)
        {
            var result = new SerializableStateHashes();
            foreach (var kvp in dict)
            {
                result.hashes.Add(new HashEntry { tick = kvp.Key, hash = kvp.Value });
            }
            return result;
        }
        
        public Dictionary<int, string> ToDictionary()
        {
            var dict = new Dictionary<int, string>();
            foreach (var entry in hashes)
            {
                dict[entry.tick] = entry.hash;
            }
            return dict;
        }
    }
}
