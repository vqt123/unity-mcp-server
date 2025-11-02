using ArenaGame.Shared.Core;
using ArenaGame.Shared.Math;
using ArenaGame.Shared.Systems;

namespace ArenaGame.Client
{
    /// <summary>
    /// Provides a bridge for the Shared assembly to access Client-side hero level data.
    /// This avoids a direct dependency from Shared to Client.
    /// </summary>
    public static class HeroLevelBridge
    {
        /// <summary>
        /// Registers the bridge function with the CommandProcessor in the Shared assembly.
        /// This should be called once during client initialization (e.g., in GameBootstrapper).
        /// </summary>
        public static void RegisterBridge()
        {
            CommandProcessor.HeroLevelBridge = GetHeroLevelBonuses;
            UnityEngine.Debug.Log("[HeroLevelBridge] Registered HeroLevelBridge with CommandProcessor.");
        }
        
        /// <summary>
        /// Retrieves the level bonuses for a given hero type from PlayerDataManager.
        /// </summary>
        /// <param name="heroType">The name of the hero type.</param>
        /// <returns>HeroLevelBonuses struct if hero has progress, or null if not found.</returns>
        private static HeroLevelBonuses? GetHeroLevelBonuses(string heroType)
        {
            if (PlayerDataManager.Instance != null)
            {
                HeroStatBonuses bonuses = HeroLevelingManager.GetHeroStatBonuses(heroType);
                
                HeroProgressData progress = PlayerDataManager.Instance.GetHeroProgress(heroType);
                
                return new HeroLevelBonuses
                {
                    Level = progress.level,
                    HealthBonus = bonuses.HealthBonus,
                    DamageBonus = bonuses.DamageBonus,
                    MoveSpeedBonus = bonuses.MoveSpeedBonus,
                    AttackSpeedBonus = bonuses.AttackSpeedBonus
                };
            }
            
            return null; // No bonuses if PlayerDataManager not initialized
        }
    }
}

