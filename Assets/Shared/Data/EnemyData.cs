using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Data
{
    /// <summary>
    /// Configuration data for enemy types
    /// </summary>
    public struct EnemyData
    {
        public string EnemyType;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 Damage;
        public Fix64 AttackRange;
        public int AttackCooldownTicks; // Ticks between attacks
        public bool IsBoss;
        public bool IsMiniBoss;
        
        // Common enemy configurations
        public static EnemyData BasicEnemy => new EnemyData
        {
            EnemyType = "Basic",
            MaxHealth = Fix64.FromInt(30),
            MoveSpeed = Fix64.FromInt(2),
            Damage = Fix64.FromInt(5),
            AttackRange = Fix64.FromFloat(0.5f),
            AttackCooldownTicks = 30, // 1 second at 30 tps
            IsBoss = false,
            IsMiniBoss = false
        };
        
        public static EnemyData FastEnemy => new EnemyData
        {
            EnemyType = "Fast",
            MaxHealth = Fix64.FromInt(20),
            MoveSpeed = Fix64.FromInt(4),
            Damage = Fix64.FromInt(3),
            AttackRange = Fix64.FromFloat(0.5f),
            AttackCooldownTicks = 20,
            IsBoss = false,
            IsMiniBoss = false
        };
        
        public static EnemyData TankEnemy => new EnemyData
        {
            EnemyType = "Tank",
            MaxHealth = Fix64.FromInt(80),
            MoveSpeed = Fix64.FromInt(1),
            Damage = Fix64.FromInt(10),
            AttackRange = Fix64.FromFloat(0.5f),
            AttackCooldownTicks = 45,
            IsBoss = false,
            IsMiniBoss = false
        };
        
        public static EnemyData MiniBoss => new EnemyData
        {
            EnemyType = "MiniBoss",
            MaxHealth = Fix64.FromInt(200),
            MoveSpeed = Fix64.FromFloat(1.5f),
            Damage = Fix64.FromInt(15),
            AttackRange = Fix64.FromInt(1),
            AttackCooldownTicks = 60,
            IsBoss = false,
            IsMiniBoss = true
        };
        
        public static EnemyData Boss => new EnemyData
        {
            EnemyType = "Boss",
            MaxHealth = Fix64.FromInt(500),
            MoveSpeed = Fix64.FromInt(1),
            Damage = Fix64.FromInt(25),
            AttackRange = Fix64.FromFloat(1.5f),
            AttackCooldownTicks = 90,
            IsBoss = true,
            IsMiniBoss = false
        };
    }
}

