using UnityEngine;
using ArenaGame.Shared.Core;

namespace ArenaGame.Client
{
    /// <summary>
    /// Unity implementation of IGameLogger
    /// </summary>
    public class UnityLogger : IGameLogger
    {
        public void Log(string message)
        {
            Debug.Log(message);
        }
        
        public void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }
        
        public void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}

