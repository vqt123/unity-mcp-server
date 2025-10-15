using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Entities
{
    /// <summary>
    /// Enemy entity - AI-controlled hostile
    /// </summary>
    public struct Enemy
    {
        public EntityId Id;
        public string EnemyType;
        
        // Transform
        public FixV2 Position;
        public FixV2 Velocity;
        
        // Stats
        public Fix64 Health;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        public Fix64 Damage;
        
        // AI
        public EntityId TargetId;
        public Fix64 AttackRange;
        public int LastAttackTick;
        public int AttackCooldownTicks;
        
        // State
        public bool IsAlive;
        public bool IsBoss;
        public bool IsMiniBoss;
        
        public bool CanAttack(int currentTick)
        {
            return IsAlive && (currentTick - LastAttackTick) >= AttackCooldownTicks;
        }

        public void TakeDamage(Fix64 amount)
        {
            Health -= amount;
            if (Health <= Fix64.Zero)
            {
                Health = Fix64.Zero;
                IsAlive = false;
            }
        }
    }
}

