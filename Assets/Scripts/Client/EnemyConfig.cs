using UnityEngine;
using ArenaGame.Shared.Math;

namespace ArenaGame.Client
{
    /// <summary>
    /// ScriptableObject configuration for enemies
    /// Contains enemy stats and visual prefab reference
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "ArenaGame/Enemy Config", order = 3)]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("Enemy Info")]
        [Tooltip("Enemy type name (e.g., 'BasicGrunt', 'FastRunner', 'Tank')")]
        public string enemyTypeName = "BasicGrunt";
        
        [Tooltip("Display name for the enemy")]
        public string displayName = "";
        
        [Tooltip("Description of the enemy")]
        [TextArea(2, 4)]
        public string description = "";
        
        [Header("Combat Stats")]
        [Tooltip("Maximum health")]
        public float maxHealth = 30f;
        
        [Tooltip("Base damage")]
        public float damage = 5f;
        
        [Tooltip("Move speed (units per second)")]
        public float moveSpeed = 2f;
        
        [Tooltip("Attack range (units)")]
        public float attackRange = 1.0f;
        
        [Tooltip("Attack speed (attacks per second)")]
        public float attackSpeed = 0.8f;
        
        [Header("Enemy Type")]
        [Tooltip("Is this a boss enemy?")]
        public bool isBoss = false;
        
        [Tooltip("Is this a mini-boss enemy?")]
        public bool isMiniBoss = false;
        
        [Header("Visual")]
        [Tooltip("Enemy prefab to instantiate (from monster asset pack)")]
        public GameObject enemyPrefab;
        
        [Tooltip("Enemy color (for UI/visuals)")]
        public Color enemyColor = Color.red;
        
        /// <summary>
        /// Validates config
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(enemyTypeName))
            {
                Debug.LogWarning($"[EnemyConfig] Enemy config '{name}' is missing enemy type name!");
                return false;
            }
            return true;
        }
    }
}

