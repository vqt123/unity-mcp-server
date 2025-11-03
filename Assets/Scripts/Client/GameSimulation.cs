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
        private int replayStopTick = -1; // Stop replay at this tick
        private int lastDivergenceCheckLogTick = -1; // Track last logged divergence check
        private bool replayFinished = false; // Track if replay has completed
        
        public Simulation Simulation => simulation;
        public float TickAccumulator => tickAccumulator;
        public float TickInterval => tickInterval;
        public int GetCurrentTick() => simulation.World.CurrentTick;
        
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
            // Don't process ticks if replay has finished
            if (replayFinished)
            {
                return;
            }
            
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
                
                // During replay, ensure first hero is level 1 before processing upgrade commands
                // (In original, hero was leveled up after spawn but before first upgrade)
                foreach (var cmd in pendingCommands)
                {
                    if (cmd is ArenaGame.Shared.Commands.ChooseUpgradeCommand upgradeCmd)
                    {
                        var world = simulation.World;
                        if (world.TryGetHero(upgradeCmd.HeroId, out ArenaGame.Shared.Entities.Hero hero))
                        {
                            if (hero.Level == 0)
                            {
                                Debug.Log($"[GameSimulation] REPLAY: Auto-leveling hero {upgradeCmd.HeroId.Value} from 0 to 1 before processing upgrade command");
                                hero.Level = 1;
                                hero.CurrentXP = 0;
                                world.UpdateHero(upgradeCmd.HeroId, hero);
                            }
                        }
                    }
                }
            }
            
            // Run simulation
            List<ISimulationEvent> events = simulation.Tick(pendingCommands);
            
            // Check if we should stop replay AFTER processing this tick
            int newTick = simulation.World.CurrentTick;
            // stopTick is the last tick that was processed in the original game
            // After processing tick N, CurrentTick becomes N+1
            // So we stop when CurrentTick > stopTick (i.e., we've processed past the last tick)
            if (isReplaying && replayStopTick >= 0 && newTick > replayStopTick)
            {
                Debug.Log($"[GameSimulation] REPLAY STOPPED at tick {newTick} (recorded stop tick: {replayStopTick}, last processed: tick {replayStopTick})");
                StopReplay();
                replayFinished = true;
                Time.timeScale = 0f; // Pause the game when replay finishes
                Debug.Log("[GameSimulation] Game paused - replay completed");
                return; // Don't process any more ticks
            }
            
            // Compute state hash for divergence detection
            string currentHash = ArenaGame.Shared.Core.SimulationStateHash.ComputeHash(simulation.World);
            
            // Check if this is the last tick we should process
            if (isReplaying && replayStopTick > 0 && newTick == replayStopTick)
            {
                // This is the final tick - do divergence check but will stop after this
                if (checkDivergence && recordedStateHashes.TryGetValue(currentTick, out string recordedHash))
                {
                    if (currentHash != recordedHash)
                    {
                        Debug.LogError($"[GameSimulation] REPLAY DIVERGENCE at final tick {currentTick}! Recorded: {recordedHash}, Current: {currentHash}");
                    }
                    else
                    {
                        Debug.Log($"[GameSimulation] ✓ Final tick {currentTick} hash match: {currentHash.Substring(0, 8)}...");
                    }
                }
            }
            
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
                    else
                    {
                        // Log success periodically (every 50 ticks) to show checking is working
                        if (currentTick - lastDivergenceCheckLogTick >= 50 || lastDivergenceCheckLogTick < 0)
                        {
                            Debug.Log($"[GameSimulation] ✓ Hash match at tick {currentTick}: {currentHash.Substring(0, 8)}... (divergence check active)");
                            lastDivergenceCheckLogTick = currentTick;
                        }
                    }
                }
                else if (currentTick % 50 == 0)
                {
                    // Log when no hash recorded for this tick (expected for some ticks)
                    Debug.Log($"[GameSimulation] No recorded hash for tick {currentTick} (expected if not all ticks were hashed)");
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
        public void StartReplay(List<ISimulationCommand> commands, Dictionary<int, string> stateHashes = null, int stopTick = -1)
        {
            isReplaying = true;
            replayFinished = false; // Reset finished flag
            replayCommands = new List<ISimulationCommand>(commands);
            replayCommandIndex = 0;
            replayStopTick = stopTick;
            Time.timeScale = 1f; // Ensure game is running during replay
            
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
            
            Debug.Log($"[GameSimulation] Started replay with {commands.Count} commands, divergence checking: {checkDivergence}, stopTick: {stopTick}");
        }
        
        /// <summary>
        /// Stop replay
        /// </summary>
        public void StopReplay()
        {
            isReplaying = false;
            replayFinished = true;
            replayCommands = null;
            replayCommandIndex = 0;
            Debug.Log("[GameSimulation] Replay stopped, simulation will no longer process ticks");
        }
        
        /// <summary>
        /// Check if currently replaying
        /// </summary>
        public bool IsReplaying => isReplaying;
    }
}

