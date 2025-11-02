using UnityEngine;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// ScriptableObject configuration for weapons
    /// Contains stats and visual prefab references
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "ArenaGame/Weapon Config", order = 1)]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Weapon Info")]
        [Tooltip("Name of the weapon (e.g., 'Bow', 'Firewand', 'Sword')")]
        public string weaponName = "Weapon";
        
        [Tooltip("Description of the weapon")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Header("Combat Stats")]
        [Tooltip("Base damage dealt by this weapon")]
        public float damage = 100f;
        
        [Tooltip("Cooldown between shots (seconds)")]
        public float shootCooldown = 0.3f;
        
        [Tooltip("Projectile speed (units per second)")]
        public float projectileSpeed = 45f;
        
        [Tooltip("Number of projectiles per shot")]
        public int projectileCount = 1;
        
        [Tooltip("Area of effect radius (0 = single target)")]
        public float aoeRadius = 0f;
        
        [Tooltip("Whether projectiles pierce through enemies")]
        public bool piercing = false;
        
        [Header("Visual")]
        [Tooltip("Projectile prefab to instantiate")]
        public GameObject projectilePrefab;
        
        [Tooltip("ProjectileFX prefab (particle effects)")]
        public GameObject projectileFXPrefab;
        
        [Tooltip("Bullet/projectile color")]
        public Color bulletColor = Color.white;
        
        [Header("Weapon Tiers (Optional)")]
        [Tooltip("If true, this weapon supports multiple tiers with upgrades")]
        public bool hasTiers = false;
        
        [Tooltip("Tier configurations (for upgrade progression)")]
        public WeaponTierConfig[] tiers;
        
        /// <summary>
        /// Gets the projectile speed as Fix64 for simulation
        /// </summary>
        public Fix64 GetProjectileSpeed()
        {
            return Fix64.FromFloat(projectileSpeed);
        }
        
        /// <summary>
        /// Gets damage as Fix64 for simulation
        /// </summary>
        public Fix64 GetDamage()
        {
            return Fix64.FromFloat(damage);
        }
        
        /// <summary>
        /// Gets AOE radius as Fix64 for simulation
        /// </summary>
        public Fix64 GetAoeRadius()
        {
            return Fix64.FromFloat(aoeRadius);
        }
        
        /// <summary>
        /// Gets the tier config for a specific tier (1-indexed)
        /// Returns base stats if no tier found
        /// </summary>
        public WeaponTierConfig GetTier(int tier)
        {
            if (!hasTiers || tiers == null || tier < 1 || tier > tiers.Length)
            {
                return null;
            }
            
            return tiers[tier - 1];
        }
    }
    
    /// <summary>
    /// Configuration for a specific weapon tier
    /// </summary>
    [System.Serializable]
    public class WeaponTierConfig
    {
        [Tooltip("Tier number (1, 2, 3, 4, etc.)")]
        public int tier;
        
        [Tooltip("Tier name (e.g., 'Wooden Bow', 'Elven Bow')")]
        public string tierName;
        
        [Tooltip("Damage multiplier for this tier")]
        public float damageMultiplier = 1f;
        
        [Tooltip("Cooldown multiplier (lower = faster)")]
        public float cooldownMultiplier = 1f;
        
        [Tooltip("Speed multiplier for this tier")]
        public float speedMultiplier = 1f;
        
        [Tooltip("Projectile count for this tier")]
        public int projectileCount = 1;
        
        [Tooltip("AOE radius for this tier")]
        public float aoeRadius = 0f;
        
        [Tooltip("Description of this tier")]
        [TextArea(1, 2)]
        public string description;
    }
}

