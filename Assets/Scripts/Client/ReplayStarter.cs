using System.Collections.Generic;
using ArenaGame.Shared.Commands;

namespace ArenaGame.Client
{
    /// <summary>
    /// Helper to pass replay commands from home menu to arena scene
    /// </summary>
    public static class ReplayStarter
    {
        private static List<ISimulationCommand> pendingReplayCommands = null;
        private static Dictionary<int, string> pendingReplayHashes = null;
        
        public static void SetReplayCommands(List<ISimulationCommand> commands, Dictionary<int, string> stateHashes = null)
        {
            pendingReplayCommands = commands;
            pendingReplayHashes = stateHashes;
        }
        
        public static List<ISimulationCommand> GetAndClearReplayCommands()
        {
            var commands = pendingReplayCommands;
            pendingReplayCommands = null;
            return commands;
        }
        
        public static Dictionary<int, string> GetAndClearReplayHashes()
        {
            var hashes = pendingReplayHashes;
            pendingReplayHashes = null;
            return hashes;
        }
    }
}
