using ArenaGame.Shared.Commands;
using ArenaGame.Shared.Systems;
using System.Collections.Generic;

namespace ArenaGame.Shared.Core
{
    /// <summary>
    /// Main simulation orchestrator - runs all systems in deterministic order
    /// </summary>
    public class Simulation
    {
        private SimulationWorld world;
        private CommandProcessor commandProcessor;
        
        public Simulation()
        {
            world = new SimulationWorld();
            commandProcessor = new CommandProcessor(world);
        }
        
        public SimulationWorld World => world;
        
        /// <summary>
        /// Run one simulation tick (1/30 second)
        /// This must be completely deterministic!
        /// </summary>
        /// <param name="commands">Commands to process this tick</param>
        /// <returns>Events generated this tick</returns>
        public List<ISimulationEvent> Tick(List<ISimulationCommand> commands = null)
        {
            // 0. Process commands (player inputs, server spawns, etc)
            if (commands != null && commands.Count > 0)
            {
                commandProcessor.ProcessCommands(commands);
            }
            
            // 1. AI decides what enemies do
            AISystem.UpdateEnemies(world);
            
            // 2. Heroes auto-shoot at enemies
            CombatSystem.ProcessHeroShooting(world);
            
            // 3. Process movement
            MovementSystem.UpdateHeroes(world);
            MovementSystem.UpdateEnemies(world);
            MovementSystem.UpdateProjectiles(world);
            
            // 4. Process combat (collisions and enemy attacks)
            CombatSystem.ProcessCollisions(world);
            CombatSystem.ProcessEnemyAttacks(world);
            
            // 5. Advance tick counter and get events
            world.Tick();
            
            // 6. Return events for this tick
            return world.GetAndClearEvents();
        }
    }
}

