using UnityEngine;
using UnityEditor;
using ArenaGame.Client;
using System.IO;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Utility to create all hero configs at once
    /// </summary>
    public class CreateAllHeroConfigs
    {
        private const string HERO_CONFIGS_PATH = "Assets/Resources/HeroConfigs";
        
        [MenuItem("Tools/Create All Hero Configs")]
        public static void CreateAllConfigs()
        {
            // Ensure directory exists
            if (!Directory.Exists(HERO_CONFIGS_PATH))
            {
                Directory.CreateDirectory(HERO_CONFIGS_PATH);
                AssetDatabase.Refresh();
            }
            
            int created = 0;
            int updated = 0;
            
            // Create Archer Hero
            if (CreateOrUpdateHeroConfig("Archer", "Archer", "Ranged hero with bow", 
                150f, 100f, 5f, 3.3f, Color.green, "Bow"))
            {
                created++;
            }
            else
            {
                updated++;
            }
            
            // Create Mage Hero
            if (CreateOrUpdateHeroConfig("Mage", "Mage", "Magic hero with fire wand", 
                120f, 100f, 5f, 1.67f, new Color(0.5f, 0f, 1f, 1f), "Firewand"))
            {
                created++;
            }
            else
            {
                updated++;
            }
            
            // Create Warrior Hero
            if (CreateOrUpdateHeroConfig("Warrior", "Warrior", "Melee hero with sword", 
                200f, 100f, 5f, 2.0f, Color.red, "Sword"))
            {
                created++;
            }
            else
            {
                updated++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Complete", 
                $"Created/Updated Hero Configs:\n" +
                $"Created: {created}\n" +
                $"Updated: {updated}\n\n" +
                "All hero configs are in Assets/Resources/HeroConfigs/",
                "OK");
            
            Debug.Log($"[CreateAllHeroConfigs] Created {created} and updated {updated} hero configs");
        }
        
        private static bool CreateOrUpdateHeroConfig(string fileName, string heroType, string description,
            float health, float damage, float moveSpeed, float attackSpeed, Color color, string weaponName)
        {
            string path = $"{HERO_CONFIGS_PATH}/{fileName}_Hero.asset";
            bool isNew = !File.Exists(path);
            
            HeroConfigSO config;
            
            if (isNew)
            {
                config = ScriptableObject.CreateInstance<HeroConfigSO>();
            }
            else
            {
                config = AssetDatabase.LoadAssetAtPath<HeroConfigSO>(path);
                if (config == null)
                {
                    // File exists but asset doesn't load - recreate it
                    config = ScriptableObject.CreateInstance<HeroConfigSO>();
                    isNew = true;
                }
            }
            
            // Set all properties
            config.heroTypeName = heroType;
            config.displayName = heroType;
            config.description = description;
            config.maxHealth = health;
            config.damage = damage;
            config.moveSpeed = moveSpeed;
            config.attackSpeed = attackSpeed;
            config.heroColor = color;
            
            // Find and assign weapon config
            WeaponConfig weaponConfig = FindWeaponConfig(weaponName);
            if (weaponConfig != null)
            {
                config.weaponConfig = weaponConfig;
                Debug.Log($"[CreateAllHeroConfigs] Assigned weapon config '{weaponName}' to {heroType}");
            }
            else
            {
                Debug.LogWarning($"[CreateAllHeroConfigs] Could not find weapon config '{weaponName}' for {heroType}. Please assign manually.");
            }
            
            if (isNew)
            {
                AssetDatabase.CreateAsset(config, path);
                Debug.Log($"[CreateAllHeroConfigs] Created new hero config: {path}");
            }
            else
            {
                EditorUtility.SetDirty(config);
                Debug.Log($"[CreateAllHeroConfigs] Updated existing hero config: {path}");
            }
            
            return isNew;
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
            
            // Also search in Assets folder using AssetDatabase
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(WeaponConfig).Name}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                WeaponConfig config = AssetDatabase.LoadAssetAtPath<WeaponConfig>(assetPath);
                if (config != null && config.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return config;
                }
            }
            
            return null;
        }
    }
}

