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
        public List<HeroProgressEntry> heroProgressList = new List<HeroProgressEntry>();
        
        /// <summary>
        /// Creates default player blob with single archer hero card
        /// </summary>
        public static PlayerBlob CreateDefault()
        {
            var blob = new PlayerBlob();
            
            // Start with only Archer unlocked and in party
            blob.heroInventory.unlockedHeroes = new List<string> { "Archer" };
            blob.heroInventory.partyHeroes = new List<string> { "Archer" };
            
            blob.totalGold = 0;
            
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

