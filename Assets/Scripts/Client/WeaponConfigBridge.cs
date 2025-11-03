using System;
using ArenaGame.Shared.Systems;

namespace ArenaGame.Client
{
    /// <summary>
    /// Bridges Client HeroConfig data to Shared CombatSystem (weapon properties now in HeroConfig)
    /// </summary>
    public static class WeaponConfigBridge
    {
        private static bool bridgeRegistered = false;
        
        /// <summary>
        /// Registers the bridge function so Shared assembly can access Client HeroConfig piercing info
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
        /// Bridge function: Gets piercing status from HeroConfig (weapon properties merged)
        /// Called by Shared assembly via CombatSystem.WeaponPiercingBridge
        /// Note: This is a legacy bridge - piercing is now determined by hero type and stars
        /// </summary>
        private static bool? GetWeaponPiercing(string weaponType)
        {
            // IceArcher is always piercing (handled in CombatSystem)
            // Return null to fall back to default logic in CombatSystem
            return null;
        }
    }
}

