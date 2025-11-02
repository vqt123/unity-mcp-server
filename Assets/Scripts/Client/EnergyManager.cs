using UnityEngine;
using System;
using System.Collections;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages player energy - regenerates over time and tracks energy consumption
    /// Energy regenerates at 1 per minute, max 30
    /// </summary>
    public class EnergyManager : MonoBehaviour
    {
        private static EnergyManager _instance;
        public static EnergyManager Instance => _instance;
        
        private const int MAX_ENERGY = 30;
        private const float REGENERATION_INTERVAL = 60f; // 1 per minute = 60 seconds
        
        private int currentEnergy = MAX_ENERGY;
        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => MAX_ENERGY;
        
        public event Action<int> OnEnergyChanged;
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadEnergyFromPlayerData();
            
            // Start regeneration coroutine
            StartCoroutine(RegenerateEnergyCoroutine());
            
            Debug.Log($"[EnergyManager] Initialized with {currentEnergy}/{MAX_ENERGY} energy");
        }
        
        private void LoadEnergyFromPlayerData()
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.PlayerBlob != null)
            {
                var blob = PlayerDataManager.Instance.PlayerBlob;
                currentEnergy = blob.currentEnergy;
                
                // Calculate energy regenerated while away
                if (blob.lastEnergyRegenTime > 0)
                {
                    double now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    double timePassed = now - blob.lastEnergyRegenTime;
                    
                    // Regenerate 1 energy per 60 seconds
                    int energyToRegen = Mathf.FloorToInt((float)(timePassed / REGENERATION_INTERVAL));
                    
                    if (energyToRegen > 0)
                    {
                        currentEnergy = Mathf.Min(MAX_ENERGY, currentEnergy + energyToRegen);
                        blob.currentEnergy = currentEnergy;
                        blob.lastEnergyRegenTime = now;
                        PlayerDataManager.Instance.SaveData();
                        
                        Debug.Log($"[EnergyManager] Regenerated {energyToRegen} energy while away (had {blob.currentEnergy - energyToRegen}, now {currentEnergy})");
                    }
                }
                else
                {
                    // First time - set timestamp
                    blob.lastEnergyRegenTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    PlayerDataManager.Instance.SaveData();
                }
                
                OnEnergyChanged?.Invoke(currentEnergy);
                Debug.Log($"[EnergyManager] Loaded {currentEnergy}/{MAX_ENERGY} energy from PlayerDataManager");
            }
            else
            {
                currentEnergy = MAX_ENERGY;
                Debug.Log("[EnergyManager] PlayerDataManager not available, using default energy");
            }
        }
        
        /// <summary>
        /// Coroutine that regenerates energy every minute
        /// </summary>
        private IEnumerator RegenerateEnergyCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(REGENERATION_INTERVAL);
                
                if (currentEnergy < MAX_ENERGY)
                {
                    currentEnergy++;
                    SaveEnergyToPlayerData();
                    OnEnergyChanged?.Invoke(currentEnergy);
                    Debug.Log($"[EnergyManager] Regenerated 1 energy: {currentEnergy}/{MAX_ENERGY}");
                }
            }
        }
        
        /// <summary>
        /// Spends energy and returns true if successful
        /// </summary>
        public bool SpendEnergy(int amount)
        {
            if (currentEnergy < amount)
            {
                Debug.LogWarning($"[EnergyManager] Not enough energy! Have {currentEnergy}, need {amount}");
                return false;
            }
            
            currentEnergy -= amount;
            SaveEnergyToPlayerData();
            OnEnergyChanged?.Invoke(currentEnergy);
            Debug.Log($"[EnergyManager] Spent {amount} energy: {currentEnergy}/{MAX_ENERGY}");
            return true;
        }
        
        /// <summary>
        /// Adds energy (for rewards, etc.)
        /// </summary>
        public void AddEnergy(int amount)
        {
            if (amount <= 0) return;
            
            currentEnergy = Mathf.Min(MAX_ENERGY, currentEnergy + amount);
            SaveEnergyToPlayerData();
            OnEnergyChanged?.Invoke(currentEnergy);
            Debug.Log($"[EnergyManager] Added {amount} energy: {currentEnergy}/{MAX_ENERGY}");
        }
        
        /// <summary>
        /// Sets energy directly
        /// </summary>
        public void SetEnergy(int amount)
        {
            currentEnergy = Mathf.Clamp(amount, 0, MAX_ENERGY);
            SaveEnergyToPlayerData();
            OnEnergyChanged?.Invoke(currentEnergy);
        }
        
        private void SaveEnergyToPlayerData()
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.PlayerBlob != null)
            {
                var blob = PlayerDataManager.Instance.PlayerBlob;
                blob.currentEnergy = currentEnergy;
                blob.maxEnergy = MAX_ENERGY;
                blob.lastEnergyRegenTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                PlayerDataManager.Instance.SaveData();
            }
        }
    }
}

