using UnityEngine;
using UnityEditor;
using ArenaGame.Client;
using System.IO;
using System.Linq;

namespace ArenaGame.Editor
{
    /// <summary>
    /// One-click setup for all weapon and hero configs
    /// </summary>
    public static class SetupAllConfigs
    {
        [MenuItem("Tools/Setup All Configs (Weapons + Heroes)")]
        public static void Execute()
        {
            ExecuteInternal(true);
        }
        
        public static void ExecuteInternal(bool showDialog = true)
        {
            // First ensure all weapon configs exist
            EnsureWeaponConfigs();
            
            // Then create all hero configs
            CreateHeroConfigsBatch.Execute();
            
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Complete", 
                    "All weapon and hero configs have been created/updated!\n\n" +
                    "Check Assets/Resources/WeaponConfigs/ and Assets/Resources/HeroConfigs/",
                    "OK");
            }
            else
            {
                Debug.Log("[SetupAllConfigs] All configs created/updated automatically!");
            }
        }
        
        private static void EnsureWeaponConfigs()
        {
            const string WEAPON_CONFIGS_PATH = "Assets/Resources/WeaponConfigs";
            
            if (!Directory.Exists(WEAPON_CONFIGS_PATH))
            {
                Directory.CreateDirectory(WEAPON_CONFIGS_PATH);
                AssetDatabase.Refresh();
            }
            
            // Check and create missing weapon configs
            var weaponDefs = new[]
            {
                new { name = "Bow", desc = "Fast single target arrows", 
                      damage = 100f, cooldown = 0.3f, speed = 36f, count = 1, aoe = 0f, piercing = false,
                      color = new Color(0.8f, 0.6f, 0.2f, 1f) },
                new { name = "Firewand", desc = "Explosive fire magic", 
                      damage = 100f, cooldown = 0.6f, speed = 30f, count = 1, aoe = 3.0f, piercing = false,
                      color = new Color(1f, 0.3f, 0f, 1f) },
                new { name = "Sword", desc = "Melee slash attack", 
                      damage = 100f, cooldown = 0.5f, speed = 24f, count = 1, aoe = 2.5f, piercing = false,
                      color = new Color(0.7f, 0.7f, 0.7f, 1f) },
                new { name = "Pistol", desc = "Standard pistol", 
                      damage = 100f, cooldown = 0.3f, speed = 45f, count = 1, aoe = 0f, piercing = false,
                      color = new Color(0.9f, 0.9f, 0.9f, 1f) },
                new { name = "SMG", desc = "Fast-firing submachine gun", 
                      damage = 100f, cooldown = 0.2f, speed = 45f, count = 1, aoe = 0f, piercing = false,
                      color = new Color(0.8f, 0.8f, 0.3f, 1f) },
                new { name = "Shotgun", desc = "Close-range spread weapon", 
                      damage = 100f, cooldown = 0.5f, speed = 35f, count = 3, aoe = 1.5f, piercing = false,
                      color = new Color(0.6f, 0.4f, 0.2f, 1f) }
            };
            
            foreach (var weaponDef in weaponDefs)
            {
                string path = $"{WEAPON_CONFIGS_PATH}/{weaponDef.name}_Weapon.asset";
                if (File.Exists(path))
                {
                    WeaponConfig existing = AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
                    if (existing != null)
                    {
                        Debug.Log($"[SetupAllConfigs] Weapon config '{weaponDef.name}' already exists");
                        continue;
                    }
                }
                
                // Create new weapon config
                WeaponConfig config = ScriptableObject.CreateInstance<WeaponConfig>();
                config.weaponName = weaponDef.name;
                config.description = weaponDef.desc;
                config.damage = weaponDef.damage;
                config.shootCooldown = weaponDef.cooldown;
                config.projectileSpeed = weaponDef.speed;
                config.projectileCount = weaponDef.count;
                config.aoeRadius = weaponDef.aoe;
                config.piercing = weaponDef.piercing;
                config.bulletColor = weaponDef.color;
                
                AssetDatabase.CreateAsset(config, path);
                Debug.Log($"[SetupAllConfigs] Created weapon config: {weaponDef.name}_Weapon.asset");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

