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
        /// Save replay data to file (with optional state hashes for divergence detection)
        /// </summary>
        public static void SaveReplay(List<ISimulationCommand> commands, Dictionary<int, string> stateHashes = null)
        {
            try
            {
                var replayData = ArenaReplayData.FromCommands(commands, stateHashes);
                string json = JsonUtility.ToJson(replayData, true);
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(ReplayFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(ReplayFilePath, json);
                Debug.Log($"[Replay] Saved replay with {commands.Count} commands and {stateHashes?.Count ?? 0} state hashes to {ReplayFilePath}");
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
            var (commands, _) = LoadReplayWithHashes();
            return commands;
        }
        
        /// <summary>
        /// Load replay data with state hashes from file
        /// </summary>
        public static (List<ISimulationCommand> commands, Dictionary<int, string> stateHashes) LoadReplayWithHashes()
        {
            try
            {
                if (!File.Exists(ReplayFilePath))
                {
                    Debug.LogWarning("[Replay] No replay file found");
                    return (null, null);
                }
                
                string json = File.ReadAllText(ReplayFilePath);
                var replayData = JsonUtility.FromJson<ArenaReplayData>(json);
                
                if (replayData == null || replayData.commands == null || replayData.commands.Count == 0)
                {
                    Debug.LogWarning("[Replay] Replay file is empty or invalid");
                    return (null, null);
                }
                
                var commands = replayData.ToCommands();
                var stateHashes = replayData.stateHashes?.ToDictionary() ?? new Dictionary<int, string>();
                
                Debug.Log($"[Replay] Loaded replay with {commands.Count} commands and {stateHashes.Count} state hashes");
                return (commands, stateHashes);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Replay] Failed to load replay: {e}");
                return (null, null);
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
