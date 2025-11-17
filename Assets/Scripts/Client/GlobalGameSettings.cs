using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Global game settings used across all scenes.
    /// Central place to assign default models, animations, and other global parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalGameSettings", menuName = "ArenaGame/Global Game Settings", order = 1)]
    public class GlobalGameSettings : ScriptableObject
    {
        [Header("Default Hero Settings")]
        [Tooltip("Drag the FBX model here. This will be used for ALL heroes.")]
        public GameObject defaultHeroModel;
        
        [Header("Hero Animations")]
        [Tooltip("Idle animation clip (drag from the FBX asset)")]
        public AnimationClip heroIdleAnimation;
        
        [Tooltip("Walk animation clip (drag from the FBX asset)")]
        public AnimationClip heroWalkAnimation;
        
        [Tooltip("Fire/Attack animation clip (drag from the FBX asset)")]
        public AnimationClip heroFireAnimation;
        
        [Header("Default Enemy Settings")]
        [Tooltip("Drag the enemy model here. This will be used for ALL enemies.")]
        public GameObject defaultEnemyModel;
        
        [Header("Enemy Animations")]
        [Tooltip("Idle animation clip for enemies")]
        public AnimationClip enemyIdleAnimation;
        
        [Tooltip("Walk animation clip for enemies")]
        public AnimationClip enemyWalkAnimation;
        
        [Tooltip("Attack animation clip for enemies")]
        public AnimationClip enemyAttackAnimation;
        
        [Header("Other Global Settings")]
        [Tooltip("Add any other global parameters here as needed")]
        public float globalParameter1;
        public float globalParameter2;
        public Color globalColor = Color.white;
        
        /// <summary>
        /// Validates that hero settings are set
        /// </summary>
        public bool IsHeroSettingsValid()
        {
            if (defaultHeroModel == null)
            {
                Debug.LogWarning("[GlobalGameSettings] Default hero model is not assigned!");
                return false;
            }
            
            if (heroIdleAnimation == null)
            {
                Debug.LogWarning("[GlobalGameSettings] Hero idle animation is not assigned!");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Validates that enemy settings are set
        /// </summary>
        public bool IsEnemySettingsValid()
        {
            if (defaultEnemyModel == null)
            {
                Debug.LogWarning("[GlobalGameSettings] Default enemy model is not assigned!");
                return false;
            }
            
            return true;
        }
    }
}

