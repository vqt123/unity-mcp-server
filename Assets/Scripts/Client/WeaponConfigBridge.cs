using System;
using ArenaGame.Shared.Systems;

namespace ArenaGame.Client
{
    /// <summary>
    /// Bridges Client WeaponConfig data to Shared CombatSystem
    /// </summary>
    public static class WeaponConfigBridge
    {
        private static bool bridgeRegistered = false;
        
        /// <summary>
        /// Registers the bridge function so Shared assembly can access Client WeaponConfig
        /// Call this from GameBootstrapper or similar initialization code
        /// </summary>
        public static void RegisterBridge()
        {
            if (bridgeRegistered) return;
            
            CombatSystem.WeaponPiercingBridge = GetWeaponPiercing;
            bridgeRegistered = true;
            UnityEngine.Debug.Log("[WeaponConfigBridge] Registered bridge to CombatSystem.WeaponPiercingBridge");
        }
        
        /// <summary>
        /// Bridge function: Gets piercing status from WeaponConfig
        /// Called by Shared assembly via CombatSystem.WeaponPiercingBridge
        /// </summary>
        private static bool? GetWeaponPiercing(string weaponType)
        {
            // Try to get from WeaponConfigDatabase (ScriptableObjects)
            if (WeaponConfigDatabase.Instance != null)
            {
                WeaponConfig config = WeaponConfigDatabase.Instance.GetWeaponConfig(weaponType);
                if (config != null)
                {
                    return config.piercing;
                }
            }
            
            // Return null to fall back to default (check weapon name for "Ice")
            return null;
        }
    }
}

