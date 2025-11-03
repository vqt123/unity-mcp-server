using ArenaGame.Shared.Math;
using System;
using System.Collections.Generic;

namespace ArenaGame.Shared.Data
{
    public static class HeroData
    {
        /// <summary>
        /// Bridge function from Client assembly to get HeroConfig from ScriptableObjects
        /// Set by HeroConfigBridge in Client assembly
        /// </summary>
        public static Func<string, HeroConfig?> ClientConfigBridge { get; set; }
        
        public static Dictionary<string, HeroConfig> Configs = new Dictionary<string, HeroConfig>
        {
            { "DefaultHero", new HeroConfig {
                HeroType = "DefaultHero",
                MaxHealth = Fix64.FromInt(150),
                MoveSpeed = Fix64.FromInt(5),
                Damage = Fix64.FromInt(100),
                AttackSpeed = Fix64.FromFloat(0.5f), // 1 per 2s
                WeaponType = "Pistol",
                WeaponTier = 1,
                Stars = 0
            }},
            { "FastHero", new HeroConfig {
                HeroType = "FastHero",
                MaxHealth = Fix64.FromInt(150),
                MoveSpeed = Fix64.FromInt(7),
                Damage = Fix64.FromInt(100),
                AttackSpeed = Fix64.FromFloat(0.5f), // 1 per 2s
                WeaponType = "SMG",
                WeaponTier = 1,
                Stars = 0
            }},
            { "TankHero", new HeroConfig {
                HeroType = "TankHero",
                MaxHealth = Fix64.FromInt(200),
                MoveSpeed = Fix64.FromInt(5),
                Damage = Fix64.FromInt(100),
                AttackSpeed = Fix64.FromFloat(0.5f), // 1 per 2s
                WeaponType = "Shotgun",
                WeaponTier = 1,
                Stars = 0
            }},
            { "Archer", new HeroConfig {
                HeroType = "Archer",
                MaxHealth = Fix64.FromInt(150),
                MoveSpeed = Fix64.FromInt(5),
                Damage = Fix64.FromInt(100),
                AttackSpeed = Fix64.FromFloat(1.0f), // 1 per 1s
                WeaponType = "Bow",
                WeaponTier = 1,
                Stars = 0
            }},
            { "IceArcher", new HeroConfig {
                HeroType = "IceArcher",
                MaxHealth = Fix64.FromInt(150),
                MoveSpeed = Fix64.FromInt(5),
                Damage = Fix64.FromInt(100),
                AttackSpeed = Fix64.FromFloat(0.5f), // 1 per 2s
                WeaponType = "Bow",
                WeaponTier = 1,
                Stars = 0
            }},
            { "Mage", new HeroConfig {
                HeroType = "Mage",
                MaxHealth = Fix64.FromInt(120),
                MoveSpeed = Fix64.FromInt(5),
                Damage = Fix64.FromInt(100),
                AttackSpeed = Fix64.FromFloat(0.5f), // 1 per 2s
                WeaponType = "Firewand",
                WeaponTier = 1,
                Stars = 0
            }},
            { "Warrior", new HeroConfig {
                HeroType = "Warrior",
                MaxHealth = Fix64.FromInt(200),
                MoveSpeed = Fix64.FromInt(5),
                Damage = Fix64.FromInt(100),
                AttackSpeed = Fix64.FromFloat(0.5f), // 1 per 2s
                WeaponType = "Sword",
                WeaponTier = 1,
                Stars = 0
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
        public int Stars;
    }
}

