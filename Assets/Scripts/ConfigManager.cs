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
}

