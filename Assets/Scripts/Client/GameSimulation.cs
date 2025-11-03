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
        
        // Divergence detection
        private Dictionary<int, string> recordedStateHashes = new Dictionary<int, string>();
        private bool checkDivergence = false;
        
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
                        Debug.Log($"[GameSimulation] REPLAY: Injecting command {cmd.GetType().Name} at tick {currentTick} (was recorded at tick {cmd.Tick})");
                        pendingCommands.Add(cmd);
                        replayCommandIndex++;
                    }
                    else
                    {
                        break; // Wait for the tick this command was recorded at
                    }
                }
            }
            
            if (pendingCommands.Count > 0 && isReplaying)
            {
                Debug.Log($"[GameSimulation] REPLAY: Tick {currentTick} - Processing {pendingCommands.Count} commands from replay");
            }
            
            // Run simulation
            List<ISimulationEvent> events = simulation.Tick(pendingCommands);
            
            // Compute state hash for divergence detection
            string currentHash = ArenaGame.Shared.Core.SimulationStateHash.ComputeHash(simulation.World);
            
            if (isRecording)
            {
                // Record state hash
                recordedStateHashes[currentTick] = currentHash;
                
                // Record commands before clearing
                foreach (var cmd in pendingCommands)
                {
                    recordedCommands.Add(cmd);
                }
            }
            else if (isReplaying && checkDivergence)
            {
                // Check for divergence
                if (recordedStateHashes.TryGetValue(currentTick, out string recordedHash))
                {
                    if (currentHash != recordedHash)
                    {
                        Debug.LogError($"[GameSimulation] REPLAY DIVERGENCE at tick {currentTick}! Recorded: {recordedHash}, Current: {currentHash}");
                        // Optionally stop replay or continue logging
                    }
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
            recordedStateHashes.Clear();
            Debug.Log("[GameSimulation] Started recording with state hash tracking");
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
        /// Get recorded state hashes (for saving with replay)
        /// </summary>
        public Dictionary<int, string> GetRecordedStateHashes()
        {
            return new Dictionary<int, string>(recordedStateHashes);
        }
        
        /// <summary>
        /// Start replaying from saved commands
        /// </summary>
        public void StartReplay(List<ISimulationCommand> commands, Dictionary<int, string> stateHashes = null)
        {
            isReplaying = true;
            replayCommands = new List<ISimulationCommand>(commands);
            replayCommandIndex = 0;
            
            // Load state hashes for divergence checking
            if (stateHashes != null && stateHashes.Count > 0)
            {
                recordedStateHashes = new Dictionary<int, string>(stateHashes);
                checkDivergence = true;
                Debug.Log($"[GameSimulation] Loaded {stateHashes.Count} state hashes for divergence checking");
            }
            else
            {
                checkDivergence = false;
                Debug.Log("[GameSimulation] No state hashes provided - divergence checking disabled");
            }
            
            // Reset simulation for clean replay
            simulation.Reset();
            
            Debug.Log($"[GameSimulation] Started replay with {commands.Count} commands, divergence checking: {checkDivergence}");
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

