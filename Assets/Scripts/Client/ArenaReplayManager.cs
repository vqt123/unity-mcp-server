using System.IO;
using UnityEngine;
using System.Collections.Generic;
using ArenaGame.Shared.Commands;

namespace ArenaGame.Client
{
    /// <summary>
    /// Manages saving and loading arena replay data
    /// </summary>
    public static class ArenaReplayManager
    {
        private static string ReplayFilePath => 
            Path.Combine(Application.persistentDataPath, "PlayerData", "last_arena_replay.json");
        
        /// <summary>
        /// Save replay data to file
        /// </summary>
        public static void SaveReplay(List<ISimulationCommand> commands)
        {
            try
            {
                var replayData = ArenaReplayData.FromCommands(commands);
                string json = JsonUtility.ToJson(replayData, true);
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(ReplayFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(ReplayFilePath, json);
                Debug.Log($"[Replay] Saved replay with {commands.Count} commands to {ReplayFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Replay] Failed to save replay: {e}");
            }
        }
        
        /// <summary>
        /// Load replay data from file
        /// </summary>
        public static List<ISimulationCommand> LoadReplay()
        {
            try
            {
                if (!File.Exists(ReplayFilePath))
                {
                    Debug.LogWarning("[Replay] No replay file found");
                    return null;
                }
                
                string json = File.ReadAllText(ReplayFilePath);
                var replayData = JsonUtility.FromJson<ArenaReplayData>(json);
                
                if (replayData == null || replayData.commands == null || replayData.commands.Count == 0)
                {
                    Debug.LogWarning("[Replay] Replay file is empty or invalid");
                    return null;
                }
                
                var commands = replayData.ToCommands();
                Debug.Log($"[Replay] Loaded replay with {commands.Count} commands");
                return commands;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Replay] Failed to load replay: {e}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if replay file exists
        /// </summary>
        public static bool HasReplay()
        {
            return File.Exists(ReplayFilePath);
        }
    }
}
