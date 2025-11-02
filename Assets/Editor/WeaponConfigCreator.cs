using UnityEngine;
using UnityEditor;
using ArenaGame.Client;
using System.IO;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Utility to create weapon config assets quickly
    /// </summary>
    public class WeaponConfigCreator
    {
        private const string WEAPON_CONFIGS_PATH = "Assets/Resources/WeaponConfigs";
        
        [MenuItem("Tools/Create Weapon Configs/Archer Bow")]
        public static void CreateBowWeapon()
        {
            CreateWeaponConfig("Bow_Weapon", "Bow", "Fast single target arrows", 100f, 0.3f, 36f, 1, 0f, false);
        }
        
        [MenuItem("Tools/Create Weapon Configs/Mage Firewand")]
        public static void CreateFirewandWeapon()
        {
            CreateWeaponConfig("Firewand_Weapon", "Firewand", "Explosive fire magic", 100f, 0.6f, 30f, 1, 3.0f, false);
        }
        
        [MenuItem("Tools/Create Weapon Configs/Warrior Sword")]
        public static void CreateSwordWeapon()
        {
            CreateWeaponConfig("Sword_Weapon", "Sword", "Melee slash attack", 100f, 0.5f, 24f, 1, 2.5f, false);
        }
        
        [MenuItem("Tools/Create Weapon Configs/Pistol")]
        public static void CreatePistolWeapon()
        {
            CreateWeaponConfig("Pistol_Weapon", "Pistol", "Standard pistol", 100f, 0.3f, 45f, 1, 0f, false);
        }
        
        [MenuItem("Tools/Create Weapon Configs/SMG")]
        public static void CreateSMGWeapon()
        {
            CreateWeaponConfig("SMG_Weapon", "SMG", "Fast-firing submachine gun", 100f, 0.2f, 45f, 1, 0f, false);
        }
        
        [MenuItem("Tools/Create Weapon Configs/Shotgun")]
        public static void CreateShotgunWeapon()
        {
            CreateWeaponConfig("Shotgun_Weapon", "Shotgun", "Close-range spread weapon", 100f, 0.5f, 35f, 3, 1.5f, false);
        }
        
        [MenuItem("Tools/Create Weapon Configs/Custom Weapon")]
        public static void CreateCustomWeapon()
        {
            CreateWeaponConfig("Custom_Weapon", "CustomWeapon", "Custom weapon configuration", 100f, 0.3f, 45f, 1, 0f, false);
        }
        
        private static void CreateWeaponConfig(string fileName, string weaponName, string description, 
            float damage, float cooldown, float speed, int count, float aoe, bool piercing)
        {
            // Ensure directory exists
            if (!Directory.Exists(WEAPON_CONFIGS_PATH))
            {
                Directory.CreateDirectory(WEAPON_CONFIGS_PATH);
                AssetDatabase.Refresh();
            }
            
            // Create the asset
            WeaponConfig config = ScriptableObject.CreateInstance<WeaponConfig>();
            config.weaponName = weaponName;
            config.description = description;
            config.damage = damage;
            config.shootCooldown = cooldown;
            config.projectileSpeed = speed;
            config.projectileCount = count;
            config.aoeRadius = aoe;
            config.piercing = piercing;
            
            // Set default colors based on weapon
            switch (weaponName.ToLower())
            {
                case "bow":
                    config.bulletColor = new Color(0.8f, 0.6f, 0.2f, 1f); // Yellow/Orange
                    break;
                case "firewand":
                    config.bulletColor = new Color(1f, 0.3f, 0f, 1f); // Red/Orange
                    break;
                case "sword":
                    config.bulletColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Gray
                    break;
                case "pistol":
                    config.bulletColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray/white
                    break;
                case "smg":
                    config.bulletColor = new Color(0.8f, 0.8f, 0.3f, 1f); // Yellow/green
                    break;
                case "shotgun":
                    config.bulletColor = new Color(0.6f, 0.4f, 0.2f, 1f); // Brown
                    break;
            }
            
            string path = $"{WEAPON_CONFIGS_PATH}/{fileName}.asset";
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            Debug.Log($"[WeaponConfigCreator] Created weapon config: {path}");
        }
    }
}

