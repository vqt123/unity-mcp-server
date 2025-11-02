using UnityEngine;
using UnityEditor;
using ArenaGame.Client;
using System.IO;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Utility to create hero config assets quickly
    /// </summary>
    public class HeroConfigCreator
    {
        private const string HERO_CONFIGS_PATH = "Assets/Resources/HeroConfigs";
        
        [MenuItem("Tools/Create Hero Configs/Archer Hero")]
        public static void CreateArcherHero()
        {
            CreateHeroConfig("Archer_Hero", "Archer", "Archer", "Ranged hero with bow", 150f, 100f, 5f, 3.3f, Color.green);
        }
        
        [MenuItem("Tools/Create Hero Configs/Mage Hero")]
        public static void CreateMageHero()
        {
            CreateHeroConfig("Mage_Hero", "Mage", "Mage", "Magic hero with fire wand", 120f, 100f, 5f, 1.67f, new Color(0.5f, 0f, 1f, 1f));
        }
        
        [MenuItem("Tools/Create Hero Configs/Warrior Hero")]
        public static void CreateWarriorHero()
        {
            CreateHeroConfig("Warrior_Hero", "Warrior", "Warrior", "Melee hero with sword", 200f, 100f, 5f, 2f, Color.red);
        }
        
        [MenuItem("Tools/Create Hero Configs/Custom Hero")]
        public static void CreateCustomHero()
        {
            CreateHeroConfig("Custom_Hero", "CustomHero", "Custom Hero", "Custom hero configuration", 150f, 100f, 5f, 3.3f, Color.white);
        }
        
        private static void CreateHeroConfig(string fileName, string heroType, string displayName, string description,
            float health, float damage, float moveSpeed, float attackSpeed, Color color)
        {
            // Ensure directory exists
            if (!Directory.Exists(HERO_CONFIGS_PATH))
            {
                Directory.CreateDirectory(HERO_CONFIGS_PATH);
                AssetDatabase.Refresh();
            }
            
            // Create the asset
            HeroConfigSO config = ScriptableObject.CreateInstance<HeroConfigSO>();
            config.heroTypeName = heroType;
            config.displayName = string.IsNullOrEmpty(displayName) ? heroType : displayName;
            config.description = description;
            config.maxHealth = health;
            config.damage = damage;
            config.moveSpeed = moveSpeed;
            config.attackSpeed = attackSpeed;
            config.heroColor = color;
            
            // Try to find matching weapon config
            string weaponName = GetDefaultWeaponForHero(heroType);
            WeaponConfig weaponConfig = FindWeaponConfig(weaponName);
            if (weaponConfig != null)
            {
                config.weaponConfig = weaponConfig;
                Debug.Log($"[HeroConfigCreator] Auto-assigned weapon config '{weaponName}' to {heroType}");
            }
            else
            {
                Debug.LogWarning($"[HeroConfigCreator] Could not find weapon config '{weaponName}' for {heroType}. Please assign manually.");
            }
            
            string path = $"{HERO_CONFIGS_PATH}/{fileName}.asset";
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            Debug.Log($"[HeroConfigCreator] Created hero config: {path}");
        }
        
        public static string GetDefaultWeaponForHero(string heroType)
        {
            switch (heroType.ToLower())
            {
                case "archer":
                    return "Bow";
                case "mage":
                    return "Firewand";
                case "warrior":
                    return "Sword";
                default:
                    return "Bow"; // Default
            }
        }
        
        private static WeaponConfig FindWeaponConfig(string weaponName)
        {
            // Try to find weapon config in Resources
            WeaponConfig[] configs = Resources.LoadAll<WeaponConfig>("WeaponConfigs");
            foreach (WeaponConfig config in configs)
            {
                if (config != null && config.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return config;
                }
            }
            return null;
        }
    }
}

