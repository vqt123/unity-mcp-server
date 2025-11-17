using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Explicit animation assignments for a hero character
    /// Assign animations directly from the FBX file - no name matching needed
    /// </summary>
    [CreateAssetMenu(fileName = "NewHeroAnimationConfig", menuName = "ArenaGame/Hero Animation Config", order = 3)]
    public class HeroAnimationConfig : ScriptableObject
    {
        [Header("Character Info")]
        [Tooltip("Character FBX file name (e.g., 'Elf', 'Wizard', 'Knight_Male')")]
        public string characterName = "";
        
        [Header("Animation Assignments")]
        [Tooltip("Idle animation clip from the character's FBX")]
        public AnimationClip idleAnimation;
        
        [Tooltip("Walk animation clip from the character's FBX")]
        public AnimationClip walkAnimation;
        
        [Tooltip("Fire/Attack animation clip from the character's FBX")]
        public AnimationClip fireAnimation;
        
        /// <summary>
        /// Validates that all required animations are assigned
        /// </summary>
        public bool IsValid()
        {
            if (idleAnimation == null)
            {
                Debug.LogWarning($"[HeroAnimationConfig] {characterName} is missing Idle animation!");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Gets the number of assigned animations
        /// </summary>
        public int GetAssignedCount()
        {
            int count = 0;
            if (idleAnimation != null) count++;
            if (walkAnimation != null) count++;
            if (fireAnimation != null) count++;
            return count;
        }
    }
}


