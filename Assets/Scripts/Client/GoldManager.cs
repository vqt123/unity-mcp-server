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
            Debug.Log("[GoldManager] Awake - Instance created");
        }
        
        void Start()
        {
            // Ensure we subscribe - unsubscribe first to avoid duplicates
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            Debug.Log($"[GoldManager] Subscribed to EnemyKilledEvent in Start. Current gold: {currentGold}");
        }
        
        void OnEnable()
        {
            // Subscribe when enabled
            if (_instance == this)
            {
                EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
                EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
                Debug.Log("[GoldManager] Subscribed to EnemyKilledEvent in OnEnable");
            }
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }
        
        private void OnEnemyKilled(ISimulationEvent evt)
        {
            Debug.Log($"[GoldManager] OnEnemyKilled called with event type: {evt.GetType().Name}");
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
                Debug.Log($"[GoldManager] Awarded {goldReward} gold for killing enemy (Boss:{killEvent.IsBoss}, MiniBoss:{killEvent.IsMiniBoss})");
            }
            else
            {
                Debug.LogWarning($"[GoldManager] Event is not EnemyKilledEvent, it's {evt?.GetType().Name}");
            }
        }
        
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log($"[GoldManager] Gold: {currentGold} (+{amount})");
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
            Debug.Log($"[GoldManager] Gold: {currentGold} (-{amount})");
            return true;
        }
        
        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(currentGold);
        }
        
        public void ResetGold()
        {
            SetGold(0);
        }
    }
}

