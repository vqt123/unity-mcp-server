using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ArenaGame.Client
{
    /// <summary>
    /// Database for loading and managing enemy configs
    /// Loads all EnemyConfigSO ScriptableObjects from Resources
    /// </summary>
    public class EnemyConfigDatabase : MonoBehaviour
    {
        private static EnemyConfigDatabase _instance;
        public static EnemyConfigDatabase Instance => _instance;
        
        private Dictionary<string, EnemyConfigSO> enemyConfigs = new Dictionary<string, EnemyConfigSO>();
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            LoadEnemyConfigs();
        }
        
        /// <summary>
        /// Loads all enemy configs from Resources/EnemyConfigs
        /// </summary>
        private void LoadEnemyConfigs()
        {
            enemyConfigs.Clear();
            
            // Load all EnemyConfigSO assets from Resources
            EnemyConfigSO[] configs = Resources.LoadAll<EnemyConfigSO>("EnemyConfigs");
            
            foreach (EnemyConfigSO config in configs)
            {
                if (config != null && !string.IsNullOrEmpty(config.enemyTypeName))
                {
                    // Use enemy type name as key (case-insensitive)
                    string key = config.enemyTypeName.ToLower();
                    enemyConfigs[key] = config;
                    
                    // Validate config
                    if (!config.IsValid())
                    {
                        Debug.LogWarning($"[EnemyConfigDatabase] Enemy config '{config.enemyTypeName}' is invalid!");
                    }
                    else
                    {
                        Debug.Log($"[EnemyConfigDatabase] Loaded enemy config: {config.enemyTypeName}" + 
                            (config.enemyPrefab != null ? $" with prefab: {config.enemyPrefab.name}" : " (no prefab)"));
                    }
                }
            }
            
            Debug.Log($"[EnemyConfigDatabase] Loaded {enemyConfigs.Count} enemy configs");
        }
        
        /// <summary>
        /// Gets an enemy config by enemy type name (case-insensitive)
        /// </summary>
        public EnemyConfigSO GetEnemyConfig(string enemyTypeName)
        {
            if (string.IsNullOrEmpty(enemyTypeName))
            {
                return null;
            }
            
            string key = enemyTypeName.ToLower();
            if (enemyConfigs.TryGetValue(key, out EnemyConfigSO config))
            {
                return config;
            }
            
            Debug.LogWarning($"[EnemyConfigDatabase] Enemy config '{enemyTypeName}' not found!");
            return null;
        }
        
        /// <summary>
        /// Gets all loaded enemy configs
        /// </summary>
        public List<EnemyConfigSO> GetAllEnemyConfigs()
        {
            return enemyConfigs.Values.ToList();
        }
        
        /// <summary>
        /// Reloads enemy configs (useful for editor/testing)
        /// </summary>
        public void Reload()
        {
            LoadEnemyConfigs();
        }
    }
}

