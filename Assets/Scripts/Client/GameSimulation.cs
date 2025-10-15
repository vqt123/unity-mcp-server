using UnityEngine;
using System.Collections.Generic;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Commands;

namespace ArenaGame.Client
{
    /// <summary>
    /// Runs the shared simulation and dispatches events
    /// </summary>
    public class GameSimulation : MonoBehaviour
    {
        public static GameSimulation Instance { get; private set; }
        
        [Header("Simulation")]
        [SerializeField] private int ticksPerSecond = 30;
        
        private Simulation simulation;
        private float tickInterval;
        private float tickAccumulator;
        private List<ISimulationCommand> pendingCommands = new List<ISimulationCommand>();
        
        public Simulation Simulation => simulation;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            simulation = new Simulation();
            tickInterval = 1f / ticksPerSecond;
            tickAccumulator = 0f;
        }
        
        void Update()
        {
            tickAccumulator += Time.deltaTime;
            
            while (tickAccumulator >= tickInterval)
            {
                TickSimulation();
                tickAccumulator -= tickInterval;
            }
        }
        
        private void TickSimulation()
        {
            // Run simulation
            List<ISimulationEvent> events = simulation.Tick(pendingCommands);
            pendingCommands.Clear();
            
            // Dispatch events to visualizers
            foreach (var evt in events)
            {
                EventBus.Publish(evt);
            }
        }
        
        public void QueueCommand(ISimulationCommand command)
        {
            command.Tick = simulation.World.CurrentTick;
            pendingCommands.Add(command);
        }
    }
}

