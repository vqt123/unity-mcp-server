using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ArenaGame.Client
{
    /// <summary>
    /// Database for loading and managing hero configs
    /// Loads all HeroConfigSO ScriptableObjects from Resources
    /// </summary>
    public class HeroConfigDatabase : MonoBehaviour
    {
        private static HeroConfigDatabase _instance;
        public static HeroConfigDatabase Instance => _instance;
        
        private Dictionary<string, HeroConfigSO> heroConfigs = new Dictionary<string, HeroConfigSO>();
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            LoadHeroConfigs();
        }
        
        /// <summary>
        /// Loads all hero configs from Resources/HeroConfigs
        /// </summary>
        private void LoadHeroConfigs()
        {
            heroConfigs.Clear();
            
            // Load all HeroConfigSO assets from Resources
            HeroConfigSO[] configs = Resources.LoadAll<HeroConfigSO>("HeroConfigs");
            
            foreach (HeroConfigSO config in configs)
            {
                if (config != null && !string.IsNullOrEmpty(config.heroTypeName))
                {
                    // Use hero type name as key (case-insensitive)
                    string key = config.heroTypeName.ToLower();
                    heroConfigs[key] = config;
                    
                    // Validate config
                    if (!config.IsValid())
                    {
                        Debug.LogWarning($"[HeroConfigDatabase] Hero config '{config.heroTypeName}' is missing weapon type!");
                    }
                    else
                    {
                        Debug.Log($"[HeroConfigDatabase] Loaded hero config: {config.heroTypeName} with weapon: {config.GetWeaponType()}");
                    }
                }
            }
            
            Debug.Log($"[HeroConfigDatabase] Loaded {heroConfigs.Count} hero configs");
        }
        
        /// <summary>
        /// Gets a hero config by hero type name (case-insensitive)
        /// </summary>
        public HeroConfigSO GetHeroConfig(string heroTypeName)
        {
            if (string.IsNullOrEmpty(heroTypeName))
            {
                return null;
            }
            
            string key = heroTypeName.ToLower();
            if (heroConfigs.TryGetValue(key, out HeroConfigSO config))
            {
                return config;
            }
            
            Debug.LogWarning($"[HeroConfigDatabase] Hero config '{heroTypeName}' not found!");
            return null;
        }
        
        /// <summary>
        /// Gets all loaded hero configs
        /// </summary>
        public List<HeroConfigSO> GetAllHeroConfigs()
        {
            return heroConfigs.Values.ToList();
        }
        
        /// <summary>
        /// Reloads hero configs (useful for editor/testing)
        /// </summary>
        public void Reload()
        {
            LoadHeroConfigs();
        }
    }
}

