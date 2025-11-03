using ArenaGame.Shared.Data;
using ArenaGame.Shared.Math;
using System;

namespace ArenaGame.Client
{
    /// <summary>
    /// Converts HeroConfigSO ScriptableObjects to Shared HeroConfig structs
    /// Sets up bridge between Client and Shared assemblies
    /// </summary>
    public static class HeroConfigConverter
    {
        private static bool bridgeRegistered = false;
        
        /// <summary>
        /// Registers the bridge function so Shared assembly can access Client HeroConfigSO
        /// Call this from GameBootstrapper or similar initialization code
        /// </summary>
        public static void RegisterBridge()
        {
            if (bridgeRegistered) return;
            
            HeroData.ClientConfigBridge = GetHeroConfigFromSO;
            bridgeRegistered = true;
            UnityEngine.Debug.Log("[HeroConfigConverter] Registered bridge to HeroData.ClientConfigBridge");
        }
        
        /// <summary>
        /// Bridge function: Gets HeroConfig from ScriptableObjects
        /// Called by Shared assembly via HeroData.ClientConfigBridge
        /// </summary>
        private static HeroConfig? GetHeroConfigFromSO(string heroType)
        {
            // Try to get from HeroConfigDatabase (ScriptableObjects)
            if (HeroConfigDatabase.Instance != null)
            {
                HeroConfigSO configSO = HeroConfigDatabase.Instance.GetHeroConfig(heroType);
                if (configSO != null)
                {
                    HeroConfig? converted = ToSharedConfig(configSO);
                    if (converted.HasValue)
                    {
                        UnityEngine.Debug.Log($"[HeroConfigConverter] Using ScriptableObject config for '{heroType}'");
                        return converted.Value;
                    }
                }
            }
            
            // Return null to fall back to hardcoded configs
            return null;
        }
        
        /// <summary>
        /// Converts a HeroConfigSO to a Shared HeroConfig
        /// Returns null if config is invalid
        /// </summary>
        public static HeroConfig? ToSharedConfig(HeroConfigSO configSO)
        {
            if (configSO == null)
            {
                return null;
            }
            
            // Validate weapon type is assigned
            if (string.IsNullOrEmpty(configSO.weaponType))
            {
                UnityEngine.Debug.LogError($"[HeroConfigConverter] HeroConfigSO '{configSO.heroTypeName}' has no weapon type assigned!");
                return null;
            }
            
            // Convert to Shared HeroConfig
            HeroConfig config = new HeroConfig
            {
                HeroType = configSO.heroTypeName,
                MaxHealth = Fix64.FromFloat(configSO.maxHealth),
                MoveSpeed = Fix64.FromFloat(configSO.moveSpeed),
                Damage = Fix64.FromFloat(configSO.damage),
                AttackSpeed = Fix64.FromFloat(configSO.attackSpeed),
                WeaponType = configSO.weaponType,
                WeaponTier = 1, // Start at tier 1 (can be upgraded later)
                Stars = 0 // Start with 0 stars
            };
            
            return config;
        }
    }
}

