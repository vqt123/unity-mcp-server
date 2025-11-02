using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ArenaGame.Client
{
    /// <summary>
    /// Database for loading and managing weapon configs
    /// Loads all WeaponConfig ScriptableObjects from Resources
    /// </summary>
    public class WeaponConfigDatabase : MonoBehaviour
    {
        private static WeaponConfigDatabase _instance;
        public static WeaponConfigDatabase Instance => _instance;
        
        private Dictionary<string, WeaponConfig> weaponConfigs = new Dictionary<string, WeaponConfig>();
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            LoadWeaponConfigs();
        }
        
        /// <summary>
        /// Loads all weapon configs from Resources/WeaponConfigs
        /// </summary>
        private void LoadWeaponConfigs()
        {
            weaponConfigs.Clear();
            
            // Load all WeaponConfig assets from Resources
            WeaponConfig[] configs = Resources.LoadAll<WeaponConfig>("WeaponConfigs");
            
            foreach (WeaponConfig config in configs)
            {
                if (config != null && !string.IsNullOrEmpty(config.weaponName))
                {
                    // Use weapon name as key (case-insensitive)
                    string key = config.weaponName.ToLower();
                    weaponConfigs[key] = config;
                    Debug.Log($"[WeaponConfigDatabase] Loaded weapon config: {config.weaponName}");
                }
            }
            
            Debug.Log($"[WeaponConfigDatabase] Loaded {weaponConfigs.Count} weapon configs");
        }
        
        /// <summary>
        /// Gets a weapon config by name (case-insensitive)
        /// </summary>
        public WeaponConfig GetWeaponConfig(string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName))
            {
                return null;
            }
            
            string key = weaponName.ToLower();
            if (weaponConfigs.TryGetValue(key, out WeaponConfig config))
            {
                return config;
            }
            
            Debug.LogWarning($"[WeaponConfigDatabase] Weapon config '{weaponName}' not found!");
            return null;
        }
        
        /// <summary>
        /// Gets all loaded weapon configs
        /// </summary>
        public List<WeaponConfig> GetAllWeaponConfigs()
        {
            return weaponConfigs.Values.ToList();
        }
        
        /// <summary>
        /// Reloads weapon configs (useful for editor/testing)
        /// </summary>
        public void Reload()
        {
            LoadWeaponConfigs();
        }
    }
}

