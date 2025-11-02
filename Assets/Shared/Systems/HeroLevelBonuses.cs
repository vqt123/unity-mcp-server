using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Systems
{
    /// <summary>
    /// Level bonuses to apply when spawning heroes
    /// Passed from Client to Shared assembly
    /// </summary>
    public struct HeroLevelBonuses
    {
        public int Level;
        public Fix64 HealthBonus;
        public Fix64 DamageBonus;
        public Fix64 MoveSpeedBonus;
        public Fix64 AttackSpeedBonus;
        
        public bool IsValid => Level > 0;
    }
}

