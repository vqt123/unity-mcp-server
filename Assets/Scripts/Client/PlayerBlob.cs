using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Serializable blob containing all player persistent data
    /// This is saved/loaded as JSON to the file system
    /// </summary>
    [Serializable]
    public class PlayerBlob
    {
        public HeroInventoryData heroInventory = new HeroInventoryData();
        public int totalGold = 0;
        public int totalGems = 0;
        public List<HeroProgressEntry> heroProgressList = new List<HeroProgressEntry>();
        
        // Arena progression
        public int playerLevel = 0; // Player level (starts at 0, levels to 1 on arena entry)
        public int upgradesChosenAtLevel1 = 0; // Track upgrades chosen at level 1 (0-2)
        
        // Energy system
        public int currentEnergy = 30;
        public int maxEnergy = 30;
        public double lastEnergyRegenTime = 0; // Unix timestamp of last regeneration check
        
        /// <summary>
        /// Creates default player blob with Archer, Ice Archer, and Mage hero cards
        /// </summary>
        public static PlayerBlob CreateDefault()
        {
            var blob = new PlayerBlob();
            
            // Start with Archer, Ice Archer, and Mage unlocked
            blob.heroInventory.unlockedHeroes = new List<string> { "Archer", "IceArcher", "Mage" };
            // Start with all three in party
            blob.heroInventory.partyHeroes = new List<string> { "Archer", "IceArcher", "Mage" };
            
            blob.totalGold = 0;
            blob.totalGems = 0;
            blob.playerLevel = 0;
            blob.upgradesChosenAtLevel1 = 0;
            
            // Start with full energy
            blob.currentEnergy = 30;
            blob.maxEnergy = 30;
            blob.lastEnergyRegenTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            return blob;
        }
        
        /// <summary>
        /// Gets hero progress data, creating if it doesn't exist
        /// </summary>
        public HeroProgressData GetHeroProgress(string heroType)
        {
            var entry = heroProgressList.Find(e => e.heroType == heroType);
            if (entry == null)
            {
                entry = new HeroProgressEntry
                {
                    heroType = heroType,
                    progress = new HeroProgressData()
                };
                heroProgressList.Add(entry);
            }
            return entry.progress;
        }
        
        /// <summary>
        /// Sets hero progress data
        /// </summary>
        public void SetHeroProgress(string heroType, HeroProgressData progress)
        {
            var entry = heroProgressList.Find(e => e.heroType == heroType);
            if (entry == null)
            {
                entry = new HeroProgressEntry { heroType = heroType, progress = progress };
                heroProgressList.Add(entry);
            }
            else
            {
                entry.progress = progress;
            }
        }
    }
    
    /// <summary>
    /// Wrapper for Dictionary-like serialization (JsonUtility doesn't support Dictionary)
    /// </summary>
    [Serializable]
    public class HeroProgressEntry
    {
        public string heroType;
        public HeroProgressData progress;
    }
    
    /// <summary>
    /// Progress data for individual heroes (level, upgrades, etc.)
    /// </summary>
    [Serializable]
    public class HeroProgressData
    {
        public int level = 1;
        public int experience = 0;
        public List<StatUpgradeEntry> statUpgradesList = new List<StatUpgradeEntry>();
        
        /// <summary>
        /// Gets upgrade count for a stat
        /// </summary>
        public int GetUpgradeCount(string statName)
        {
            var entry = statUpgradesList.Find(e => e.statName == statName);
            return entry?.upgradeCount ?? 0;
        }
        
        /// <summary>
        /// Sets upgrade count for a stat
        /// </summary>
        public void SetUpgradeCount(string statName, int count)
        {
            var entry = statUpgradesList.Find(e => e.statName == statName);
            if (entry == null)
            {
                entry = new StatUpgradeEntry { statName = statName, upgradeCount = count };
                statUpgradesList.Add(entry);
            }
            else
            {
                entry.upgradeCount = count;
            }
        }
    }
    
    /// <summary>
    /// Wrapper for stat upgrade dictionary serialization
    /// </summary>
    [Serializable]
    public class StatUpgradeEntry
    {
        public string statName;
        public int upgradeCount;
    }
}

