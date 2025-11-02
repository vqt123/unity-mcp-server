using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// ScriptableObject configuration for heroes
    /// Contains hero stats and references to weapon configs
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
        
        [Tooltip("Attack speed (attacks per second)")]
        public float attackSpeed = 3.3f;
        
        [Header("Weapon")]
        [Tooltip("Starting weapon config - assign a WeaponConfig here")]
        public WeaponConfig weaponConfig;
        
        [Tooltip("Hero color (for UI/visuals)")]
        public Color heroColor = Color.white;
        
        [Header("Visual")]
        [Tooltip("Hero prefab (optional - can use default)")]
        public GameObject heroPrefab;
        
        /// <summary>
        /// Gets the weapon type name from the weapon config
        /// Falls back to weaponConfig.weaponName if available
        /// </summary>
        public string GetWeaponType()
        {
            if (weaponConfig != null && !string.IsNullOrEmpty(weaponConfig.weaponName))
            {
                return weaponConfig.weaponName;
            }
            return "Bow"; // Default fallback
        }
        
        /// <summary>
        /// Validates that a weapon config is assigned
        /// </summary>
        public bool IsValid()
        {
            if (weaponConfig == null)
            {
                Debug.LogWarning($"[HeroConfig] {heroTypeName} is missing weapon config!");
                return false;
            }
            return true;
        }
    }
}

