using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages persistent player data (hero inventory, party, etc)
    /// </summary>
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }
        
        private HeroInventoryData heroInventory;
        private const string SAVE_KEY = "PlayerData_HeroInventory";
        
        public HeroInventoryData HeroInventory => heroInventory;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadData();
        }
        
        private void LoadData()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                heroInventory = JsonUtility.FromJson<HeroInventoryData>(json);
                Debug.Log($"[PlayerData] Loaded hero inventory: {heroInventory.unlockedHeroes.Count} heroes, {heroInventory.partyHeroes.Count} in party");
            }
            else
            {
                // First time - create default data
                heroInventory = HeroInventoryData.CreateDefault();
                SaveData();
                Debug.Log("[PlayerData] Created default hero inventory");
            }
        }
        
        public void SaveData()
        {
            string json = JsonUtility.ToJson(heroInventory);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[PlayerData] Saved hero inventory");
        }
        
        public void ResetData()
        {
            heroInventory = HeroInventoryData.CreateDefault();
            SaveData();
            Debug.Log("[PlayerData] Reset to default data");
        }
    }
}

