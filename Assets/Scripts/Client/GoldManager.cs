using UnityEngine;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Events;
using System;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages player gold - awards gold for kills and tracks total gold
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        private static GoldManager _instance;
        public static GoldManager Instance => _instance;
        
        private int currentGold = 0;
        public int CurrentGold => currentGold;
        
        [Header("Gold Settings")]
        [SerializeField] private int goldPerEnemy = 10;
        [SerializeField] private int goldPerMiniBoss = 50;
        [SerializeField] private int goldPerBoss = 100;
        
        public event Action<int> OnGoldChanged;
        
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Load gold from PlayerDataManager if available
            if (PlayerDataManager.Instance != null)
            {
                currentGold = PlayerDataManager.Instance.TotalGold;
                Debug.Log($"[GoldManager] Loaded {currentGold} gold from PlayerDataManager");
            }
            else
            {
                Debug.Log("[GoldManager] Awake - Instance created (PlayerDataManager not available yet)");
            }
        }
        
        void Start()
        {
            // Ensure we subscribe - unsubscribe first to avoid duplicates
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            
            // Sync with PlayerDataManager in Start (after all Awake() calls)
            SyncWithPlayerData();
            
            Debug.Log($"[GoldManager] Subscribed to EnemyKilledEvent in Start. Current gold: {currentGold}");
        }
        
        /// <summary>
        /// Syncs gold with PlayerDataManager (called when PlayerDataManager initializes)
        /// </summary>
        private void SyncWithPlayerData()
        {
            if (PlayerDataManager.Instance != null)
            {
                int savedGold = PlayerDataManager.Instance.TotalGold;
                if (savedGold != currentGold)
                {
                    // Use saved gold if it's higher (in case gold was earned in previous session)
                    if (savedGold > currentGold)
                    {
                        currentGold = savedGold;
                        OnGoldChanged?.Invoke(currentGold);
                        Debug.Log($"[GoldManager] Synced gold with PlayerDataManager: {currentGold}");
                    }
                    else
                    {
                        // Update PlayerDataManager with current gold
                        PlayerDataManager.Instance.SetTotalGold(currentGold);
                    }
                }
            }
        }
        
        void OnEnable()
        {
            // Subscribe when enabled
            if (_instance == this)
            {
                EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
                EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            }
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }
        
        private void OnEnemyKilled(ISimulationEvent evt)
        {
            if (evt is EnemyKilledEvent killEvent)
            {
                // Use enemy data from event (enemy might be removed from world already)
                int goldReward = 0;
                if (killEvent.IsBoss)
                {
                    goldReward = goldPerBoss;
                }
                else if (killEvent.IsMiniBoss)
                {
                    goldReward = goldPerMiniBoss;
                }
                else
                {
                    goldReward = goldPerEnemy;
                }
                
                AddGold(goldReward);
            }
        }
        
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
            
            // Sync with PlayerDataManager for persistence
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddGold(amount);
            }
            
        }
        
        public bool SpendGold(int amount)
        {
            if (currentGold < amount)
            {
                Debug.LogWarning($"[GoldManager] Not enough gold! Have {currentGold}, need {amount}");
                return false;
            }
            
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            
            // Sync with PlayerDataManager for persistence
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SetTotalGold(currentGold);
            }
            
            Debug.Log($"[GoldManager] Gold: {currentGold} (-{amount})");
            return true;
        }
        
        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(currentGold);
            
            // Sync with PlayerDataManager for persistence
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SetTotalGold(currentGold);
            }
        }
        
        public void ResetGold()
        {
            SetGold(0);
        }
    }
}

