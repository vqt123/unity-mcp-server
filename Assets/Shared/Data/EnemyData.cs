using ArenaGame.Shared.Math;
using System.Collections.Generic;

namespace ArenaGame.Shared.Data
{
    public static class EnemyData
    {
        public static Dictionary<string, EnemyConfig> Configs = new Dictionary<string, EnemyConfig>
        {
            { "basic", new EnemyConfig {
                EnemyType = "BasicGrunt",
                MaxHealth = Fix64.FromInt(30),
                MoveSpeed = Fix64.FromInt(2),
                Damage = Fix64.FromInt(5),
                AttackRange = Fix64.FromFloat(1.0f),
                AttackSpeed = Fix64.FromFloat(0.8f), // 0.8 attacks per second
                IsBoss = false,
                IsMiniBoss = false
            }},
            { "BasicGrunt", new EnemyConfig {
                EnemyType = "BasicGrunt",
                MaxHealth = Fix64.FromInt(30),
                MoveSpeed = Fix64.FromInt(2),
                Damage = Fix64.FromInt(5),
                AttackRange = Fix64.FromFloat(1.0f),
                AttackSpeed = Fix64.FromFloat(0.8f), // 0.8 attacks per second
                IsBoss = false,
                IsMiniBoss = false
            }},
            { "FastRunner", new EnemyConfig {
                EnemyType = "FastRunner",
                MaxHealth = Fix64.FromInt(30),
                MoveSpeed = Fix64.FromInt(4),
                Damage = Fix64.FromInt(3),
                AttackRange = Fix64.FromFloat(0.8f),
                AttackSpeed = Fix64.FromFloat(1.2f),
                IsBoss = false,
                IsMiniBoss = false
            }},
            { "Tank", new EnemyConfig {
                EnemyType = "Tank",
                MaxHealth = Fix64.FromInt(200),
                MoveSpeed = Fix64.FromInt(1),
                Damage = Fix64.FromInt(15),
                AttackRange = Fix64.FromFloat(1.2f),
                AttackSpeed = Fix64.FromFloat(0.5f),
                IsBoss = false,
                IsMiniBoss = false
            }},
            { "miniboss", new EnemyConfig {
                EnemyType = "MiniBoss",
                MaxHealth = Fix64.FromInt(500),
                MoveSpeed = Fix64.FromInt(2),
                Damage = Fix64.FromInt(25),
                AttackRange = Fix64.FromFloat(1.5f),
                AttackSpeed = Fix64.FromFloat(0.7f),
                IsBoss = false,
                IsMiniBoss = true
            }},
            { "MiniBoss", new EnemyConfig {
                EnemyType = "MiniBoss",
                MaxHealth = Fix64.FromInt(500),
                MoveSpeed = Fix64.FromInt(2),
                Damage = Fix64.FromInt(25),
                AttackRange = Fix64.FromFloat(1.5f),
                AttackSpeed = Fix64.FromFloat(0.7f),
                IsBoss = false,
                IsMiniBoss = true
            }},
            { "boss", new EnemyConfig {
                EnemyType = "Boss",
                MaxHealth = Fix64.FromInt(2000),
                MoveSpeed = Fix64.FromInt(1),
                Damage = Fix64.FromInt(50),
                AttackRange = Fix64.FromFloat(2.0f),
                AttackSpeed = Fix64.FromFloat(0.4f),
                IsBoss = true,
                IsMiniBoss = false
            }},
            { "Boss", new EnemyConfig {
                EnemyType = "Boss",
                MaxHealth = Fix64.FromInt(2000),
                MoveSpeed = Fix64.FromInt(1),
                Damage = Fix64.FromInt(50),
                AttackRange = Fix64.FromFloat(2.0f),
                AttackSpeed = Fix64.FromFloat(0.4f),
                IsBoss = true,
                IsMiniBoss = false
            }},
            { "EliteTank", new EnemyConfig {
                EnemyType = "EliteTank",
                MaxHealth = Fix64.FromInt(120), // 4x BasicGrunt health (30 * 4)
                MoveSpeed = Fix64.FromInt(1),
                Damage = Fix64.FromInt(10),
                AttackRange = Fix64.FromFloat(1.2f),
                AttackSpeed = Fix64.FromFloat(0.5f),
                IsBoss = false,
                IsMiniBoss = false
            }},
        };

        public static EnemyConfig GetConfig(string enemyType)
        {
            if (Configs.TryGetValue(enemyType, out EnemyConfig config))
            {
                return config;
            }
            return Configs["BasicGrunt"]; // Fallback
        }
    }

    public struct EnemyConfig
    {
        public string EnemyType;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 Damage;
        public Fix64 AttackRange;
        public Fix64 AttackSpeed;
        public bool IsBoss;
        public bool IsMiniBoss;
    }
}

