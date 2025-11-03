using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArenaGame.Client
{
    /// <summary>
    /// Persistent data for player's hero collection and party
    /// </summary>
    [Serializable]
    public class HeroInventoryData
    {
        public List<string> unlockedHeroes = new List<string>();
        public List<string> partyHeroes = new List<string>();
        public const int MAX_PARTY_SIZE = 5;
        
        public static HeroInventoryData CreateDefault()
        {
            var data = new HeroInventoryData();
            
            // Start with Archer, Ice Archer, and Mage unlocked
            data.unlockedHeroes = new List<string> { "Archer", "IceArcher", "Mage" };
            
            // Start with all three in party
            data.partyHeroes = new List<string> { "Archer", "IceArcher", "Mage" };
            
            return data;
        }
        
        public bool IsInParty(string heroType)
        {
            return partyHeroes.Contains(heroType);
        }
        
        public bool CanAddToParty()
        {
            return partyHeroes.Count < MAX_PARTY_SIZE;
        }
        
        public bool AddToParty(string heroType)
        {
            if (!unlockedHeroes.Contains(heroType))
            {
                Debug.LogWarning($"Hero {heroType} is not unlocked");
                return false;
            }
            
            if (partyHeroes.Contains(heroType))
            {
                Debug.LogWarning($"Hero {heroType} is already in party");
                return false;
            }
            
            if (partyHeroes.Count >= MAX_PARTY_SIZE)
            {
                Debug.LogWarning($"Party is full ({MAX_PARTY_SIZE} max)");
                return false;
            }
            
            partyHeroes.Add(heroType);
            return true;
        }
        
        public bool RemoveFromParty(string heroType)
        {
            if (partyHeroes.Count <= 1)
            {
                Debug.LogWarning("Cannot remove last hero from party");
                return false;
            }
            
            return partyHeroes.Remove(heroType);
        }
    }
}

