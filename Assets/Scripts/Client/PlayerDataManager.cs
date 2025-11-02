using System.IO;
using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages persistent player data (hero inventory, party, etc)
    /// Uses JSON file system for save/load blobs
    /// </summary>
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }
        
        private PlayerBlob playerBlob;
        
        // File path for player blob
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "PlayerData", "player_blob.json");
        
        public HeroInventoryData HeroInventory => playerBlob?.heroInventory;
        public PlayerBlob PlayerBlob => playerBlob;
        public int TotalGold => playerBlob?.totalGold ?? 0;
        
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
        
        /// <summary>
        /// Loads player blob from JSON file
        /// </summary>
        private void LoadData()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    playerBlob = JsonUtility.FromJson<PlayerBlob>(json);
                    
                    if (playerBlob == null || playerBlob.heroInventory == null)
                    {
                        Debug.LogWarning("[PlayerData] Loaded blob was invalid, creating default");
                        playerBlob = PlayerBlob.CreateDefault();
                        SaveData();
                    }
                    else
                    {
                        Debug.Log($"[PlayerData] Loaded player blob: {playerBlob.heroInventory.unlockedHeroes.Count} heroes unlocked, {playerBlob.heroInventory.partyHeroes.Count} in party, {playerBlob.totalGold} gold");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerData] Failed to load player blob: {e.Message}");
                    playerBlob = PlayerBlob.CreateDefault();
                    SaveData();
                }
            }
            else
            {
                // First time - create default data with Archer
                playerBlob = PlayerBlob.CreateDefault();
                SaveData();
                Debug.Log("[PlayerData] Created default player blob with Archer hero");
            }
        }
        
        /// <summary>
        /// Saves player blob to JSON file
        /// </summary>
        public void SaveData()
        {
            if (playerBlob == null)
            {
                Debug.LogWarning("[PlayerData] Cannot save - player blob is null");
                return;
            }
            
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(SaveFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string json = JsonUtility.ToJson(playerBlob, prettyPrint: true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[PlayerData] Saved player blob to: {SaveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerData] Failed to save player blob: {e.Message}");
            }
        }
        
        /// <summary>
        /// Resets player data to default (Archer only)
        /// </summary>
        public void ResetData()
        {
            playerBlob = PlayerBlob.CreateDefault();
            SaveData();
            Debug.Log("[PlayerData] Reset to default player blob");
        }
        
        /// <summary>
        /// Updates total gold and saves
        /// </summary>
        public void SetTotalGold(int gold)
        {
            if (playerBlob != null)
            {
                playerBlob.totalGold = gold;
                SaveData();
            }
        }
        
        /// <summary>
        /// Adds gold and saves
        /// </summary>
        public void AddGold(int amount)
        {
            if (playerBlob != null)
            {
                playerBlob.totalGold += amount;
                SaveData();
            }
        }
        
        /// <summary>
        /// Gets hero progress data
        /// </summary>
        public HeroProgressData GetHeroProgress(string heroType)
        {
            return playerBlob?.GetHeroProgress(heroType);
        }
    }
}

