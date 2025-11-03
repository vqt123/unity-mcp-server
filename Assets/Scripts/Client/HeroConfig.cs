using UnityEngine;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// ScriptableObject configuration for heroes
    /// Contains hero stats and weapon properties (merged from WeaponConfig)
    /// </summary>
    [CreateAssetMenu(fileName = "NewHeroConfig", menuName = "ArenaGame/Hero Config", order = 2)]
    public class HeroConfigSO : ScriptableObject
    {
        [Header("Hero Info")]
        [Tooltip("Hero type name (e.g., 'Archer', 'Mage', 'Warrior')")]
        public string heroTypeName = "Hero";
        
        [Tooltip("Display name for the hero")]
        public string displayName = "";
        
        [Tooltip("Description of the hero")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Header("Combat Stats")]
        [Tooltip("Maximum health")]
        public float maxHealth = 150f;
        
        [Tooltip("Base damage")]
        public float damage = 100f;
        
        [Tooltip("Move speed (units per second)")]
        public float moveSpeed = 5f;
        
        [Tooltip("Attack speed (attacks per second) - Archer: 1.0, Others: 0.5")]
        public float attackSpeed = 1.0f;
        
        [Header("Weapon Properties")]
        [Tooltip("Weapon type name (e.g., 'Bow', 'Firewand', 'Sword')")]
        public string weaponType = "Bow";
        
        [Tooltip("Projectile speed (units per second)")]
        public float projectileSpeed = 45f;
        
        [Tooltip("Number of projectiles per shot (base)")]
        public int projectileCount = 1;
        
        [Tooltip("Area of effect radius (0 = single target)")]
        public float aoeRadius = 0f;
        
        [Tooltip("Whether projectiles pierce through enemies")]
        public bool piercing = false;
        
        [Tooltip("Bullet/projectile color")]
        public Color bulletColor = Color.white;
        
        [Header("Visual")]
        [Tooltip("Hero color (for UI/visuals)")]
        public Color heroColor = Color.white;
        
        [Tooltip("Hero prefab (optional - can use default)")]
        public GameObject heroPrefab;
        
        [Tooltip("Projectile prefab to instantiate")]
        public GameObject projectilePrefab;
        
        [Tooltip("ProjectileFX prefab (particle effects)")]
        public GameObject projectileFXPrefab;
        
        /// <summary>
        /// Gets the weapon type name
        /// </summary>
        public string GetWeaponType()
        {
            return weaponType;
        }
        
        /// <summary>
        /// Validates config
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(weaponType))
            {
                Debug.LogWarning($"[HeroConfig] {heroTypeName} is missing weapon type!");
                return false;
            }
            return true;
        }
    }
}

