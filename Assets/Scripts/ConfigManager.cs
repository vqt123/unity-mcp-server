using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ColorData
{
    public float r;
    public float g;
    public float b;
    public float a;
    
    public Color ToColor()
    {
        return new Color(r, g, b, a);
    }
}

[System.Serializable]
public class HeroData
{
    public string type;
    public float maxHealth;
    public float damage;
    public float shootCooldown;
    public float bulletSpeed;
    public string startingWeapon;
    public ColorData color;
}

[System.Serializable]
public class HeroTypesConfig
{
    public List<HeroData> heroes;
}

    [System.Serializable]
    public class WeaponTierData
    {
        public int tier;
        public string name;
        public float damage;
        public float shootCooldown;
        public float bulletSpeed;
        public int projectileCount;
        public float aoeRadius;
        public bool piercing;
        public string description;
    }

[System.Serializable]
public class WeaponData
{
    public string name;
    public ColorData bulletColor;
    public List<WeaponTierData> tiers;
}

[System.Serializable]
public class WeaponTypesConfig
{
    public List<WeaponData> weapons;
}

[System.Serializable]
public class EnemySpawnData
{
    public string enemyType;           // Type of enemy to spawn
    public int count;                  // Number of this enemy type to spawn
    public float health;               // Health multiplier
    public float damage;               // Damage multiplier
    public float moveSpeed;            // Move speed multiplier
}

[System.Serializable]
public class WaveData
{
    public int waveNumber;             // Wave identifier
    public float duration;             // How long the wave lasts (seconds)
    public float spawnInterval;        // Time between spawns (seconds)
    public List<EnemySpawnData> enemies; // Enemies to spawn in this wave
    public bool isBossWave;            // Is this a boss wave?
    public bool isMiniBossWave;        // Is this a mini-boss wave?
    public string bossType;            // Boss type if applicable
}

[System.Serializable]
public class LevelData
{
    public int levelNumber;            // Level identifier
    public string levelName;           // Display name
    public float difficultyMultiplier; // Global difficulty multiplier
    public List<WaveData> waves;       // All waves in this level
}

[System.Serializable]
public class LevelsConfig
{
    public List<LevelData> levels;
}

public class ConfigManager : MonoBehaviour
{
    private static ConfigManager _instance;
    public static ConfigManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ConfigManager");
                _instance = go.AddComponent<ConfigManager>();
                DontDestroyOnLoad(go);
                _instance.LoadConfigs();
            }
            return _instance;
        }
    }
    
    private HeroTypesConfig heroTypes;
    private WeaponTypesConfig weaponTypes;
    private LevelsConfig levelsConfig;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfigs();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadConfigs()
    {
        // Load hero types
        TextAsset heroJson = Resources.Load<TextAsset>("HeroTypes");
        if (heroJson != null)
        {
            heroTypes = JsonConvert.DeserializeObject<HeroTypesConfig>(heroJson.text);
            Debug.Log($"[ConfigManager] Loaded {heroTypes.heroes.Count} hero types");
        }
        else
        {
            Debug.LogError("[ConfigManager] Failed to load HeroTypes.json");
        }
        
        // Load weapon types
        TextAsset weaponJson = Resources.Load<TextAsset>("WeaponTypes");
        if (weaponJson != null)
        {
            weaponTypes = JsonConvert.DeserializeObject<WeaponTypesConfig>(weaponJson.text);
            Debug.Log($"[ConfigManager] Loaded {weaponTypes.weapons.Count} weapon types");
        }
        else
        {
            Debug.LogError("[ConfigManager] Failed to load WeaponTypes.json");
        }
        
        // Load levels
        TextAsset levelsJson = Resources.Load<TextAsset>("Levels");
        if (levelsJson != null)
        {
            levelsConfig = JsonConvert.DeserializeObject<LevelsConfig>(levelsJson.text);
            Debug.Log($"[ConfigManager] Loaded {levelsConfig.levels.Count} levels");
        }
        else
        {
            Debug.LogError("[ConfigManager] Failed to load Levels.json");
        }
    }
    
    public HeroData GetHeroData(string heroType)
    {
        return heroTypes?.heroes.FirstOrDefault(h => h.type == heroType);
    }
    
    public WeaponData GetWeaponData(string weaponName)
    {
        return weaponTypes?.weapons.FirstOrDefault(w => w.name == weaponName);
    }
    
    public WeaponTierData GetWeaponTier(string weaponName, int tier)
    {
        WeaponData weapon = GetWeaponData(weaponName);
        return weapon?.tiers.FirstOrDefault(t => t.tier == tier);
    }
    
    public List<HeroData> GetAllHeroes()
    {
        return heroTypes?.heroes ?? new List<HeroData>();
    }
    
    public List<WeaponData> GetAllWeapons()
    {
        return weaponTypes?.weapons ?? new List<WeaponData>();
    }
    
    public LevelData GetLevelData(int levelNumber)
    {
        return levelsConfig?.levels.FirstOrDefault(l => l.levelNumber == levelNumber);
    }
    
    public List<LevelData> GetAllLevels()
    {
        return levelsConfig?.levels ?? new List<LevelData>();
    }
}

