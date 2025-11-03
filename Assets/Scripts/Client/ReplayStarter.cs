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
        
        public static void SetReplayCommands(List<ISimulationCommand> commands)
        {
            pendingReplayCommands = commands;
        }
        
        public static List<ISimulationCommand> GetAndClearReplayCommands()
        {
            var commands = pendingReplayCommands;
            pendingReplayCommands = null;
            return commands;
        }
    }
}
