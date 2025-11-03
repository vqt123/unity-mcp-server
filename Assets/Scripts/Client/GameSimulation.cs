using UnityEngine;
using System.Collections.Generic;
using ArenaGame.Shared.Core;
using ArenaGame.Shared.Commands;

namespace ArenaGame.Client
{
    /// <summary>
    /// Runs the shared simulation and dispatches events
    /// This is the SINGLE EVENT FUNNEL - all simulation events flow through TickSimulation()
    /// </summary>
    public class GameSimulation : MonoBehaviour
    {
        public static GameSimulation Instance { get; private set; }
        
        [Header("Simulation")]
        [SerializeField] private int ticksPerSecond = 10;
        
        private Simulation simulation;
        private float tickInterval;
        private float tickAccumulator;
        private List<ISimulationCommand> pendingCommands = new List<ISimulationCommand>();
        
        // Replay system
        private List<ISimulationCommand> recordedCommands = new List<ISimulationCommand>();
        private bool isRecording = false;
        private bool isReplaying = false;
        private List<ISimulationCommand> replayCommands = null;
        private int replayCommandIndex = 0;
        
        public Simulation Simulation => simulation;
        public float TickAccumulator => tickAccumulator;
        public float TickInterval => tickInterval;
        
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
        
        /// <summary>
        /// SINGLE EVENT FUNNEL - All simulation events flow through here
        /// Simulation.Tick() generates events, this method dispatches them to EventBus
        /// Client code subscribes via EventBus - no direct simulation access
        /// </summary>
        private void TickSimulation()
        {
            int currentTick = simulation.World.CurrentTick;
            
            // Log pending commands
            if (pendingCommands.Count > 0)
            {
                Debug.Log($"[GameSimulation] Tick {currentTick}: Processing {pendingCommands.Count} commands");
                foreach (var cmd in pendingCommands)
                {
                    Debug.Log($"[GameSimulation]   - Command: {cmd.GetType().Name}, Tick: {cmd.Tick}");
                }
            }
            
            // If replaying, inject commands that should execute this tick
            // Commands are injected when currentTick >= recorded tick
            if (isReplaying && replayCommands != null)
            {
                while (replayCommandIndex < replayCommands.Count)
                {
                    var cmd = replayCommands[replayCommandIndex];
                    // Inject commands that were recorded at this tick or earlier
                    if (cmd.Tick <= currentTick)
                    {
                        pendingCommands.Add(cmd);
                        replayCommandIndex++;
                    }
                    else
                    {
                        break; // Wait for the tick this command was recorded at
                    }
                }
            }
            
            // Run simulation
            List<ISimulationEvent> events = simulation.Tick(pendingCommands);
            
            // Log events generated
            if (events.Count > 0)
            {
                Debug.Log($"[GameSimulation] Tick {currentTick}: Generated {events.Count} events");
                foreach (var evt in events)
                {
                    Debug.Log($"[GameSimulation]   - Event: {evt.GetType().Name}, Tick: {evt.Tick}");
                    if (evt is Shared.Events.HeroSpawnedEvent heroSpawn)
                    {
                        Debug.Log($"[GameSimulation]     HeroSpawnedEvent - HeroId: {heroSpawn.HeroId.Value}, HeroType: {heroSpawn.HeroType}");
                    }
                }
            }
            
            // Record commands before clearing
            if (isRecording)
            {
                foreach (var cmd in pendingCommands)
                {
                    recordedCommands.Add(cmd);
                }
            }
            
            pendingCommands.Clear();
            
            // Dispatch events to visualizers via EventBus (SINGLE FUNNEL POINT)
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
        
        /// <summary>
        /// Start recording commands for replay
        /// </summary>
        public void StartRecording()
        {
            isRecording = true;
            recordedCommands.Clear();
        }
        
        /// <summary>
        /// Stop recording and return recorded commands
        /// </summary>
        public List<ISimulationCommand> StopRecording()
        {
            isRecording = false;
            return new List<ISimulationCommand>(recordedCommands);
        }
        
        /// <summary>
        /// Start replaying from saved commands
        /// </summary>
        public void StartReplay(List<ISimulationCommand> commands)
        {
            isReplaying = true;
            replayCommands = new List<ISimulationCommand>(commands);
            replayCommandIndex = 0;
            
            // Reset simulation for clean replay
            simulation.Reset();
        }
        
        /// <summary>
        /// Stop replay
        /// </summary>
        public void StopReplay()
        {
            isReplaying = false;
            replayCommands = null;
            replayCommandIndex = 0;
        }
        
        /// <summary>
        /// Check if currently replaying
        /// </summary>
        public bool IsReplaying => isReplaying;
    }
}

