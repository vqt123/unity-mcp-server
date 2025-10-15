using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Data
{
    /// <summary>
    /// Configuration data for hero types
    /// </summary>
    public struct HeroData
    {
        public string HeroType;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 Damage;
        public Fix64 AttackSpeed; // Attacks per second
        public string WeaponType;
        public int WeaponTier;
        
        // Common hero configurations
        public static HeroData DefaultHero => new HeroData
        {
            HeroType = "Default",
            MaxHealth = Fix64.FromInt(100),
            MoveSpeed = Fix64.FromInt(5),
            Damage = Fix64.FromInt(10),
            AttackSpeed = Fix64.FromFloat(2.0f),
            WeaponType = "Pistol",
            WeaponTier = 1
        };
        
        public static HeroData FastHero => new HeroData
        {
            HeroType = "Fast",
            MaxHealth = Fix64.FromInt(80),
            MoveSpeed = Fix64.FromInt(7),
            Damage = Fix64.FromInt(8),
            AttackSpeed = Fix64.FromFloat(3.0f),
            WeaponType = "SMG",
            WeaponTier = 1
        };
        
        public static HeroData TankHero => new HeroData
        {
            HeroType = "Tank",
            MaxHealth = Fix64.FromInt(150),
            MoveSpeed = Fix64.FromInt(3),
            Damage = Fix64.FromInt(15),
            AttackSpeed = Fix64.FromFloat(1.0f),
            WeaponType = "Shotgun",
            WeaponTier = 1
        };
    }
}

