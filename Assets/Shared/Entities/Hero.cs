using ArenaGame.Shared.Math;

namespace ArenaGame.Shared.Entities
{
    /// <summary>
    /// Hero entity - player-controlled character
    /// </summary>
    public struct Hero
    {
        public EntityId Id;
        public string HeroType;
        
        // Transform
        public FixV2 Position;
        public FixV2 Velocity;
        public Fix64 Rotation; // Radians
        
        // Stats
        public Fix64 Health;
        public Fix64 MaxHealth;
        public Fix64 MoveSpeed;
        
        // Combat
        public Fix64 Damage;
        public Fix64 AttackSpeed; // Attacks per second
        public int LastShotTick;
        public int ShotCooldownTicks;
        public string WeaponType;
        public int WeaponTier;
        
        // Progression
        public int Level;
        public int CurrentXP;
        public int XPToNextLevel;
        
        // State
        public bool IsAlive;
        
        public bool CanShoot(int currentTick)
        {
            return IsAlive && (currentTick - LastShotTick) >= ShotCooldownTicks;
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

        public void Heal(Fix64 amount)
        {
            Health = Fix64.Min(Health + amount, MaxHealth);
        }

        public bool GainXP(int xp, out bool leveledUp)
        {
            CurrentXP += xp;
            leveledUp = false;
            
            while (CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                Level++;
                leveledUp = true;
                
                // XP requirement increases by 20% per level
                XPToNextLevel = (XPToNextLevel * 12) / 10;
            }
            
            return leveledUp;
        }
    }
}

