using UnityEngine;
using System;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages player gems currency
    /// </summary>
    public class GemsManager : MonoBehaviour
    {
        private static GemsManager _instance;
        public static GemsManager Instance => _instance;
        
        private int currentGems = 0;
        public int CurrentGems => currentGems;
        
        public event Action<int> OnGemsChanged;
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Load gems from PlayerDataManager if available
            if (PlayerDataManager.Instance != null)
            {
                currentGems = PlayerDataManager.Instance.TotalGems;
                Debug.Log($"[GemsManager] Loaded {currentGems} gems from PlayerDataManager");
            }
            else
            {
                Debug.Log("[GemsManager] Awake - Instance created (PlayerDataManager not available yet)");
            }
        }
        
        void Start()
        {
            // Sync with PlayerDataManager after it's initialized
            if (PlayerDataManager.Instance != null)
            {
                SyncWithPlayerData();
            }
        }
        
        private void SyncWithPlayerData()
        {
            if (PlayerDataManager.Instance != null)
            {
                int savedGems = PlayerDataManager.Instance.TotalGems;
                if (savedGems != currentGems)
                {
                    currentGems = savedGems;
                    OnGemsChanged?.Invoke(currentGems);
                }
            }
        }
        
        /// <summary>
        /// Adds gems and saves to PlayerDataManager
        /// </summary>
        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            
            currentGems += amount;
            
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddGems(amount);
            }
            
            OnGemsChanged?.Invoke(currentGems);
            Debug.Log($"[GemsManager] Added {amount} gems. Total: {currentGems}");
        }
        
        /// <summary>
        /// Spends gems and returns true if successful
        /// </summary>
        public bool SpendGems(int amount)
        {
            if (currentGems < amount)
            {
                Debug.LogWarning($"[GemsManager] Not enough gems! Have {currentGems}, need {amount}");
                return false;
            }
            
            currentGems -= amount;
            
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SpendGems(amount);
            }
            
            OnGemsChanged?.Invoke(currentGems);
            Debug.Log($"[GemsManager] Spent {amount} gems. Remaining: {currentGems}");
            return true;
        }
        
        /// <summary>
        /// Sets gems directly
        /// </summary>
        public void SetGems(int amount)
        {
            currentGems = Mathf.Max(0, amount);
            
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SetTotalGems(amount);
            }
            
            OnGemsChanged?.Invoke(currentGems);
        }
    }
}

