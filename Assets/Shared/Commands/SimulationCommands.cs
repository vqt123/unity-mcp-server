using ArenaGame.Shared.Entities;
using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Commands
{
    /// <summary>
    /// Base interface for all simulation commands
    /// Commands are PLAYER CHOICES that affect the simulation
    /// Most game logic runs deterministically without commands
    /// </summary>
    public interface ISimulationCommand
    {
        int Tick { get; set; }
    }
    
    /// <summary>
    /// Player chooses an upgrade for their hero
    /// </summary>
    public struct ChooseUpgradeCommand : ISimulationCommand
    {
        public int Tick { get; set; }
        public EntityId HeroId;
        public string UpgradeType; // "Damage", "AttackSpeed", "MoveSpeed", "Health", etc.
        public int UpgradeTier;
    }
    
    /// <summary>
    /// Player chooses a weapon upgrade
    /// </summary>
    public struct ChooseWeaponCommand : ISimulationCommand
    {
        public int Tick { get; set; }
        public EntityId HeroId;
        public string WeaponType; // "SMG", "Shotgun", "Rifle", etc.
    }
    
    /// <summary>
    /// Start a new wave (player presses ready/start)
    /// </summary>
    public struct StartWaveCommand : ISimulationCommand
    {
        public int Tick { get; set; }
        public int WaveNumber;
        public int LevelNumber;
    }
    
    /// <summary>
    /// Spawn hero command (initialization only)
    /// </summary>
    public struct SpawnHeroCommand : ISimulationCommand
    {
        public int Tick { get; set; }
        public string HeroType;
        public FixV2 Position;
    }
}

