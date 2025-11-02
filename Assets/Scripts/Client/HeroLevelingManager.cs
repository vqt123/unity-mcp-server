using UnityEngine;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages persistent hero leveling - uses gold to permanently upgrade hero stats
    /// </summary>
    public static class HeroLevelingManager
    {
        // Cost calculation: baseCost * (level + 1) ^ costMultiplier
        private const int baseLevelCost = 50;
        private const float costMultiplier = 1.2f;
        
        // Stat increases per level
        private const float healthPerLevel = 20f;
        private const float damagePerLevel = 10f;
        private const float moveSpeedPerLevel = 0.5f;
        private const float attackSpeedPerLevel = 0.2f;
        
        /// <summary>
        /// Calculates the cost to level up a hero
        /// </summary>
        public static int GetLevelUpCost(int currentLevel)
        {
            // Cost increases exponentially: baseCost * (level + 1)^1.2
            float cost = baseLevelCost * Mathf.Pow(currentLevel + 1, costMultiplier);
            return Mathf.RoundToInt(cost);
        }
        
        /// <summary>
        /// Attempts to level up a hero using gold
        /// </summary>
        public static bool LevelUpHero(string heroType)
        {
            if (PlayerDataManager.Instance == null || GoldManager.Instance == null)
            {
                Debug.LogError("[HeroLeveling] PlayerDataManager or GoldManager not available!");
                return false;
            }
            
            HeroProgressData progress = PlayerDataManager.Instance.GetHeroProgress(heroType);
            int currentLevel = progress.level;
            int cost = GetLevelUpCost(currentLevel);
            
            // Check if player has enough gold
            if (GoldManager.Instance.CurrentGold < cost)
            {
                Debug.LogWarning($"[HeroLeveling] Not enough gold! Need {cost}, have {GoldManager.Instance.CurrentGold}");
                return false;
            }
            
            // Spend gold
            if (!GoldManager.Instance.SpendGold(cost))
            {
                return false;
            }
            
            // Level up
            progress.level++;
            PlayerDataManager.Instance.PlayerBlob.SetHeroProgress(heroType, progress);
            PlayerDataManager.Instance.SaveData();
            
            Debug.Log($"[HeroLeveling] {heroType} leveled up to level {progress.level} (cost: {cost} gold)");
            return true;
        }
        
        /// <summary>
        /// Gets stat bonuses for a hero based on their level
        /// </summary>
        public static HeroStatBonuses GetStatBonuses(int level)
        {
            // Level 1 = no bonuses, level 2+ = bonuses based on (level - 1)
            int bonusLevels = Mathf.Max(0, level - 1);
            
            return new HeroStatBonuses
            {
                healthBonus = healthPerLevel * bonusLevels,
                damageBonus = damagePerLevel * bonusLevels,
                moveSpeedBonus = moveSpeedPerLevel * bonusLevels,
                attackSpeedBonus = attackSpeedPerLevel * bonusLevels
            };
        }
        
        /// <summary>
        /// Gets stat bonuses for a hero type from saved progress
        /// </summary>
        public static HeroStatBonuses GetHeroStatBonuses(string heroType)
        {
            if (PlayerDataManager.Instance == null)
            {
                return new HeroStatBonuses();
            }
            
            HeroProgressData progress = PlayerDataManager.Instance.GetHeroProgress(heroType);
            return GetStatBonuses(progress.level);
        }
    }
    
    /// <summary>
    /// Stat bonuses from leveling
    /// </summary>
    public struct HeroStatBonuses
    {
        public float healthBonus;
        public float damageBonus;
        public float moveSpeedBonus;
        public float attackSpeedBonus;
        
        public Fix64 HealthBonus => Fix64.FromFloat(healthBonus);
        public Fix64 DamageBonus => Fix64.FromFloat(damageBonus);
        public Fix64 MoveSpeedBonus => Fix64.FromFloat(moveSpeedBonus);
        public Fix64 AttackSpeedBonus => Fix64.FromFloat(attackSpeedBonus);
    }
}

