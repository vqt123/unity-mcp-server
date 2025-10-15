using ArenaGame.Shared.Math;
using System.Collections.Generic;

namespace ArenaGame.Shared.Data
{
    public static class HeroData
    {
        public static Dictionary<string, HeroConfig> Configs = new Dictionary<string, HeroConfig>
        {
            { "DefaultHero", new HeroConfig {
                HeroType = "DefaultHero",
                MaxHealth = Fix64.FromInt(100),
                MoveSpeed = Fix64.FromInt(5),
                Damage = Fix64.FromInt(10),
                AttackSpeed = Fix64.FromFloat(2.0f),
                WeaponType = "Pistol",
                WeaponTier = 1
            }},
            { "FastHero", new HeroConfig {
                HeroType = "FastHero",
                MaxHealth = Fix64.FromInt(80),
                MoveSpeed = Fix64.FromInt(7),
                Damage = Fix64.FromInt(8),
                AttackSpeed = Fix64.FromFloat(3.0f),
                WeaponType = "SMG",
                WeaponTier = 1
            }},
            { "TankHero", new HeroConfig {
                HeroType = "TankHero",
                MaxHealth = Fix64.FromInt(150),
                MoveSpeed = Fix64.FromInt(3),
                Damage = Fix64.FromInt(15),
                AttackSpeed = Fix64.FromFloat(1.0f),
                WeaponType = "Shotgun",
                WeaponTier = 1
            }},
        };

        public static HeroConfig GetConfig(string heroType)
        {
            if (Configs.TryGetValue(heroType, out HeroConfig config))
            {
                return config;
            }
            return Configs["DefaultHero"]; // Fallback
        }
    }

    public struct HeroConfig
    {
        public string HeroType;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 Damage;
        public Fix64 AttackSpeed;
        public string WeaponType;
        public int WeaponTier;
    }
}

