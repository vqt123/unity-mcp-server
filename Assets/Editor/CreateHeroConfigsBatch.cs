using UnityEngine;
using UnityEditor;
using ArenaGame.Client;
using System.IO;
using System.Linq;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Batch creates hero configs - can be executed programmatically
    /// </summary>
    public static class CreateHeroConfigsBatch
    {
        [MenuItem("Tools/Create All Hero Configs (Batch)")]
        public static void Execute()
        {
            const string HERO_CONFIGS_PATH = "Assets/Resources/HeroConfigs";
            
            // Ensure directory exists
            if (!Directory.Exists(HERO_CONFIGS_PATH))
            {
                Directory.CreateDirectory(HERO_CONFIGS_PATH);
                AssetDatabase.Refresh();
            }
            
            // Hero definitions
            var heroDefs = new[]
            {
                new { name = "DefaultHero", displayName = "Default Hero", desc = "Standard balanced hero", 
                      health = 150f, damage = 100f, moveSpeed = 5f, attackSpeed = 3.3f, 
                      color = Color.white, weapon = "Pistol" },
                new { name = "FastHero", displayName = "Fast Hero", desc = "High mobility hero", 
                      health = 150f, damage = 100f, moveSpeed = 7f, attackSpeed = 3.3f, 
                      color = Color.cyan, weapon = "SMG" },
                new { name = "TankHero", displayName = "Tank Hero", desc = "High health tank hero", 
                      health = 200f, damage = 100f, moveSpeed = 5f, attackSpeed = 2.0f, 
                      color = Color.blue, weapon = "Shotgun" },
                new { name = "Archer", displayName = "Archer", desc = "Ranged hero with bow", 
                      health = 150f, damage = 100f, moveSpeed = 5f, attackSpeed = 3.3f, 
                      color = Color.green, weapon = "Bow" },
                new { name = "Mage", displayName = "Mage", desc = "Magic hero with fire wand", 
                      health = 120f, damage = 100f, moveSpeed = 5f, attackSpeed = 1.67f, 
                      color = new Color(0.5f, 0f, 1f, 1f), weapon = "Firewand" },
                new { name = "Warrior", displayName = "Warrior", desc = "Melee hero with sword", 
                      health = 200f, damage = 100f, moveSpeed = 5f, attackSpeed = 2.0f, 
                      color = Color.red, weapon = "Sword" }
            };
            
            int created = 0;
            int updated = 0;
            
            foreach (var heroDef in heroDefs)
            {
                string path = $"{HERO_CONFIGS_PATH}/{heroDef.name}_Hero.asset";
                bool exists = File.Exists(path);
                
                HeroConfigSO config;
                if (exists)
                {
                    config = AssetDatabase.LoadAssetAtPath<HeroConfigSO>(path);
                    if (config == null)
                    {
                        config = ScriptableObject.CreateInstance<HeroConfigSO>();
                        exists = false;
                    }
                }
                else
                {
                    config = ScriptableObject.CreateInstance<HeroConfigSO>();
                }
                
                // Configure the hero
                config.heroTypeName = heroDef.name;
                config.displayName = heroDef.displayName;
                config.description = heroDef.desc;
                config.maxHealth = heroDef.health;
                config.damage = heroDef.damage;
                config.moveSpeed = heroDef.moveSpeed;
                config.attackSpeed = heroDef.attackSpeed;
                config.heroColor = heroDef.color;
                
                // Find weapon config
                WeaponConfig weaponConfig = FindWeaponConfig(heroDef.weapon);
                if (weaponConfig != null)
                {
                    config.weaponConfig = weaponConfig;
                }
                else
                {
                    Debug.LogWarning($"[CreateHeroConfigsBatch] Weapon config '{heroDef.weapon}' not found for {heroDef.name}");
                }
                
                if (!exists)
                {
                    AssetDatabase.CreateAsset(config, path);
                    created++;
                    Debug.Log($"[CreateHeroConfigsBatch] Created {heroDef.name}_Hero.asset");
                }
                else
                {
                    EditorUtility.SetDirty(config);
                    updated++;
                    Debug.Log($"[CreateHeroConfigsBatch] Updated {heroDef.name}_Hero.asset");
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[CreateHeroConfigsBatch] Complete! Created: {created}, Updated: {updated}");
        }
        
        private static WeaponConfig FindWeaponConfig(string weaponName)
        {
            // Search in Resources
            WeaponConfig[] resourcesConfigs = Resources.LoadAll<WeaponConfig>("WeaponConfigs");
            WeaponConfig found = resourcesConfigs.FirstOrDefault(c => 
                c != null && c.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase));
            
            if (found != null) return found;
            
            // Search using AssetDatabase
            string[] guids = AssetDatabase.FindAssets($"t:WeaponConfig");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                WeaponConfig config = AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
                if (config != null && config.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return config;
                }
            }
            
            return null;
        }
    }
}

